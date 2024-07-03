// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;

namespace Elegy.MapCompiler.Data.Processing
{
	public class Entity
	{
		public Vector3 Centre = Vector3.Zero;
		public Box3 BoundingBox = new();
		public List<Face> Faces { get; set; } = new();

		public string ClassName = string.Empty;
		public Dictionary<string, string> Pairs = new();

		public Entity( BrushMapEntity entity )
		{
			Centre = entity.Centre;
			BoundingBox = entity.BoundingBox;
			ClassName = entity.ClassName;
			Pairs = new( entity.Pairs );

			Faces = entity.Brushes
				.SelectMany( Face.CreateFacesFromMapBrush )
				.ToList();
		}

		public bool IsWorld()
		{
			return ClassName == "worldspawn";
		}

		public bool IsPointEntity()
		{
			return Faces.Count == 0;
		}

		public bool HasMaterialFlag( ToolMaterialFlag flags )
		{
			for ( int i = 0; i < Faces.Count; i++ )
			{
				if ( Faces[i].Material.Data.ToolFlags.HasFlag( flags ) )
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Moves all vertex positions of this brush by <paramref name="offset"/>.
		/// </summary>
		public void MoveFaces( Vector3 offset )
		{
			Centre += offset;
			for ( int i = 0; i < Faces.Count; i++ )
			{
				Faces[i].Move( offset );
			}

			RegenerateBounds();
		}

		public void RegenerateBounds()
		{
			// Point entities are points after all
			if ( Faces.Count == 0 )
			{
				BoundingBox = new( Centre, Vector3.One * 0.001f );
				return;
			}

			BoundingBox = Box3.FromCorners( Faces[0].Vertices[0].Position, Faces[0].Vertices[1].Position );
			foreach ( var face in Faces )
			{
				foreach ( var vertex in face.Vertices )
				{
					BoundingBox = BoundingBox.Expand( vertex.Position );
				}
			}
		}
	}
}
