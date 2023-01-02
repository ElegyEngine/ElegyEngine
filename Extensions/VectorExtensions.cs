// SPDX-FileCopyrightText: 2022 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy.Extensions
{
	public static class VectorExtensions
	{
		public static Vector3 ToGodot( this Vector3 vector, float scale = 1.0f / 39.37f )
		{
			return new Vector3( -vector.y, vector.z, -vector.x ) * scale;
		}

		public static Vector3 Average( this Vector3[] vectors )
		{
			if ( vectors.Length == 0 )
			{
				return Vector3.Zero;
			}

			Vector3 sum = Vector3.Zero;
			for ( int i = 0; i < vectors.Length; i++ )
			{
				sum += vectors[i];
			}

			return sum / (float)vectors.Length;
		}

		public static Vector3 Forward( this Transform3D transform )
		{
			return -transform.basis.z;
		}

		public static Vector3 Back( this Transform3D transform )
		{
			return transform.basis.z;
		}

		public static Vector3 Right( this Transform3D transform )
		{
			return transform.basis.x;
		}

		public static Vector3 Left( this Transform3D transform )
		{
			return -transform.basis.x;
		}

		public static Vector3 Up( this Transform3D transform )
		{
			return transform.basis.y;
		}

		public static Vector3 Down( this Transform3D transform )
		{
			return -transform.basis.y;
		}

		public static Vector3 ToVector3( this Vector4 vector )
		{
			return new Vector3( vector.x, vector.y, vector.z );
		}

		public static Vector2 ToVector2( this Vector4 vector )
		{
			return new Vector2( vector.x, vector.y );
		}

		public static Vector2 ToVector2( this Vector3 vector )
		{
			return new Vector2( vector.x, vector.y );
		}

		public static Vector3 XOZ( this Vector3 vector )
		{
			return new Vector3( vector.x, 0.0f, vector.z );
		}

		public static Vector2 XZ( this Vector3 vector )
		{
			return new Vector2( vector.x, vector.z );
		}
	}
}
