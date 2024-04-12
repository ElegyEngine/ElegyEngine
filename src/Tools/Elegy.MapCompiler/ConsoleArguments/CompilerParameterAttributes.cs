// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Text;

namespace Elegy.MapCompiler.ConsoleArguments
{
	[AttributeUsage( AttributeTargets.Property )]
	public abstract class CompilerParamAttribute : Attribute
	{
		public string Name { get; set; }

		public CompilerParamAttribute( string name )
		{
			Name = name;
		}

		public abstract object Parse( string value );

		public abstract string GetPossibleValues();
	}

	public class PathParamAttribute : CompilerParamAttribute
	{
		public bool IsFile { get; private set; }

		public PathParamAttribute( string name, bool isFile = false )
			: base( name )
		{
			IsFile = isFile;
		}

		public override object Parse( string value )
		{
			return value.Replace( '\\', '/' ).TrimEnd( '/' );
		}

		public override string GetPossibleValues() => $"a valid path to a {(IsFile ? "file" : "directory")}";
	}

	public class EnumParamAttribute<T> : CompilerParamAttribute where T : struct, Enum
	{
		private Type enumType = typeof( T );

		public EnumParamAttribute( string name )
			: base( name )
		{
		}

		public override object Parse( string value )
		{
			if ( Enum.TryParse( enumType, value, out var result ) )
			{
				return result;
			}

			return Enum.ToObject( enumType, 0 );
		}

		public override string GetPossibleValues()
		{
			StringBuilder stringBuilder = new();
			var names = Enum.GetNames<T>();
			for ( int i = 0; i < names.Length; i++ )
			{
				stringBuilder.Append( names[i] );
				if ( i < names.Length - 2 )
				{
					stringBuilder.Append( ", " );
				}
				else if ( i == names.Length - 2 )
				{
					stringBuilder.Append( ", or " );
				}
			}
			return stringBuilder.ToString();
		}
	}

	public class FloatParamAttribute : CompilerParamAttribute
	{
		public float MinValue { get; set; }
		public float MaxValue { get; set; }

		public FloatParamAttribute( string name, float minValue, float maxValue )
			: base( name )
		{
			MinValue = minValue;
			MaxValue = maxValue;
		}

		public override object Parse( string value )
		{
			return Math.Clamp( Common.Utilities.Parse.Float( value ), MinValue, MaxValue );
		}

		public override string GetPossibleValues() => $"between {MinValue} and {MaxValue}";
	}
}
