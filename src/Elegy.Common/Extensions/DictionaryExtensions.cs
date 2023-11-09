// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

using Elegy.Utilities;

namespace Elegy.Extensions
{
	public static class DictionaryExtensions
	{
#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
		public static TValue GetOrAdd<TKey, TValue>( this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue ) where TValue : new()
		{
			if ( dictionary.TryGetValue( key, out TValue? value ) )
			{
				return value;
			}

			dictionary.Add( key, defaultValue );
			return dictionary[key];
		}
#pragma warning restore CS8714

		public static int GetInt( this Dictionary<string, string> list, string key, int defaultValue = 0 )
		{
			if ( !Parse.TryInt( list.GetValueOrDefault( key, string.Empty ), out int result ) )
			{
				return defaultValue;
			}

			return result;
		}

		public static float GetFloat( this Dictionary<string, string> list, string key, float defaultValue = 0.0f )
		{
			if ( !Parse.TryFloat( list.GetValueOrDefault( key, string.Empty ), out float result ) )
			{
				return defaultValue;
			}

			return result;
		}

		public static bool GetBool( this Dictionary<string, string> list, string key, bool defaultValue = false )
		{
			if ( key == string.Empty )
			{
				return defaultValue;
			}

			return list.GetInt( key ) != 0;
		}

		public static Vector2 GetVector2( this Dictionary<string, string> list, string key )
		{
			return list.GetValueOrDefault( key, "0 0" ).ToVector2();
		}

		public static Vector3 GetVector3( this Dictionary<string, string> list, string key )
		{
			return list.GetValueOrDefault( key, "0 0 0" ).ToVector3();
		}

		public static Vector4 GetVector4( this Dictionary<string, string> list, string key )
		{
			return list.GetValueOrDefault( key, "0 0 0 0" ).ToVector4();
		}

		public static void SetVector2( this Dictionary<string, string> list, string key, Vector2 value )
		{
			list[key] = $"{value.X} {value.Y}";
		}

		public static void SetVector3( this Dictionary<string, string> list, string key, Vector3 value )
		{
			list[key] = $"{value.X} {value.Y} {value.Z}";
		}

		public static void SetVector4( this Dictionary<string, string> list, string key, Vector4 value )
		{
			list[key] = $"{value.X} {value.Y} {value.Z} {value.W}";
		}
	}
}
