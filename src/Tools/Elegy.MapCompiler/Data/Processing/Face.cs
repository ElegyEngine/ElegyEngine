// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;

namespace Elegy.MapCompiler.Data.Processing
{
	public class Face
	{
		public Vector3 Centre { get; set; } = Vector3.Zero;
		public List<Vertex> Vertices { get; set; } = new();
		public AssetSystem.Resources.Material Material { get; set; }
		public Plane Plane { get; set; }

		public Face( BrushMapFace face )
		{
			Material = AssetSystem.API.Assets.LoadMaterial( "materials/" + face.MaterialName ) ?? throw new ArgumentException($"Material \"{face.MaterialName}\" not found");
			TextureMetadata diffuseMetadata = AssetSystem.API.Assets.GetTextureMetadata( Material.Data.DiffuseMap ?? "") ?? AssetSystem.API.Assets.MissingTexture.Metadata;
			Centre = face.Polygon.Origin;

			for ( int i = 0; i < face.Polygon.Points.Count; i++ )
			{
				Vertices.Add( new()
				{
					Position = face.Polygon.Points[i],
					Normal = face.Plane.Normal,
					Uv = face.CalculateUV( face.Polygon.Points[i], diffuseMetadata.Width, diffuseMetadata.Height ),
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
