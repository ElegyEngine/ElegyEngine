// SPDX-FileCopyrightText: 2023 Admer Šuko
// SPDX-License-Identifier: MIT

using Elegy.Assets.BrushMapData;
using Elegy.Text;
using Elegy.Utilities;

namespace Elegy.Assets
{
	public class BrushMapDocument
	{
		public const string Tag = "BrushMapDocument";

		public void MergeInto( string parentClass, string mergedClass )
		{
			Entity? worldspawn = MapEntities.Find( e => e.ClassName == parentClass );
			if ( worldspawn is null )
			{
				return;
			}

			for ( int i = 0; i < MapEntities.Count; i++ )
			{
				var entity = MapEntities[i];
				if ( entity.ClassName == mergedClass )
				{
					worldspawn.Brushes.AddRange( entity.Brushes );
					worldspawn.BoundingBox = worldspawn.BoundingBox.Merge( entity.BoundingBox );

					MapEntities.Remove( entity );
					i--;
				}
			}
		}

		public static Face ParseFace( Lexer lex )
		{
			// ( x1 y1 z1 ) ( x2 y2 z2 ) ( x3 y3 z3 ) texture_name [ ux uy uz offsetX ] [ vx vy vz offsetY ] rotation scaleX scaleY
			Face face = new();

			for ( int i = 0; i < 3; i++ )
			{
				// Eat the (
				if ( !lex.Expect( "(", true ) )
				{
					throw new Exception( $"Expected '(' {lex.GetLineInfo()}" );
				}

				face.PlaneDefinition[i].X = Parse.Float( lex.Next() );
				face.PlaneDefinition[i].Y = Parse.Float( lex.Next() );
				face.PlaneDefinition[i].Z = Parse.Float( lex.Next() );

				// Eat the )
				if ( !lex.Expect( ")", true ) )
				{
					throw new Exception( $"Expected ')' {lex.GetLineInfo()}" );
				}
			}

			face.Plane = new Plane( face.PlaneDefinition[0], face.PlaneDefinition[1], face.PlaneDefinition[2] );

			// We could potentially have slashes in here and all kinds of wacky characters
			lex.IgnoreDelimiters = true;
			face.MaterialName = lex.Next();
			lex.IgnoreDelimiters = false;

			if ( face.MaterialName == string.Empty )
			{
				throw new Exception( $"Texture or material is empty {lex.GetLineInfo()}" );
			}

			for ( int i = 0; i < 2; i++ )
			{
				if ( !lex.Expect( "[", true ) )
				{
					throw new Exception( $"Expected '[' {lex.GetLineInfo()}" );
				}

				string token = lex.Next();
				face.ProjectionUVS[i].X = Parse.Float( token );
				token = lex.Next();
				face.ProjectionUVS[i].Y = Parse.Float( token );
				token = lex.Next();
				face.ProjectionUVS[i].Z = Parse.Float( token );
				token = lex.Next();
				face.ProjectionUVS[i].W = Parse.Float( token );

				if ( !lex.Expect( "]", true ) )
				{
					throw new Exception( $"Expected ']' {lex.GetLineInfo()}" );
				}
			}

			face.Rotation = Parse.Float( lex.Next() );
			face.Scale.X = Parse.Float( lex.Next() );
			face.Scale.Y = Parse.Float( lex.Next() );

			// This is an ugly, hacky way to support Quake 3's blessed map format
			string nextToken = lex.Peek();
			while ( nextToken != "}" && nextToken != "(" )
			{
				lex.Next();
				nextToken = lex.Peek();
				if ( nextToken == "}" || nextToken == "(" )
				{
					break;
				}
			}

			return face;
		}

		public static Brush ParseBrush( Lexer lex )
		{
			Brush brush = new();

			// Eat the {
			lex.Next();

			while ( true )
			{
				if ( lex.IsEnd() )
				{
					throw new Exception( $"Unexpected EOF {lex.GetLineInfo()}" );
				}

				// Eat the }
				if ( lex.Expect( "}", true ) )
				{
					break;
				}
				// It's a map face
				else if ( lex.Expect( "(" ) )
				{
					brush.Faces.Add( ParseFace( lex ) );
				}
				// Forgor to add this
				else
				{
					throw new Exception( $"Unexpected token '{lex.Next()}' {lex.GetLineInfo()}" );
				}
			}

			// The bounding box is derived from plane definitions.
			// This is not really a reliable way to do it, because plane definitions could
			// be anywhere. We trust the map editor to do it "correctly".
			//
			// Said bbox is used as a reference point for the brush's local coordinate system,
			// while intersecting all planes to form usable polygons. This then gets refined
			// with coordinates of these polygons. Refer to ProcessingData.Processing.Brush for that.
			//
			// In other words, bounding boxes & centres in Data.Map.* are slightly inaccurate, however
			// they get very refined in Data.Processing.*
			brush.BoundingBox = new Aabb( brush.Faces[0].Centre, Vector3.One * 0.001f );
			brush.Faces.ForEach( face =>
			{
				for ( int i = 0; i < 3; i++ )
				{
					brush.BoundingBox = brush.BoundingBox.Expand( face.PlaneDefinition[i] );
				}
			} );

			return brush;
		}

		public static Entity ParseEntity( Lexer lex )
		{
			Entity entity = new();

			while ( true )
			{
				if ( lex.IsEnd() )
				{
					throw new Exception( $"Unexpected EOF {lex.GetLineInfo()}" );
				}

				// Closure of this entity
				if ( lex.Expect( "}", true ) )
				{
					break;
				}
				// New brush
				else if ( lex.Expect( "{" ) )
				{
					entity.Brushes.Add( ParseBrush( lex ) );
				}
				// Key-value pair
				else
				{
					string key = lex.Next();

					lex.IgnoreDelimiters = true;
					string value = lex.Next();
					lex.IgnoreDelimiters = false;

					entity.Pairs.Add( key, value );
				}
			}

			if ( entity.Pairs.TryGetValue( "classname", out string? className ) )
			{
				entity.ClassName = className;
			}
			else
			{
				entity.ClassName = string.Empty;
			}

			if ( entity.Pairs.TryGetValue( "origin", out string? originString ) )
			{
				entity.Centre = originString.ToVector3();
				entity.Pairs["origin"] = $"{entity.Centre.X} {entity.Centre.Y} {entity.Centre.Z}";
			}

			if ( entity.Brushes.Count > 0 )
			{
				entity.BoundingBox = entity.Brushes[0].BoundingBox;
				for ( int i = 1; i < entity.Brushes.Count; i++ )
				{
					entity.BoundingBox = entity.BoundingBox.Merge( entity.Brushes[i].BoundingBox );
				}
			}

			return entity;
		}

		public static BrushMapDocument? FromValve220MapFile( string path )
		{
			BrushMapDocument map = new();

			Lexer lex = new( File.ReadAllText( path ) );
			while ( !lex.IsEnd() )
			{
				string token = lex.Next();

				if ( token == "{" )
				{
					map.MapEntities.Add( ParseEntity( lex ) );
				}
				else if ( token == "}" || token == string.Empty )
				{
					break;
				}
				else
				{
					throw new Exception( $"Unknown token '{token}' {lex.GetLineInfo()}" );
				}
			}

			map.Valid = true;
			return map;
		}

		public string Title = "unknown";
		public string Description = "unknown";
		public List<Entity> MapEntities = new();
		public bool Valid = false;
	}
}
