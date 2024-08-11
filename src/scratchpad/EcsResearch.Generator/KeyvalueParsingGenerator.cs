// SPDX-FileCopyrightText: 2024-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace EcsResearch.Generators
{
	public enum ComponentPropertyType
	{
		Enum,
		String,
		Bool,
		Int,
		Byte,
		Short,
		Float,
		Double,

		Vector2,
		Vector2D,
		Vector2I,
		Vector3,
		Vector3D,
		Vector3I,
		Vector4,
		Vector4B,
		Vector4D,
		Vector4I,

		Output
	}

	public record ComponentPropertyMetadata
	{
		public string PropertyName { get; set; }
		public ComponentPropertyType Type { get; set; }
		public string TypeName { get; set; } // used for enums so we can cast them
	};

	public record ComponentMetadata
	{
		public string FullName { get; set; }
		public string ComponentName { get; set; }
		public List<ComponentPropertyMetadata> Properties { get; set; }
	}

	[Generator]
	public class KeyvalueParsingGenerator : IIncrementalGenerator
	{
		private static ComponentPropertyType GetComponentPropertyType( string typeName )
			=> typeName switch
			{
				"Bool" => ComponentPropertyType.Bool,
				"Boolean" => ComponentPropertyType.Bool,
				"bool" => ComponentPropertyType.Bool,
				"Double" => ComponentPropertyType.Double,
				"double" => ComponentPropertyType.Double,
				"Single" => ComponentPropertyType.Float,
				"float" => ComponentPropertyType.Float,
				"Int32" => ComponentPropertyType.Int,
				"int" => ComponentPropertyType.Int,
				"String" => ComponentPropertyType.String,
				"string" => ComponentPropertyType.String,
				"Short" => ComponentPropertyType.Short,
				"short" => ComponentPropertyType.Short,

				"Vector2" => ComponentPropertyType.Vector2,
				"Vector2D" => ComponentPropertyType.Vector2D,
				"Vector2I" => ComponentPropertyType.Vector2I,
				"Vector3" => ComponentPropertyType.Vector3,
				"Vector3D" => ComponentPropertyType.Vector3D,
				"Vector3I" => ComponentPropertyType.Vector3I,
				"Vector4" => ComponentPropertyType.Vector4,
				"Vector4B" => ComponentPropertyType.Vector4B,
				"Vector4D" => ComponentPropertyType.Vector4D,
				"Vector4I" => ComponentPropertyType.Vector4I,

				"Output" => ComponentPropertyType.Output,

				_ => throw new NotSupportedException( $"Component property unknown: '{typeName}'" )
			};

		private static string GetFullName( ISymbol symbol )
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

		private static string GetParser( ComponentPropertyType type, string typeName )
			=> type switch
			{
				ComponentPropertyType.Enum => $"({typeName})Enum.ToObject( typeof({typeName}), Parse.Int( value ) )",
				ComponentPropertyType.String => "value",
				ComponentPropertyType.Bool => "Parse.Int( value ) != 0",
				ComponentPropertyType.Int => "Parse.Int( value )",
				ComponentPropertyType.Byte => "(byte)Parse.Int( value )",
				ComponentPropertyType.Short => "(short)Parse.Int( value )",
				ComponentPropertyType.Float => "Parse.Float( value )",
				ComponentPropertyType.Double => "(double)Parse.Float( value )",

				ComponentPropertyType.Vector2 => "Parse.Vector2( value )",
				ComponentPropertyType.Vector2D => "Parse.Vector2D( value )",
				ComponentPropertyType.Vector2I => "Parse.Vector2I( value )",
				ComponentPropertyType.Vector3 => "Parse.Vector3( value )",
				ComponentPropertyType.Vector3D => "Parse.Vector3D( value )",
				ComponentPropertyType.Vector3I => "Parse.Vector3I( value )",
				ComponentPropertyType.Vector4 => "Parse.Vector4( value )",
				ComponentPropertyType.Vector4B => "Parse.Vector4B( value )",
				ComponentPropertyType.Vector4D => "Parse.Vector4D( value )",
				ComponentPropertyType.Vector4I => "Parse.Vector4I( value )",

				ComponentPropertyType.Output => "ParseOutput( entity, key, value )",

				_ => throw new NotSupportedException()
			};

		public void Initialize( IncrementalGeneratorInitializationContext context )
		{
			var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
				fullyQualifiedMetadataName: "EcsResearch.MapComponentAttribute",

				predicate: static ( syntaxNode, cancellationToken )
					=> syntaxNode is TypeDeclarationSyntax,

				transform: static ( syntaxContext, cancellationToken ) =>
				{
					List<ComponentPropertyMetadata> propertyMetadatas = new();

					var typeSymbol = syntaxContext.TargetSymbol as ITypeSymbol;
					var typeMembers = typeSymbol.GetMembers();
					foreach ( var member in typeMembers )
					{
						if ( member is IPropertySymbol typeProperty )
						{
							if ( typeProperty.SetMethod is null )
							{
								continue;
							}

							propertyMetadatas.Add( new()
							{
								PropertyName = typeProperty.Name,
								TypeName = typeProperty.Type.Name,
								Type = typeProperty.Type.TypeKind switch
								{
									TypeKind.Enum => ComponentPropertyType.Enum,
									_ => GetComponentPropertyType( typeProperty.Type.Name )
								} } );
						}
					}

					return new ComponentMetadata()
					{
						FullName = GetFullName( syntaxContext.TargetSymbol ),
						ComponentName = syntaxContext.TargetSymbol.Name,
						Properties = propertyMetadatas
					};
				} );

			context.RegisterSourceOutput( pipeline.Collect(),
				static ( sourceProductionContext, data ) =>
				{
					StringBuilder sb = new();
					sb.AppendLine(
					"""
					// This madness was auto-generated by EcsResearch.Generators
					
					using System;
					using Elegy.Common.Utilities;
					using fennecs;
					
					namespace EcsResearch;
					
					public static partial class ComponentRegistry
					{
						private static ref C GetComp<C>( Entity entity ) where C: new()
						{
							if ( !entity.Has<C>() )
							{
								entity.Add<C>();
							}

							return ref entity.Ref<C>();
						}

						public static partial bool ParseComponentKeyvalue( Entity entity, ReadOnlySpan<char> key, string value )
						{
							switch ( key )
							{
					""" );

					foreach ( var component in data )
					{
						foreach ( var property in component.Properties )
						{
							sb.AppendLine(
					$"""
							case "{component.ComponentName}.{property.PropertyName}":
								Create<{component.FullName}>( entity ).{property.PropertyName} = {GetParser( property.Type, property.TypeName )};
								break;
					"""
							);
						}
					}

					sb.AppendLine(
					"""
							default: return false;
							}

							return true;
						}
					}
					""" );

					sourceProductionContext.AddSource( "ComponentRegistry.Keyvalues.generated.cs",
						SourceText.From( sb.ToString(), Encoding.UTF8 ) );
				} );
		}
	}
}
