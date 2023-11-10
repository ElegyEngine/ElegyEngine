﻿
/*
public static ConCommand cmdMap = new( "map", ( args ) => RunMap( args[0] ) )
{
	Autocomplete = AutocompleteMapSelection,
	Validate = ValidateMapSelection
};
 */

namespace Elegy
{
	/// <summary>
	/// Console command.
	/// </summary>
	public class ConCommand
	{
		/// <summary>
		/// Called when the user has typed this command's <see cref="Name"/> but is
		/// probably missing parameters. Provides autocomplete suggestions.
		/// </summary>
		/// <param name="text">The current text input in the console.</param>
		/// <param name="outSuggestions">Output suggestions. For performance
		/// reasons this is a preallocated buffer of strings.</param>
		/// <returns>If anything is available to autocomplete.</returns>
		public delegate bool AutocompleteMethod( ReadOnlySpan<char> text, ref string[] outSuggestions );

		/// <summary>
		/// Called when the console command is invoked.
		/// </summary>
		/// <param name="args">A list of arguments, without the command itself.</param>
		/// <returns>Success.</returns>
		public delegate bool CommandMethod( string[] args );

		/// <summary>
		/// Called for every keystroke when the command's arguments are typed out.
		/// Provides feedback as to whether the input is correct.
		/// </summary>
		/// <param name="args">The current arguments provided to the console.</param>
		/// <param name="outMessage">A message to the user about their error, if <c>false</c> is returned.</param>
		/// <returns>If everything is correct.</returns>
		public delegate bool ValidateMethod( string[] args, ref string outMessage );

		/// <summary>
		/// The name of this console command.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// See <see cref="AutocompleteMethod"/> for details.
		/// </summary>
		public AutocompleteMethod Autocomplete { get; set; } = DefaultAutocomplete;

		/// <summary>
		/// Method to be called when the console command is invoked.
		/// </summary>
		public CommandMethod Method { get; set; }

		/// <summary>
		/// See <see cref="ValidateMethod"/> for more details.
		/// </summary>
		public ValidateMethod Validate { get; set; } = DefaultValidate;

		/// <summary>
		/// Console command constructor.
		/// </summary>
		public ConCommand( string name, CommandMethod method )
		{
			Name = name;
			Method = method;
		}

		/// <summary>
		/// Default autocomplete implementation. Doesn't offer any suggestions.
		/// </summary>
		public static bool DefaultAutocomplete( ReadOnlySpan<char> text, ref string[] outSuggestions ) => true;

		/// <summary>
		/// Default validate implementation. Everything goes.
		/// </summary>
		public static bool DefaultValidate( string[] args, ref string outMessage ) => true;

		/// <summary>
		/// Validates parameters
		/// </summary>
		public static bool ValidateSingleNumeric( string[] args, ref string outMessage )
		{
			if ( args.Length == 0 )
			{
				outMessage = "Provide one numeric parameter.";
				return false;
			}

			if ( args.Length > 1 )
			{
				outMessage = "You provided more than one parameter, this needs only one.";
				return false;
			}

			foreach ( char c in args[0] )
			{
				if ( !c.IsNumeric() && c != '.' )
				{
					outMessage = "You did not provide a number, this needs one.";
					return false;
				}
			}

			return true;
		}
	}
}
