// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Text;
using StringDictionary = System.Collections.Generic.Dictionary<string, string>;

namespace Elegy.Common.Assets
{
	/// <summary>
	/// A material defined inside a material document.
	/// </summary>
	public class MaterialDefinition
	{
		/// <summary>
		/// Material name.
		/// </summary>
		public string Name { get; set; } = string.Empty;

		/// <summary>
		/// Name of the material template.
		/// </summary>
		public string TemplateName { get; set; } = string.Empty;

		/// <summary>
		/// Shader parameters.
		/// </summary>
		public StringDictionary Parameters { get; set; } = new();

		/// <summary>
		/// Map compiler parameters.
		/// </summary>
		public StringDictionary ToolParameters { get; set; } = new();

		/// <summary>
		/// Map compiler flags.
		/// </summary>
		public ToolMaterialFlag ToolFlags { get; set; } = ToolMaterialFlag.None;

		/// <summary>
		/// Safely obtain a parameter string.
		/// </summary>
		public string? GetParameterString( string name )
		{
			if ( Parameters.ContainsKey( name ) )
			{
				return Parameters[name];
			}

			return null;
		}

		/// <summary>
		/// Diffuse map.
		/// </summary>
		public string? DiffuseMap => GetParameterString( "DiffuseMap" );
	}

	/// <summary>
	/// Elegy material document.
	/// </summary>
	public class MaterialDocument
	{
		private void ParseMaterialParameters( Lexer lex, StringDictionary parameters, ref ToolMaterialFlag flags )
		{
			if ( !lex.Expect( "{", true ) )
			{
				return;
			}

			while ( !lex.IsEnd() )
			{
				string token = lex.Next();
				
				if ( token == "}" )
				{
					return;
				}
				else if ( Enum.IsDefined( typeof( ToolMaterialFlag ), token ) )
				{
					flags |= Enum.Parse<ToolMaterialFlag>( token );
				}
				// Q3 map editor hack: interpret 'map' as Elegy's 'DiffuseMap'
				else if ( token == "map" )
				{
					parameters.Add( "DiffuseMap", lex.TokensBeforeNewline() );
				}
				else
				{
					parameters.Add( token, lex.TokensBeforeNewline() );
				}
			}
		}

		private MaterialDefinition? ParseMaterialDefinition( Lexer lex )
		{
			MaterialDefinition materialDefinition = new();

			string token = lex.Next();
			if ( token == "{" || token == "}" )
			{
				// TODO: add a way to jam errors into Lexer, or give me a logger!
				return null;
			}
			materialDefinition.Name = token;
			
			if ( !lex.Expect( "{", true ) )
			{
				return null;
			}

			while ( !lex.IsEnd() )
			{
				token = lex.Next();
				bool isMaterialTemplate = token == "materialTemplate";

				if ( token == "}" )
				{
					break;
				}
				else if ( isMaterialTemplate || token == "compilerParams" )
				{
					if ( isMaterialTemplate )
					{
						materialDefinition.TemplateName = lex.Next();
					}

					var toolFlags = materialDefinition.ToolFlags;
					ParseMaterialParameters(
						lex,
						isMaterialTemplate
						? materialDefinition.Parameters
						: materialDefinition.ToolParameters,
						ref toolFlags );
					
					materialDefinition.ToolFlags = toolFlags;
				}
				else
				{
					// Ignore everything else
					lex.SkipUntil( "}", true );
				}
			}

			return materialDefinition;
		}

		/// <summary>
		/// Parses a material document from given text,
		/// loaded from a file or generated elsewhere.
		/// </summary>
		public MaterialDocument( string content )
		{
			Lexer lex = new( content, "{}" );

			while ( !lex.IsEnd() )
			{
				MaterialDefinition? materialDef = ParseMaterialDefinition( lex );

				if ( materialDef is not null )
				{
					Materials.Add( materialDef );
				}
			}
		}

		/// <summary>
		/// The materials defined in this document.
		/// </summary>
		public List<MaterialDefinition> Materials { get; } = new();
	}
}
