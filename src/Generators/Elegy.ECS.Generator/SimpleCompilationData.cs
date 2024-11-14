// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Elegy.ECS.Generator
{
	public enum TypeKind
	{
		Class,
		Struct,
		Record,
		Enum,
		Invalid
	}

	public record SimpleInfo
	{
		public SimpleInfo( string name, SyntaxList<AttributeListSyntax> attributeLists )
		{
			Name = name;

			foreach ( var attributeList in attributeLists )
			{
				foreach ( var attribute in attributeList.Attributes )
				{
					Attributes.Add( attribute.ToString() );
				}
			}
		}

		public string Name { get; set; } = string.Empty;
		public List<string> Attributes { get; set; } = [];

		public bool HasAttribute( string name, bool partial = false )
		{
			foreach ( var attribute in Attributes )
			{
				if ( attribute == name )
				{
					return true;
				}
				
				if ( partial && attribute.Contains( name ) )
				{
					return true;
				}
			}

			return false;
		}
	}

	[DebuggerDisplay( "{Datatype} {Name} ({Attributes.Count} attributes)" )]
	public record SimplePropertyInfo : SimpleInfo
	{
		public SimplePropertyInfo( PropertyDeclarationSyntax syntax )
			: base( syntax.Identifier.ValueText, syntax.AttributeLists )
		{
			Datatype = syntax.Type.ToString();
		}

		public string Datatype { get; set; } = string.Empty;
	}

	[DebuggerDisplay( "{Datatype} {Name} ({Attributes.Count} attributes)" )]
	public record SimpleParametreInfo : SimpleInfo
	{
		public SimpleParametreInfo( ParameterSyntax syntax )
			: base( syntax.Identifier.ValueText, syntax.AttributeLists )
		{
			Datatype = syntax.Type?.ToString() ?? "object?";
		}

		public string Datatype { get; set; } = string.Empty;
	}

	[DebuggerDisplay( "Method {Name}({Parametres}) ({Attributes.Count} attributes)" )]
	public record SimpleMethodInfo : SimpleInfo
	{
		public SimpleMethodInfo( MethodDeclarationSyntax syntax )
			: base( syntax.Identifier.ValueText, syntax.AttributeLists )
		{
			foreach ( var parametre in syntax.ParameterList.Parameters )
			{
				Parametres.Add( new( parametre ) );
			}
		}

		public List<SimpleParametreInfo> Parametres { get; set; } = [];
	}

	[DebuggerDisplay( "Method {Name}({Parametres}) ({Attributes.Count} attributes)" )]
	public record SimpleDelegateInfo : SimpleInfo
	{
		public SimpleDelegateInfo( SimpleTypeInfo parent, DelegateDeclarationSyntax syntax )
			: base( syntax.Identifier.ValueText, syntax.AttributeLists )
		{
			foreach ( var parametre in syntax.ParameterList.Parameters )
			{
				Parametres.Add( new( parametre ) );
			}

			ContainingType = parent;
		}

		public string FullName => $"{ContainingType.FullName}.{Name}";

		public SimpleTypeInfo ContainingType { get; }

		public List<SimpleParametreInfo> Parametres { get; set; } = [];
	}

	[DebuggerDisplay( "{Kind} {Name} ({Attributes.Count} attributes)" )]
	public record SimpleTypeInfo : SimpleInfo
	{
		public SimpleTypeInfo( TypeDeclarationSyntax syntax )
			: base( syntax.Identifier.ValueText, syntax.AttributeLists )
		{
			if ( syntax is ClassDeclarationSyntax )
			{
				Kind = TypeKind.Class;
			}
			else if ( syntax is StructDeclarationSyntax )
			{
				Kind = TypeKind.Struct;
			}
			else if ( syntax is RecordDeclarationSyntax )
			{
				Kind = TypeKind.Record;
			}

			foreach ( var member in syntax.Members )
			{
				if ( member is MethodDeclarationSyntax method )
				{
					Methods.Add( new( method ) );
				}
				else if ( member is PropertyDeclarationSyntax property )
				{
					Properties.Add( new( property ) );
				}
				else if ( member is DelegateDeclarationSyntax @delegate )
				{
					Delegates.Add( new( this, @delegate ) );
				}
			}

			foreach ( var modifier in syntax.Modifiers )
			{
				if ( modifier.ToString() == "partial" )
				{
					Partial = true;
					break;
				}
			}

			Namespace = Utilities.GetNamespace( syntax );

			if ( syntax.BaseList is not null )
			{
				Bases.AddRange( syntax.BaseList.Types.Select( b => b.ToString() ) );
			}
		}

		/// <summary>
		/// Partial datatypes are eligible for merging.
		/// Methods and properties are affected.
		/// </summary>
		public void Merge( SimpleTypeInfo other )
		{
			Attributes.AddRange( other.Attributes );
			Methods.AddRange( other.Methods );
			Delegates.AddRange( other.Delegates );
			Properties.AddRange( other.Properties );
			Bases.AddRange( other.Bases );
		}

		public bool Implements( string name, bool partial = false )
		{
			foreach ( var type in Bases )
			{
				if ( type == name )
				{
					return true;
				}

				if ( partial && type.Contains( name ) )
				{
					return true;
				}
			}

			return false;
		}

		public string FullName => $"{Namespace}.{Name}";
		public string Namespace { get; set; } = string.Empty;
		public bool Partial { get; set; } = false;
		public TypeKind Kind { get; set; } = TypeKind.Invalid;
		public List<SimpleMethodInfo> Methods { get; set; } = [];
		public List<SimpleDelegateInfo> Delegates { get; set; } = [];
		public List<SimplePropertyInfo> Properties { get; set; } = [];
		public List<string> Bases { get; set; } = [];
	}

	/// <summary>
	/// Data for one compilation unit. Can merge with other
	/// instances of itself.
	/// </summary>
	public class SimpleCompilationData
	{
		public SimpleCompilationData( CompilationUnitSyntax syntax )
		{
			Utilities.SimpleCompilationVisitor visitor = new( this );
			visitor.Visit( syntax );
		}

		public SimpleCompilationData( SimpleCompilationData data )
		{
			TypeDictionary = new( data.TypeDictionary );
		}

		/// <summary>
		/// Basic data about all datatypes. Implemented as a dictionary
		/// for faster merging in <see cref="Merge(SimpleCompilationData)"/>.
		/// </summary>
		public Dictionary<string, SimpleTypeInfo> TypeDictionary { get; set; } = [];

		public Dictionary<string, SimpleTypeInfo>.ValueCollection Types => TypeDictionary.Values;

		/// <summary>
		/// Expands current compilation data with data from another.
		/// Duplicates are merged.
		/// </summary>
		public void Merge( SimpleCompilationData other )
		{
			foreach ( var type in other.Types )
			{
				if ( !TypeDictionary.ContainsKey( type.FullName ) )
				{
					TypeDictionary[type.FullName] = type;
					continue;
				}

				TypeDictionary[type.FullName].Merge( type );
			}
		}
	}
}
