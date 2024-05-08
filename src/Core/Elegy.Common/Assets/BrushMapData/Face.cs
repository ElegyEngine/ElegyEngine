// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Geometry;

namespace Elegy.Common.Assets.BrushMapData
{
	/// <summary></summary>
	public class Face
	{
		/// <summary></summary>
		public Vector3[] PlaneDefinition = new Vector3[3];
		/// <summary></summary>
		public Plane Plane = new();

		/// <summary></summary>
		public string MaterialName = string.Empty;
		/// <summary></summary>
		public Vector4[] ProjectionUVS = new Vector4[2]; // XYZ -> axis; W -> offset along axis
		/// <summary></summary>
		public float Rotation = 0.0f;
		/// <summary></summary>
		public Vector2 Scale = Vector2.One;

		// Filled in after parsing (fields above are during parsing)
		/// <summary></summary>
		public Polygon3 Polygon = new();

		/// <summary></summary>
		public Vector3 Centre => (PlaneDefinition[0] + PlaneDefinition[1] + PlaneDefinition[2]) / 3.0f;

		/// <summary></summary>
		public Vector2 CalculateUV( Vector3 point, uint imageWidth, uint imageHeight )
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
