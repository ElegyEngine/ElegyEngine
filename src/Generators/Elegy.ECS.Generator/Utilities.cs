// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elegy.ECS.Generator
{
	public static class Utilities
	{
		/// <summary>
		/// Refer to <see cref="FindAllSymbols(INamespaceOrTypeSymbol, Predicate{ISymbol})"/>.
		/// </summary>
		private class PredicateVisitor : SymbolVisitor
		{
			public PredicateVisitor( Predicate<ISymbol> predicate )
			{
				Predicate = predicate;
			}

			public Predicate<ISymbol> Predicate { get; }
			public List<ISymbol> Result { get; } = new();

			public override void Visit( ISymbol? symbol )
			{
				if ( symbol is not null && Predicate( symbol ) )
				{
					Result.Add( symbol );
				}

				base.Visit( symbol );
			}
		}

		/// <summary>
		/// Returns all child and descendant symbols that match the predicate.
		/// </summary>
		public static List<ISymbol> FindAllSymbols( INamespaceOrTypeSymbol root, Predicate<ISymbol> filter )
		{
			PredicateVisitor visitor = new( filter );
			root.Accept( visitor );
			return visitor.Result;
		}

		/// <summary>
		/// Refer to <see cref="ForAllSymbols(in IncrementalGeneratorInitializationContext, Action{ISymbol})"/>.
		/// </summary>
		private class ActionVisitor : SymbolVisitor
		{
			public ActionVisitor( Action<ISymbol> action )
			{
				Action = action;
			}

			public Action<ISymbol> Action { get; }

			public override void Visit( ISymbol? symbol )
			{
				if ( symbol is not null )
				{
					Action( symbol );
				}

				base.Visit( symbol );
			}
		}

		/// <summary>
		/// Visits all the symbols with an action.
		/// </summary>
		public static void ForAllSymbols( in IncrementalGeneratorInitializationContext context, Action<ISymbol> action )
		{
			ForAllCompilations( context, compilation =>
			{
				Visit( compilation.GlobalNamespace, action );
			} );
		}

		/// <summary>
		/// Visits the symbol and all child symbols with an action.
		/// </summary>
		public static void Visit( ISymbol symbol, Action<ISymbol> action )
		{
			symbol.Accept( new ActionVisitor( action ) );
		}

		/// <summary>
		/// Visits all the compilations with an action.
		/// </summary>
		public static void ForAllCompilations( in IncrementalGeneratorInitializationContext context, Action<Compilation> action )
		{
			context.CompilationProvider.Select( ( compilation, token ) =>
			{
				action( compilation );
				return 0;
			} );
		}

		/// <summary>
		/// Returns a fully qualified name of the type marked with
		/// GenerateComponentRegistry, or <c>null</c> if it cannot be found.
		/// </summary>
		public static string? GetComponentRegistryTypeName( in IncrementalGeneratorInitializationContext context )
		{
			List<string> results = new();

			// This basically goes through all classes, structs etc.
			// and checks if they have the GCR attribute
			ForAllCompilations( context, compilation =>
			{
				var symbols = FindAllSymbols(
					root: compilation.GlobalNamespace,
					filter: symbol =>
						symbol is ITypeSymbol
						&& HasAttribute( symbol, "GenerateComponentRegistryAttribute" )
				);

				results.AddRange( symbols.Select( s => GetFullName( (ITypeSymbol)s ) ) );
			} );

			if ( results.Count == 0 )
			{
				return null;
			}
			// TODO: complain if there's more than 1 result,
			// only ONE class may be GCR'ed
			return results[0];
		}

		public static string? GetComponentRegistryFullName( IEnumerable<ISymbol> symbols )
		{
			foreach ( var symbol in symbols )
			{
				if ( symbol is null )
				{
					continue;
				}

				if ( HasAttribute( symbol, "GenerateComponentRegistryAttribute" ) )
				{
					return GetFullName( (ITypeSymbol)symbol );
				}
			}

			return null;
		}

		/// <summary>
		/// Returns two strings: a namespace string and a typename string respectively.
		/// </summary>
		public static (string, string) SeparateNamespaceAndTypename( string fullName )
		{
			return (ExtractNamespaces(fullName), StripNamespaces(fullName));
		}

		/// <summary>
		/// Returns just the type name.
		/// </summary>
		public static string StripNamespaces( string name )
		{
			int dot = name.LastIndexOf( '.' );
			if ( dot == -1 )
			{
				return name;
			}

			return name.Substring( dot + 1 );
		}

		/// <summary>
		/// Returns just the type name.
		/// </summary>
		public static string ExtractNamespaces( string name )
		{
			int dot = name.LastIndexOf( '.' );
			if ( dot == -1 )
			{
				return name;
			}

			return name.Substring( 0, dot );
		}

		/// <summary>
		/// Gets the fully qualified name of a symbol.
		/// </summary>
		public static string GetFullName( INamespaceOrTypeSymbol symbol )
		{
			List<string> namespaces = new();
			INamespaceOrTypeSymbol? containingSymbol = symbol.ContainingType is null
				? symbol.ContainingNamespace
				: symbol.ContainingType;

			while ( containingSymbol is not null )
			{
				if ( containingSymbol is INamespaceSymbol namespaceSymbol && namespaceSymbol.IsGlobalNamespace )
				{
					break;
				}

				namespaces.Add( containingSymbol.Name );

				if ( containingSymbol.ContainingType is null )
				{
					containingSymbol = containingSymbol.ContainingNamespace;
				}
				else
				{
					containingSymbol = containingSymbol.ContainingType;
				}
			}

			StringBuilder sb = new( namespaces.Count * 2 + 1 );
			for ( int i = namespaces.Count - 1; i >= 0; i-- )
			{
				sb.Append( namespaces[i] );
				sb.Append( '.' );
			}
			sb.Append( symbol.Name );

			return sb.ToString();
		}

		/// <summary>
		/// Whether or not a symbol is marked with an attribute, at least partially.
		/// </summary>
		public static bool HasAttribute( ISymbol symbol, string attributeName, bool partial = false )
		{
			foreach ( var attributeData in symbol.GetAttributes() )
			{
				if ( attributeData.AttributeClass is null )
				{
					continue;
				}

				string actualName = GetFullName( attributeData.AttributeClass );

				if ( actualName == attributeName )
				{
					return true;
				}

				if ( partial && actualName.Contains( attributeName ) )
				{
					return true;
				}
			}

			return false;
		}

		public static void SetupSymbolBasedGenerator( in IncrementalGeneratorInitializationContext context, bool implementationOutput,
			Action<SourceProductionContext, IEnumerable<ISymbol>> action )
		{
			var pipeline = context.SyntaxProvider.CreateSyntaxProvider(
				predicate: ( node, token )
					=> node is TypeDeclarationSyntax,

				transform: ( syntaxContext, token )
					=> syntaxContext.SemanticModel.GetSymbolInfo( syntaxContext.Node ).Symbol
			);

			if ( implementationOutput )
			{
				context.RegisterImplementationSourceOutput( pipeline.Collect(), ( productionContext, data ) =>
				{
					IEnumerable<ISymbol> dataNotNull = data.Where( s => s is not null );
					action( productionContext, dataNotNull );
				} );
			}
			else
			{
				context.RegisterSourceOutput( pipeline.Collect(), ( productionContext, data ) =>
				{
					IEnumerable<ISymbol> dataNotNull = data.Where( s => s is not null );
					action( productionContext, dataNotNull );
				} );
			}
		}
	}
}
