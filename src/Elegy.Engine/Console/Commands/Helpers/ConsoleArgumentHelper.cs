// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.ConsoleCommands.Helpers
{
	/// <summary>
	/// Marks a class to be used by <see cref="HelperManager"/> to help with console parsing.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
	public class ConsoleArgumentHelperAttribute : Attribute
	{

	}

	/// <summary>
	/// Interface for <see cref="ConsoleCommand"/> argument parsing utilities.
	/// </summary>
	public interface IConsoleArgumentHelper
	{
		/// <summary>
		/// The datatype that the helper works with.
		/// </summary>
		public Type Type { get; }

		/// <summary>
		/// Parses the argument as a string and resolves it as the appropriate datatype <see cref="Type"/>.
		/// </summary>
		public object Parse( ReadOnlySpan<char> argument );

		/// <summary>
		/// Checks if <paramref name="argument"/> can be validated and if not, fills in <paramref name="errorMessage"/>.
		/// </summary>
		/// <returns><c>true</c> if <paramref name="argument"/> is valid, <c>false</c> otherwise.</returns>
		public bool Validate( ReadOnlySpan<char> argument, out string? errorMessage );
	}

	/// <summary>
	/// A template to help you implement <see cref="IConsoleArgumentHelper"/>.
	/// </summary>
	public abstract class HelperTemplate<T> : IConsoleArgumentHelper
	{
		/// <inheritdoc/>
		public Type Type => typeof(T);

		/// <inheritdoc/>
		public abstract object Parse( ReadOnlySpan<char> argument );

		/// <inheritdoc/>
		public abstract bool Validate( ReadOnlySpan<char> argument, out string? errorMessage );
	}
}
