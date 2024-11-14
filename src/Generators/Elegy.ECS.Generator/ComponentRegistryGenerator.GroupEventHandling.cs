﻿// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Text;

namespace Elegy.ECS.Generator
{
	public partial class ComponentRegistryGenerator
	{
		private void GenerateGroupEventHandling( SourceProductionContext production, SimpleTypeInfo registry,
			List<SimpleTypeInfo> components, List<SimpleTypeInfo> eventModels )
		{
			string findComponentFullName( string namePart )
			{
				foreach ( var component in components )
				{
					if ( component.FullName.EndsWith( namePart ) )
					{
						return $"global::{component.FullName}";
					}
				}

				return namePart;
			}

			string getFullNameByEventModelName( string name )
			{
				foreach ( var model in eventModels )
				{
					if ( name.EndsWith( model.Name ) )
					{
						return model.FullName;
					}
				}

				return name;
			}

			string getEventModelFromMethod( SimpleMethodInfo method )
				=> $"global::{getFullNameByEventModelName( method.Parametres[0].Datatype )}";


			bool methodMatchesEventModel( SimpleMethodInfo method, SimpleTypeInfo model )
			{
				if ( !method.HasAttribute( "GroupEvent" ) )
				{
					return false;
				}

				string typeName = method.Parametres[0].Datatype;
				return typeName.EndsWith( model.Name );
			}

			string getMethodComponents( SimpleMethodInfo method )
			{
				StringBuilder sb = new();
				for ( int i = 1; i < method.Parametres.Count; i++ )
				{
					var parametre = method.Parametres[i];

					sb.Append( findComponentFullName( parametre.Datatype ) );
					if ( i < method.Parametres.Count - 1 )
					{
						sb.Append( ", " );
					}
				}

				return sb.ToString();
			}

			string generateGroupEventMapCalls( SimpleTypeInfo model )
			{
				StringBuilder sb = new();
				foreach ( var component in components )
				{
					foreach ( var method in component.Methods )
					{
						if ( !methodMatchesEventModel( method, model ) )
						{
							continue;
						}

						sb.AppendLine(
			$$"""
						world.Stream<{{getMethodComponents( method )}}>().For(
							uniform: ({{getEventModelFromMethod( method )}})param,
							action: global::{{component.FullName}}.{{method.Name}}
						);
			"""
						);
					}
				}

				return sb.ToString();
			}

			string generateGroupEventMap()
			{
				StringBuilder sb = new();
				foreach ( var model in eventModels )
				{
					sb.AppendLine(
			$$"""
					{ typeof(global::{{model.FullName}}), static ( world, param ) =>
					{
			{{generateGroupEventMapCalls( model )}}
					} },
			"""
					);
				}

				return sb.ToString();
			}

			StringBuilder sb = new();
			sb.AppendLine(
			$$"""
			// This madness was auto-generated by Elegy.ECS.Generator

			namespace {{registry.Namespace}};

			public partial {{registry.Kind.ToString().ToLower()}} {{registry.Name}}
			{
				private static readonly Dictionary<Type, Action<fennecs.World, object>> mGroupEventMap = new()
				{
			{{generateGroupEventMap()}}
				};

				public static bool DispatchGroupEvent<T>( fennecs.World world, T data )
					where T: notnull
				{
					var type = typeof( T );

					if ( !mGroupEventMap.ContainsKey( type ) )
					{
						return false;
					}

					mGroupEventMap[type]( world, data );
					return true;
				}
			}
			"""
			);

			production.AddSource( $"{registry.Name}.GroupEventHandling.generated.cs",
				SourceText.From( sb.ToString(), Encoding.UTF8 ) );
		}
	}
}
