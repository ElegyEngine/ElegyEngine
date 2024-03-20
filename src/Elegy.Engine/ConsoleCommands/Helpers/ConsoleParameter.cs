// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Engine.ConsoleCommands.Helpers
{
	/// <summary>
	/// Essentially a dictionary of console command arguments, with their respective parsing handlers.
	/// <code>public static void Test( float a, int b, int c = 10 )</code>
	/// would create:
	/// <code>
	///	( "a", FloatHelper ),
	///	( "b", IntHelper ),
	///	( "c", IntHelper, 10 )
	/// </code>
	/// </summary>
	public class ConsoleParameter
	{
		/// <summary>
		/// Creates a <see cref="ConsoleParameter"/> from reflection metadata.
		/// </summary>
		public ConsoleParameter( IConsoleArgumentHelper helper, ParameterInfo info )
		{
			ArgumentHelper = helper;
			Name = info.Name ?? "?";
			DefaultValue = info.DefaultValue;
			Id = info.Position;
		}

		/// <summary>
		/// Name of the parametre.
		/// </summary>
		public string Name { get; } = string.Empty;

		/// <summary>
		/// The argument helper.
		/// </summary>
		public IConsoleArgumentHelper ArgumentHelper { get; }

		/// <summary>
		/// The default value, if any.
		/// </summary>
		public object? DefaultValue { get; } = null;

		/// <summary>
		/// The order of the parameter. Must match the order of the parameter in the method it came from.
		/// </summary>
		public int Id { get; } = -1;
	}
}
