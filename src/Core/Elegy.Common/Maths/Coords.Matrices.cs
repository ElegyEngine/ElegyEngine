// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Elegy.Common.Maths
{
	/// <summary>
	/// Elegy's coordinate system.
	/// </summary>
	public static partial class Coords
	{
		/// <summary>
		/// We use this to quickly construct needed matrices.
		/// </summary>
		internal struct MatrixWithVec4s
		{
			[UnscopedRef]
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public ref Matrix4x4 AsM4x4() => ref Unsafe.As<MatrixWithVec4s, Matrix4x4>( ref this );

			public Vector4 X;
			public Vector4 Y;
			public Vector4 Z;
			public Vector4 W;

			public MatrixWithVec4s( Vector4 x, Vector4 y, Vector4 z, Vector4 w )
			{
				X = x;
				Y = y;
				Z = z;
				W = w;
			}
		}

		/// <summary>
		/// Constructs a view matrix that you can use for 3D camera transformations.
		/// </summary>
		/// <param name="position">The position. Identity is <see cref="Vector3.Zero"/>.</param>
		/// <param name="forward">The forward matrix. Identity is <see cref="Forward"/>.</param>
		/// <param name="up">The forward matrix. Identity is <see cref="Up"/>.</param>
		public static Matrix4x4 CreateViewMatrix( Vector3 position, Vector3 forward, Vector3 up )
		{
			Vector3 right = Vector3.Cross( forward, up );
			Vector3 positionFinal = new(
				right.Dot( position ),
				up.Dot( position ),
				-forward.Dot( position )
			);

			// Basically, if a vertex (1,2,3) goes into this matrix,
			// it comes out as (1,3,-2) in the viewspace.
			// So the matrix's identity would be something like:
			// | 1  0  0  0 |
			// | 0  0 -1  0 |
			// | 0  1  0  0 |
			// | 0  0  0  1 |

			MatrixWithVec4s mat = new(
				new( new( right.X, up.X, -forward.X ), 0.0f ),
				new( new( right.Y, up.Y, -forward.Y ), 0.0f ),
				new( new( right.Z, up.Z, -forward.Z ), 0.0f ),
				new( -positionFinal, 1.0f )
			);

			return mat.AsM4x4();
		}

		public static Matrix4x4 CreateViewMatrixDegrees( Vector3 position, Vector3 angles )
		{
			DirectionsFromDegrees( angles, out var forward, out var up );
			return CreateViewMatrix( position, forward, up );
		}

		public static Matrix4x4 CreateViewMatrixRadians( Vector3 position, Vector3 angles )
		{
			DirectionsFromRadians( angles, out var forward, out var up );
			return CreateViewMatrix( position, forward, up );
		}

		public static Matrix4x4 CreatePerspectiveMatrix( float fov, float aspectRatio, float nearZ, float farZ )
		{
			return Matrix4x4.CreatePerspectiveFieldOfView( fov, aspectRatio, nearZ, farZ );
		}

		public static Matrix4x4 CreateWorldMatrix( Vector3 position, Vector3 forward, Vector3 up )
		{
			Vector3 right = Vector3.Cross( forward, up );

			MatrixWithVec4s mat = new(
				new( right, 0.0f ),
				new( forward, 0.0f ),
				new( up, 0.0f ),
				new( position, 1.0f )
			);

			return mat.AsM4x4();
		}

		public static Matrix4x4 CreateWorldMatrixDegrees( Vector3 position, Vector3 angles )
		{
			DirectionsFromDegrees( angles, out var forward, out var up );
			return CreateWorldMatrix( position, forward, up );
		}

		public static Matrix4x4 CreateWorldMatrixRadians( Vector3 position, Vector3 angles )
		{
			DirectionsFromRadians( angles, out var forward, out var up );
			return CreateWorldMatrix( position, forward, up );
		}
	}
}
