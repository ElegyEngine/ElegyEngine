// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Maths;
using System.Numerics;
using Xunit.Sdk;

namespace Elegy.Common.Tests.Maths;

public static class CoordsAngleTests
{
	internal static void RoughlyEqual( Vector3 value, Vector3 expected )
	{
		// This should be enough of a delta to catch most discrepancies
		if ( (expected - value).Length() > 0.001f )
		{
			throw new EqualException( expected, value );
		}
	}

	internal static void SimpleAnglesTest( Vector3 angles, Vector3 expectedForward, Vector3? expectedUp = null, Vector3? expectedRight = null )
	{
		Coords.DirectionsFromDegrees( angles, out var forward, out var up );
		Vector3 right = Vector3.Cross( forward, up );

		RoughlyEqual( forward, expectedForward );

		if ( expectedUp is not null )
		{
			RoughlyEqual( up, expectedUp.Value );
		}

		if ( expectedRight is not null )
		{
			RoughlyEqual( right, expectedRight.Value );
		}
	}

	[Fact]
	public static void AnglesIdentity()
		=> SimpleAnglesTest(
			angles: Vector3.Zero,
			expectedForward: Coords.Forward,
			expectedUp: Coords.Up,
			expectedRight: Coords.Right );

	#region Individual axes
	[Fact]
	public static void AnglesPitch90()
		=> SimpleAnglesTest(
			angles: new( 90.0f, 0.0f, 0.0f ),
			expectedForward: Coords.Up,
			expectedUp: Coords.Back,
			expectedRight: Coords.Right );

	[Fact]
	public static void AnglesPitch90Negative()
		=> SimpleAnglesTest(
			angles: new( -90.0f, 0.0f, 0.0f ),
			expectedForward: Coords.Down,
			expectedUp: Coords.Forward,
			expectedRight: Coords.Right );

	[Fact]
	public static void AnglesYaw90()
		=> SimpleAnglesTest(
			angles: new( 0.0f, 90.0f, 0.0f ),
			expectedForward: Coords.Right,
			expectedUp: Coords.Up,
			expectedRight: Coords.Back );

	[Fact]
	public static void AnglesYaw90Negative()
		=> SimpleAnglesTest(
			angles: new( 0.0f, -90.0f, 0.0f ),
			expectedForward: Coords.Left,
			expectedUp: Coords.Up,
			expectedRight: Coords.Forward );

	[Fact]
	public static void AnglesRoll90()
		=> SimpleAnglesTest(
			angles: new( 0.0f, 0.0f, 90.0f ),
			expectedForward: Coords.Forward,
			expectedUp: Coords.Right,
			expectedRight: Coords.Down );

	[Fact]
	public static void AnglesRoll90Negative()
		=> SimpleAnglesTest(
			angles: new( 0.0f, 0.0f, -90.0f ),
			expectedForward: Coords.Forward,
			expectedUp: Coords.Left,
			expectedRight: Coords.Up );

	#endregion

	#region Dual axes
	[Fact]
	public static void AnglesPitch90Yaw90()
		=> SimpleAnglesTest(
			angles: new( 90.0f, 90.0f, 0.0f ),
			expectedForward: Coords.Up,
			expectedUp: Coords.Left,
			expectedRight: Coords.Back );

	[Fact]
	public static void AnglesPitch90Roll90()
		=> SimpleAnglesTest(
			angles: new( 90.0f, 0.0f, 90.0f ),
			expectedForward: Coords.Up,
			expectedUp: Coords.Right,
			expectedRight: Coords.Forward );

	[Fact]
	public static void AnglesYaw90Roll90()
		=> SimpleAnglesTest(
			angles: new( 0.0f, 90.0f, 90.0f ),
			expectedForward: Coords.Right,
			expectedUp: Coords.Back,
			expectedRight: Coords.Down );
	#endregion
}
