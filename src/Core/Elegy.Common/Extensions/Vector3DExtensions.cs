// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Maths;

namespace Elegy.Common.Extensions
{
	/// <summary></summary>
	public static class Vector3DExtensions
	{
		/// <summary></summary>
		public static Vector3D ToGodot( this Vector3D vector, float scale = 1.0f / 39.37f )
		{
			return new Vector3D( -vector.Y, vector.Z, -vector.X ) * scale;
		}

		/// <summary></summary>
		public static Vector3D Average( this IReadOnlyList<Vector3D> vectors )
		{
			if ( vectors.Count == 0 )
			{
				return Vector3D.Zero;
			}

			Vector3D sum = Vector3D.Zero;
			for ( int i = 0; i < vectors.Count; i++ )
			{
				sum += vectors[i];
			}

			return sum / vectors.Count;
		}

		/// <summary></summary>
		public static bool ContainsInRadius( this IReadOnlyList<Vector3D> vectors, Vector3D point, Vector3D radius )
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

		/// <summary></summary>
		public static IReadOnlyList<Vector3D> WithUniqueValues( this IReadOnlyList<Vector3D> vectors )
		{
			List<Vector3D> uniqueValues = new();
			
			for ( int i = 0; i < vectors.Count; i++ )
			{
				if ( !uniqueValues.Contains( vectors[i] ) )
				{
					uniqueValues.Add( vectors[i] );
				}
			}
			
			return uniqueValues;
		}

		/// <summary></summary>
		public static IReadOnlyList<Vector3D> WithUniqueValuesInRadius( this IReadOnlyList<Vector3D> vectors, Vector3D radius )
		{
			List<Vector3D> uniqueValues = new();

			for ( int i = 0; i < vectors.Count; i++ )
			{
				if ( !uniqueValues.ContainsInRadius( vectors[i], radius ) )
				{
					uniqueValues.Add( vectors[i] );
				}
			}

			return uniqueValues;
		}

		/// <summary></summary>
		public static Vector3D ToVector3D( this Vector4D vector )
		{
			return new Vector3D( vector.X, vector.Y, vector.Z );
		}

		/// <summary></summary>
		public static Vector2D ToVector2D( this Vector4D vector )
		{
			return new Vector2D( vector.X, vector.Y );
		}

		/// <summary></summary>
		public static Vector2D ToVector2D( this Vector3D vector )
		{
			return new Vector2D( vector.X, vector.Y );
		}
	}
}
