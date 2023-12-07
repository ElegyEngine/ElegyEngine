// SPDX-FileCopyrightText: 2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy
{
	/// <summary>
	/// Marks a method as a console command to be registered.
	/// </summary>
	[AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
	public class ConsoleCommandAttribute : Attribute
	{
		/// <summary>
		/// The name of the console command to be displayed and looked up by <see cref="Console.Execute(string)"/>.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Basic console command attribute constructor.
		/// </summary>
		public ConsoleCommandAttribute( string name = "" )
		{
			// If this is empty, it'll get filled in by reflection later
			Name = name;
		}
	}
}
