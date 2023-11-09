// SPDX-FileCopyrightText: 2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy.MapCompiler.Data.Processing
{
	public class Face
	{
		public Vector3 Centre { get; set; } = Vector3.Zero;
		public List<Vertex> Vertices { get; set; } = new();
		public Material Material { get; set; }
		public Plane Plane { get; set; }

		public Face( BrushMapFace face )
		{
			Material = MaterialSystem.LoadMaterial( $"materials/{face.MaterialName}" );
			Centre = face.Polygon.Origin;

			for ( int i = 0; i < face.Polygon.Points.Count; i++ )
			{
				Vertices.Add( new()
				{
					Position = face.Polygon.Points[i],
					Normal = face.Plane.Normal,
					Uv = face.CalculateUV( face.Polygon.Points[i], Material.Width, Material.Height ),
					Colour = Vector4.Zero
				} );
			}
		}

		public void Move( Vector3 offset )
		{
			for ( int i = 0; i < Vertices.Count; i++ )
			{
				Vertex v = Vertices[i];
				v.Position += offset;
				Vertices[i] = v;
			}
		}
	}
}
