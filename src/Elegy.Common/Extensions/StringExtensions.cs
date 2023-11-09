// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

using Elegy.Utilities;

namespace Elegy.Extensions
{
	public static class StringExtensions
	{
		public static bool IsNumeric( this char c )
		{
			return c >= '0' && c <= '9';
		}

		public static List<float> SplitFloatsElegy( this string text, char separator = ' ' )
		{
			List<float> result = new();

			string[] strings = text.Split( separator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries );
			for ( int i = 0; i < strings.Length; i++ )
			{
				result.Add( Parse.Float( strings[i] ) );
			}

			return result;
		}

		public static Vector2 ToVector2( this string text )
		{
			if ( text == string.Empty )
			{
				throw new Exception( "text is empty" );
			}

			List<float> components = text.SplitFloatsElegy();
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

			List<float> components = text.SplitFloatsElegy();
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

			List<float> components = text.SplitFloatsElegy();
			for ( uint i = 0; i < (4 - components.Count); i++ )
			{
				components.Add( 0.0f );
			}

			return new( components[0], components[1], components[2], components[3] );
		}
	}
}
