// SPDX-FileCopyrightText: 2022 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy.Utilities
{
	public static class StringExtensions
	{
		public static List<float> SplitFloatsQuake( this string text )
		{
			List<float> result = new();

			string[] strings = text.Split( ' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries );
			for ( int i = 0; i < strings.Length; i++ )
			{
				result.Add( float.Parse( strings[i] ) );
			}

			return result;
		}

		public static Vector2 ToVector2( this string text )
		{
			if ( text == string.Empty )
			{
				throw new Exception( "text is empty" );
			}

			List<float> components = new( text.SplitFloatsQuake() );
			for ( uint i = 0; i < (2 - components.Count); i++ )
			{
				components.Add( 0.0f );
			}

			return new( components[0], components[1] );
		}

		public static Vector3 ToVector3( this string text )
		{
			if ( text == string.Empty )
			{
				throw new Exception( "text is empty" );
			}

			List<float> components = new( text.SplitFloatsQuake() );
			for ( uint i = 0; i < (3 - components.Count); i++ )
			{
				components.Add( 0.0f );
			}

			return new( components[0], components[1], components[2] );
		}

		public static Vector4 ToVector4( this string text )
		{
			if ( text == string.Empty )
			{
				throw new Exception( "text is empty" );
			}

			List<float> components = new( text.SplitFloatsQuake() );
			for ( uint i = 0; i < (4 - components.Count); i++ )
			{
				components.Add( 0.0f );
			}

			return new( components[0], components[1], components[2], components[3] );
		}
	}

	public static class DictionaryExtensions
	{
		public static TValue GetOrAdd<TKey, TValue>( this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue ) where TValue : new()
		{
			if ( dictionary.TryGetValue( key, out TValue? value ) )
			{
				return value;
			}

			dictionary.Add( key, defaultValue );
			return dictionary[key];
		}

		public static int GetInt( this Dictionary<string, string> list, string key, int defaultValue = 0 )
		{
			if ( !int.TryParse( list.GetValueOrDefault( key, string.Empty ), out int result ) )
			{
				return defaultValue;
			}

			return result;
		}

		public static float GetFloat( this Dictionary<string, string> list, string key, float defaultValue = 0.0f )
		{
			if ( !float.TryParse( list.GetValueOrDefault( key, string.Empty ), out float result ) )
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
	}
}
