// SPDX-FileCopyrightText: 2023 Admer Šuko
// SPDX-License-Identifier: MIT

using System.Globalization;

namespace Elegy.ConsoleCommands.Helpers
{
	/// <summary>
	/// Console argument helper for floating-point arguments.
	/// </summary>
	[ConsoleArgumentHelper]
	public class FloatHelper : HelperTemplate<float>
	{
		/// <inheritdoc/>
		public override object Parse( ReadOnlySpan<char> argument )
			=> float.Parse( argument, NumberStyles.Float, CultureInfo.InvariantCulture );

		/// <inheritdoc/>
		public override bool Validate( ReadOnlySpan<char> argument, out string? errorMessage )
		{
			if ( !float.TryParse( argument, NumberStyles.Float, CultureInfo.InvariantCulture, out _ ) )
			{
				errorMessage = $"'{argument}' is not numeric";
				return false;
			}

			errorMessage = null;
			return true;
		}
	}

	/// <summary>
	/// Console argument helper for double-precision floating-point arguments.
	/// </summary>
	[ConsoleArgumentHelper]
	public class DoubleHelper : HelperTemplate<double>
	{
		/// <inheritdoc/>
		public override object Parse( ReadOnlySpan<char> argument )
			=> double.Parse( argument, NumberStyles.Float, CultureInfo.InvariantCulture );

		/// <inheritdoc/>
		public override bool Validate( ReadOnlySpan<char> argument, out string? errorMessage )
		{
			if ( !double.TryParse( argument, NumberStyles.Float, CultureInfo.InvariantCulture, out _ ) )
			{
				errorMessage = $"'{argument}' is not numeric";
				return false;
			}

			errorMessage = null;
			return true;
		}
	}

	/// <summary>
	/// Console argument helper for integer arguments.
	/// </summary>
	[ConsoleArgumentHelper]
	public class IntHelper : HelperTemplate<int>
	{
		/// <inheritdoc/>
		public override object Parse( ReadOnlySpan<char> argument )
			=> int.Parse( argument, NumberStyles.Integer, CultureInfo.InvariantCulture );

		/// <inheritdoc/>
		public override bool Validate( ReadOnlySpan<char> argument, out string? errorMessage )
		{
			if ( !int.TryParse( argument, NumberStyles.Integer, CultureInfo.InvariantCulture, out _ ) )
			{
				errorMessage = $"'{argument}' is not numeric";
				return false;
			}

			errorMessage = null;
			return true;
		}
	}

	/// <summary>
	/// Console argument helper for double-precision floating-point arguments.
	/// </summary>
	[ConsoleArgumentHelper]
	public class BoolHelper : HelperTemplate<bool>
	{
		/// <inheritdoc/>
		public override object Parse( ReadOnlySpan<char> argument )
			=> char.ToLower( argument[0] ) switch
			{
				'1' or 't' or 'y' => true,
				'0' or 'f' or 'n' => false,

				_ => new ArgumentException()
			};

		/// <inheritdoc/>
		public override bool Validate( ReadOnlySpan<char> argument, out string? errorMessage )
		{
			bool result = char.ToLower( argument[0] ) switch
			{
				'1' or 't' or 'y' => true,
				'0' or 'f' or 'n' => true,

				_ => false
			};

			errorMessage = result ? null : $"'{argument}' is not boolean";
			return result;
		}
	}

	/// <summary>
	/// Console argument helper for string arguments.
	/// </summary>
	[ConsoleArgumentHelper]
	public class StringHelper : HelperTemplate<string>
	{
		/// <inheritdoc/>
		public override object Parse( ReadOnlySpan<char> argument )
			=> argument.ToString();

		/// <inheritdoc/>
		public override bool Validate( ReadOnlySpan<char> argument, out string? errorMessage )
		{
			errorMessage = null;
			return true;
		}
	}

	/// <summary>
	/// Console argument helper for string arguments.
	/// </summary>
	[ConsoleArgumentHelper]
	public class StringArrayHelper : HelperTemplate<string[]>
	{
		/// <inheritdoc/>
		public override object Parse( ReadOnlySpan<char> argument )
			=> argument.ToString();

		/// <inheritdoc/>
		public override bool Validate( ReadOnlySpan<char> argument, out string? errorMessage )
		{
			errorMessage = null;
			return true;
		}
	}
}
