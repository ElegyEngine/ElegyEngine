// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Text;

namespace Elegy.ECS.Generator
{
	public static class Utilities
	{
		public class SimpleCompilationVisitor : CSharpSyntaxVisitor
		{
			public SimpleCompilationData Result { get; }

			public SimpleCompilationVisitor( SimpleCompilationData output )
			{
				Result = output;
			}

			public override void VisitCompilationUnit( CompilationUnitSyntax node )
			{
				foreach ( var child in node.ChildNodes() )
				{
					Visit( child );
				}
			}

			public override void VisitNamespaceDeclaration( NamespaceDeclarationSyntax node )
			{
				foreach ( var child in node.ChildNodes() )
				{
					Visit( child );
				}
			}

			private void VisitTypeDeclaration( TypeDeclarationSyntax node )
			{
				SimpleTypeInfo info = new( node );
				Result.TypeDictionary[info.FullName] = info;

				// There may be more types declared inside
				foreach ( var child in node.ChildNodes() )
				{
					Visit( child );
				}
			}

			public override void VisitClassDeclaration( ClassDeclarationSyntax node )
				=> VisitTypeDeclaration( node );

			public override void VisitStructDeclaration( StructDeclarationSyntax node )
				=> VisitTypeDeclaration( node );

			public override void VisitRecordDeclaration( RecordDeclarationSyntax node )
				=> VisitTypeDeclaration( node );
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
		/// Gets the full namespace path to a type.
		/// </summary>
		public static string GetNamespace( TypeDeclarationSyntax syntax )
		{
			string name = syntax.Identifier.ToString();

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
					namespaces.Add( type.Identifier.ToString() );
				}

				containingNode = containingNode.Parent;
			}

			StringBuilder sb = new( namespaces.Count * 2 + 1 );
			sb.Append( namespaces[namespaces.Count - 1] );

			for ( int i = namespaces.Count - 2; i >= 0; i-- )
			{
				sb.Append( '.' );
				sb.Append( namespaces[i] );
			}

			return sb.ToString();
		}

		/// <summary>
		/// Obtains a generic type argument from a type name. Only supports one type parametre!
		/// </summary>
		public static string GetGenericArgument( string typeName )
		{
			int leftArrow = typeName.IndexOf( '<' ) + 1;
			int rightArrow = typeName.LastIndexOf( '>' );

			return typeName.Substring( leftArrow, rightArrow - leftArrow );
		}
	}
}
