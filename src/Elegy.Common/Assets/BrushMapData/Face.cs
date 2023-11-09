// SPDX-FileCopyrightText: 2023 Admer Šuko
// SPDX-License-Identifier: MIT

using Elegy.Geometry;

namespace Elegy.Assets.BrushMapData
{
	public class Face
	{
		public Vector3[] PlaneDefinition = new Vector3[3];
		public Plane Plane = new();

		public string MaterialName = string.Empty;
		// XYZ -> axis; W -> offset along axis
		public Vector4[] ProjectionUVS = new Vector4[2];
		public float Rotation = 0.0f;
		public Vector2 Scale = Vector2.One;

		// Filled in after parsing (fields above are during parsing)
		public Polygon3D Polygon = new();

		public Vector3 Centre => (PlaneDefinition[0] + PlaneDefinition[1] + PlaneDefinition[2]) / 3.0f;

		public Vector2 CalculateUV( Vector3 point, int imageWidth, int imageHeight )
		{
			Vector3 axisU = ProjectionUVS[0].ToVector3() * (1.0f / Scale.X);
			Vector3 axisV = ProjectionUVS[1].ToVector3() * (1.0f / Scale.Y);

			return new()
			{
				X = (point.Dot( axisU ) + ProjectionUVS[0].W) / imageWidth,
				Y = (point.Dot( axisV ) + ProjectionUVS[1].W) / imageHeight
			};
		}
	}
}
