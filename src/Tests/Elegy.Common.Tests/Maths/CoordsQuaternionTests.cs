// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Maths;
using System.Numerics;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Elegy.Common.Tests.Maths;

public class CoordsQuaternionsTests
{
	private ITestOutputHelper mLogger;

	public CoordsQuaternionsTests( ITestOutputHelper output )
	{
		mLogger = output;
	}

	internal void RoughlyEqual( Vector3 value, Vector3 expected )
	{
		// This should be enough of a delta to catch most discrepancies
		if ( (expected - value).Length() > 0.001f )
		{
			throw new EqualException( expected, value );
		}
	}

	internal void SimpleAnglesTest(
		Vector3 angles, Vector3 expectedForward, Vector3? expectedUp = null, Vector3? expectedRight = null )
	{
		Quaternion q = Coords.WorldQuatFromDegrees( angles );
		Coords.DirectionsFromQuat( q, out var forward, out var up );
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

	public static float Snap( float n )
	{
		if ( n > 0.98f )
			return 1.0f;
		if ( n < -0.98f )
			return -1.0f;

		return 0.0f;
	}

	public static Vector3 Snap( Vector3 v )
	{
		return new( Snap( v.X ), Snap( v.Y ), Snap( v.Z ) );
	}

	[Fact]
	public void ConversionDebug()
	{
		Quaternion q = Coords.WorldQuatFromDegrees( Vector3.Zero );
		mLogger.WriteLine( $"0 0 0 X: {Coords.RightFromQuat( q )}" );
		mLogger.WriteLine( $"0 0 0 Y: {Coords.ForwardFromQuat( q )}" );
		mLogger.WriteLine( $"0 0 0 Z: {Coords.UpFromQuat( q )}" );
		mLogger.WriteLine( "" );
		q = Coords.WorldQuatFromDegrees( new( 90.0f, 0.0f, 0.0f ) );
		mLogger.WriteLine( $"0 0 0 X: {Snap( Coords.RightFromQuat( q ) )}" );
		mLogger.WriteLine( $"0 0 0 Y: {Snap( Coords.ForwardFromQuat( q ) )}" );
		mLogger.WriteLine( $"0 0 0 Z: {Snap( Coords.UpFromQuat( q ) )}" );
		mLogger.WriteLine( "" );
		q = Coords.WorldQuatFromDegrees( new( 0.0f, 90.0f, 0.0f ) );
		mLogger.WriteLine( $"0 0 0 X: {Snap( Coords.RightFromQuat( q ) )}" );
		mLogger.WriteLine( $"0 0 0 Y: {Snap( Coords.ForwardFromQuat( q ) )}" );
		mLogger.WriteLine( $"0 0 0 Z: {Snap( Coords.UpFromQuat( q ) )}" );
		mLogger.WriteLine( "" );
	}

	#region Forward up right

	[Fact]
	public void AnglesIdentity()
		=> SimpleAnglesTest(
			angles: Vector3.Zero,
			expectedForward: Coords.Forward,
			expectedUp: Coords.Up,
			expectedRight: Coords.Right );

	#endregion

	#region Individual axes

	[Fact]
	public void AnglesPitch90()
		=> SimpleAnglesTest(
			angles: new( 90.0f, 0.0f, 0.0f ),
			expectedForward: Coords.Up,
			expectedUp: Coords.Back,
			expectedRight: Coords.Right );

	[Fact]
	public void AnglesPitch90Negative()
		=> SimpleAnglesTest(
			angles: new( -90.0f, 0.0f, 0.0f ),
			expectedForward: Coords.Down,
			expectedUp: Coords.Forward,
			expectedRight: Coords.Right );

	[Fact]
	public void AnglesYaw90()
		=> SimpleAnglesTest(
			angles: new( 0.0f, 90.0f, 0.0f ),
			expectedForward: Coords.Right,
			expectedUp: Coords.Up,
			expectedRight: Coords.Back );

	[Fact]
	public void AnglesYaw90Negative()
		=> SimpleAnglesTest(
			angles: new( 0.0f, -90.0f, 0.0f ),
			expectedForward: Coords.Left,
			expectedUp: Coords.Up,
			expectedRight: Coords.Forward );

	[Fact]
	public void AnglesRoll90()
		=> SimpleAnglesTest(
			angles: new( 0.0f, 0.0f, 90.0f ),
			expectedForward: Coords.Forward,
			expectedUp: Coords.Right,
			expectedRight: Coords.Down );

	[Fact]
	public void AnglesRoll90Negative()
		=> SimpleAnglesTest(
			angles: new( 0.0f, 0.0f, -90.0f ),
			expectedForward: Coords.Forward,
			expectedUp: Coords.Left,
			expectedRight: Coords.Up );

	#endregion

	#region Dual axes

	[Fact]
	public void AnglesPitch90Yaw90()
		=> SimpleAnglesTest(
			angles: new( 90.0f, 90.0f, 0.0f ),
			expectedForward: Coords.Up,
			expectedUp: Coords.Left,
			expectedRight: Coords.Back );

	[Fact]
	public void AnglesPitch90Roll90()
		=> SimpleAnglesTest(
			angles: new( 90.0f, 0.0f, 90.0f ),
			expectedForward: Coords.Up,
			expectedUp: Coords.Right,
			expectedRight: Coords.Forward );

	[Fact]
	public void AnglesYaw90Roll90()
		=> SimpleAnglesTest(
			angles: new( 0.0f, 90.0f, 90.0f ),
			expectedForward: Coords.Right,
			expectedUp: Coords.Back,
			expectedRight: Coords.Down );

	#endregion
}
