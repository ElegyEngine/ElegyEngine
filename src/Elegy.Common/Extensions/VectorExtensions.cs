// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy.Extensions
{
	public static class VectorExtensions
	{
		public static Vector3 ToGodot( this Vector3 vector, float scale = 1.0f / 39.37f )
		{
			return new Vector3( -vector.Y, vector.Z, -vector.X ) * scale;
		}

		public static Vector3 Average( this IReadOnlyList<Vector3> vectors )
		{
			if ( vectors.Count == 0 )
			{
				return Vector3.Zero;
			}

			Vector3 sum = Vector3.Zero;
			for ( int i = 0; i < vectors.Count; i++ )
			{
				sum += vectors[i];
			}

			return sum / vectors.Count;
		}

		public static bool ContainsInRadius( this IReadOnlyList<Vector3> vectors, Vector3 point, Vector3 radius )
		{
			for ( int i = 0; i < vectors.Count; i++ )
			{
				if ( (vectors[i] - point).LengthSquared() < radius.LengthSquared() )
				{
					return true;
				}
			}

			return false;
		}

		public static IReadOnlyList<Vector3> WithUniqueValues( this IReadOnlyList<Vector3> vectors )
		{
			List<Vector3> uniqueValues = new();
			
			for ( int i = 0; i < vectors.Count; i++ )
			{
				if ( !uniqueValues.Contains( vectors[i] ) )
				{
					uniqueValues.Add( vectors[i] );
				}
			}
			
			return uniqueValues;
		}

		public static IReadOnlyList<Vector3> WithUniqueValuesInRadius( this IReadOnlyList<Vector3> vectors, Vector3 radius )
		{
			List<Vector3> uniqueValues = new();

			for ( int i = 0; i < vectors.Count; i++ )
			{
				if ( !uniqueValues.ContainsInRadius( vectors[i], radius ) )
				{
					uniqueValues.Add( vectors[i] );
				}
			}

			return uniqueValues;
		}

		public static Vector3 Forward( this Transform3D transform )
		{
			return -transform.Basis.Z;
		}

		public static Vector3 Back( this Transform3D transform )
		{
			return transform.Basis.Z;
		}

		public static Vector3 Right( this Transform3D transform )
		{
			return transform.Basis.X;
		}

		public static Vector3 Left( this Transform3D transform )
		{
			return -transform.Basis.X;
		}

		public static Vector3 Up( this Transform3D transform )
		{
			return transform.Basis.Y;
		}

		public static Vector3 Down( this Transform3D transform )
		{
			return -transform.Basis.Y;
		}

		public static Vector3 ToVector3( this Vector4 vector )
		{
			return new Vector3( vector.X, vector.Y, vector.Z );
		}

		public static Vector2 ToVector2( this Vector4 vector )
		{
			return new Vector2( vector.X, vector.Y );
		}

		public static Vector2 ToVector2( this Vector3 vector )
		{
			return new Vector2( vector.X, vector.Y );
		}

		public static Vector3 XOZ( this Vector3 vector )
		{
			return new Vector3( vector.X, 0.0f, vector.Z );
		}

		public static Vector2 XZ( this Vector3 vector )
		{
			return new Vector2( vector.X, vector.Z );
		}
	}
}
