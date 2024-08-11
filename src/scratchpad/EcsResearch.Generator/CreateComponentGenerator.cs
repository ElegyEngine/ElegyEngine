// SPDX-FileCopyrightText: 2024-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace EcsResearch.Generators
{
	public class ComponentDependencyEntry
	{
		public string ComponentName { get; set; } = string.Empty;
		public string ComponentFullName { get; set; } = string.Empty;
		public List<string> DependencyNames { get; set; } = [];
	}

	[Generator]
	public class CreateComponentGenerator : IIncrementalGenerator
	{
		private string GetFullNameForDecl( TypeDeclarationSyntax syntax )
		{
			string name = GetNameForTypeDecl( syntax );

			List<string> namespaces = new( 4 );
			SyntaxNode? containingNode = syntax.Parent;
			while ( containingNode is not null )
			{
				if ( containingNode is NamespaceDeclarationSyntax @namespace )
				{
					namespaces.Add( @namespace.Name.ToString() );
				}
				else if ( containingNode is TypeDeclarationSyntax type )
				{
					namespaces.Add( GetNameForTypeDecl( type ) );
				}

				containingNode = containingNode.Parent;
			}

			StringBuilder sb = new( namespaces.Count * 2 + 1 );
			for ( int i = namespaces.Count - 1; i >= 0; i-- )
			{
				sb.Append( namespaces[i] );
				sb.Append( '.' );
			}
			sb.Append( name );

			return sb.ToString();
		}

		private static SyntaxList<AttributeListSyntax>? GetAttributeLists( SyntaxNode node )
		{
			if ( node is TypeDeclarationSyntax typeDecl )
			{
				return typeDecl.AttributeLists;
			}
			else if ( node is MemberDeclarationSyntax memberDecl )
			{
				return memberDecl.AttributeLists;
			}
			return [];
		}

		private static string GetNameForTypeDecl( TypeDeclarationSyntax syntax )
		{
			if ( syntax is ClassDeclarationSyntax @class )
			{
				return @class.Identifier.ToString();
			}
			else if ( syntax is StructDeclarationSyntax @struct )
			{
				return @struct.Identifier.ToString();
			}
			else if ( syntax is RecordDeclarationSyntax @record )
			{
				return @record.Identifier.ToString();
			}
			else if ( syntax is InterfaceDeclarationSyntax @interface )
			{
				return @interface.Identifier.ToString();
			}

			return "unknown";
		}

		private static Func<SyntaxNode, CancellationToken, bool> MatchNodeByAttribute( Predicate<AttributeSyntax> any )
			=> ( node, token ) =>
			{
				var attributeLists = GetAttributeLists( node );

				foreach ( var attributeList in attributeLists )
				{
					foreach ( var attribute in attributeList.Attributes )
					{
						if ( any( attribute ) )
						{
							return true;
						}
					}
				}

				return false;
			};

		private static Func<SyntaxNode, CancellationToken, bool> MatchNodeByAttributeName( Predicate<string> any )
			=> MatchNodeByAttribute( attribute => any( attribute.Name.ToString() ) );

		public void Initialize( IncrementalGeneratorInitializationContext context )
		{
			Console.WriteLine( "Blep" );

			// Step 1: obtain full names of all component types
			var componentNamePipeline = context.SyntaxProvider.CreateSyntaxProvider(
				predicate: ( node, token )
					=> node is TypeDeclarationSyntax
						&& MatchNodeByAttributeName( name => name.Contains( "MapComponent" ) )( node, token ),
				transform: ( context, token )
					=> GetFullNameForDecl( context.Node as TypeDeclarationSyntax ) );

			// Step 2: get a list of dependencies for each component
			var componentDependencyPipeline = context.SyntaxProvider.CreateSyntaxProvider<ComponentDependencyEntry>(
				predicate: ( node, token ) 
					=> node is TypeDeclarationSyntax
						&& MatchNodeByAttributeName( name => name.Contains( "Requires<" ) )( node, token ),

				transform: ( context, token ) =>
				{
					var syntax = context.Node as TypeDeclarationSyntax;
					Console.WriteLine( $"Type: {GetNameForTypeDecl( syntax )}" );
					Console.WriteLine( $"  * Full name: {GetFullNameForDecl( syntax )}" );

					ComponentDependencyEntry result = new()
					{
						ComponentName = GetNameForTypeDecl( syntax ),
						ComponentFullName = GetFullNameForDecl( syntax )
					};

					foreach ( var attributeList in syntax.AttributeLists )
					{
						foreach ( var attribute in attributeList.Attributes )
						{
							string attributeName = attribute.Name.ToString();
							Console.WriteLine( $"  * Attrib: {attributeName}" );

							if ( attributeName.Contains( "Requires<" ) )
							{
								int leftArrow = attributeName.IndexOf( '<' ) + 1;
								int rightArrow = attributeName.LastIndexOf( '>' );

								string componentName = attributeName.Substring( leftArrow, rightArrow - leftArrow );
								Console.WriteLine( $"    * This one requires a '{componentName}' component!" );

								// The full name will be resolved later
								result.DependencyNames.Add( componentName );
							}
						}
					}

					return result;
				} );

			var combinedPipeline = componentDependencyPipeline.Combine( componentNamePipeline.Collect() );

			context.RegisterSourceOutput( combinedPipeline.Collect(),
				static ( sourceProductionContext, data ) =>
				{
					string FindComponentFullName( string name )
					{
						var fullNames = data.First().Right;

						foreach ( var fullName in fullNames )
						{
							if ( fullName.EndsWith( $".{name}" ) )
							{
								return fullName;
							}
						}

						return name;
					}

					StringBuilder sb = new();
					sb.AppendLine(
					"""
					// This madness was auto-generated by EcsResearch.Generators
					
					using System;
					using fennecs;
					
					namespace EcsResearch;
					
					public static partial class ComponentRegistry
					{
						public static partial ref T Create<T>( Entity entity )
							where T: notnull, new()
						{
							var typeOfT = typeof( T );
							// We create the component first, so that further Has<T>
							// checks will return true
							ref T result = ref GetComp<T>( entity );
					""" );

					foreach ( var item in data )
					{
						sb.AppendLine(
					$$"""
							if ( typeOfT == typeof( {{item.Left.ComponentFullName}} ) )
							{
					""" );

						foreach ( var dependency in item.Left.DependencyNames )
						{
							string fullName = FindComponentFullName( dependency );

							sb.AppendLine(
					$$"""
								if ( !entity.Has<{{fullName}}>() )
								{
									Create<{{FindComponentFullName( dependency )}}>( entity );
								}
					"""		);
						}

						sb.AppendLine(
					"""
							}
					""" );

					}

					sb.AppendLine(
					"""
							return ref result;
						}
					}
					""" );

					sourceProductionContext.AddSource( "ComponentRegistry.Create.generated.cs",
						SourceText.From( sb.ToString(), Encoding.UTF8 ) );
				} );
		}
	}
}
