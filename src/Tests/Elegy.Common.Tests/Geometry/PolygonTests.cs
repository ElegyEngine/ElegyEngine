// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Geometry;
using Elegy.Common.Maths;
using System.Numerics;

namespace Elegy.Common.Tests.Geometry;

public static class PolygonTests
{
	[Fact]
	public static void AxisFromPlane()
	{
		Vector3[] normals = [
			Coords.Up,
			Coords.Down,
			Coords.Left,
			Coords.Right,
			Coords.Forward,
			Coords.Back
		];

		foreach ( var normal in normals )
		{
			Plane plane = new( normal, 1.0f );
			Polygon3 polygon = new( plane, 1.0f );

			Assert.Equal( plane.Normal, polygon.Plane.Normal );
			Assert.Equal( plane.D, polygon.Plane.D );
		}
	}
}
