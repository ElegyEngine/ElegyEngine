// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Extensions;
using Elegy.Common.Maths;
using System.Numerics;
using Xunit.Sdk;

namespace Elegy.Common.Tests.Maths;

public static class CoordsMatrixTests
{
	//[Fact]
	//public static void ViewMatrixIdentity()
	//{
	//	var viewMatrix = Coords.CreateViewMatrix( Vector3.Zero, Coords.Forward, Coords.Up );
	//	Assert.Equal( Matrix4x4.Identity, viewMatrix );
	//}

	[Fact]
	public static void ViewMatrixIdentityTransform()
	{
		var viewMatrix = Coords.CreateViewMatrix( Vector3.Zero, Coords.Forward, Coords.Up );
		Vector3 transformedForwardVector = Vector3.Transform( new( 1.0f, 2.0f, 3.0f ), viewMatrix );
		CoordsAngleTests.RoughlyEqual( new( 1.0f, 3.0f, -2.0f ), transformedForwardVector );
	}

	[Fact]
	public static void ViewMatrixIdentityTransform_Yaw90()
	{
		Coords.DirectionsFromRadians( Coords.TurnRight / 4.0f, out var forward, out var up );
	
		var viewMatrix = Coords.CreateViewMatrix( Vector3.Zero, forward, up );

		// If we turn right, then the point - relative to our view - goes left
		Vector3 transformedForwardVector = Vector3.Transform( Coords.Forward, viewMatrix );
		CoordsAngleTests.RoughlyEqual( Coords.Left, transformedForwardVector );

		// If we turn right, then the point - relative to our view - goes left
		transformedForwardVector = Vector3.Transform( new( 1.0f, 2.0f, 3.0f ), viewMatrix );
		CoordsAngleTests.RoughlyEqual( new( -2.0f, 3.0f, -1.0f ), transformedForwardVector );
	}
}
