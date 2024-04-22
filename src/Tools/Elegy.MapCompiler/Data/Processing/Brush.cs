// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;

namespace Elegy.MapCompiler.Data.Processing
{
	public class Brush
	{
		public Vector3 Centre { get; set; } = Vector3.Zero;
		public Box3 BoundingBox { get; set; } = new();
		public List<Face> Faces { get; set; } = new();

		public Brush( BrushMapBrush mapBrush )
		{
			Faces = CreateFacesFromMapBrush( mapBrush );
			for ( int i = 0; i < Faces.Count; i++ )
			{
				Centre += Faces[i].Centre;
			}
			Centre /= Faces.Count;

			RegenerateBounds();
		}

		public List<Face> CreateFacesFromMapBrush( BrushMapBrush mapBrush )
		{
			List<Face> result = new();

			for ( int i = 0; i < mapBrush.Faces.Count; i++ )
			{
				// To be 100% explicit we're building Processing.Face from Map.Face
				BrushMapFace mapFace = mapBrush.Faces[i];
				result.Add( new( mapFace ) );
			}

			return result;
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

		public void RegenerateBounds()
		{
			BoundingBox = new( Centre, Vector3.One * 0.001f );
			for ( int i = 0; i < Faces.Count; i++ )
			{
				for ( int v = 0; v < Faces[i].Vertices.Count; v++ )
				{
					BoundingBox = BoundingBox.Expand( Faces[i].Vertices[v].Position );
				}
			}
		}

		/// <summary>
		/// Moves all vertex positions of this brush by <paramref name="offset"/>.
		/// </summary>
		public void Move( Vector3 offset )
		{
			Centre += offset;
			for ( int i = 0; i < Faces.Count; i++ )
			{
				Faces[i].Move( offset );
			}

			RegenerateBounds();
		}
	}
}
