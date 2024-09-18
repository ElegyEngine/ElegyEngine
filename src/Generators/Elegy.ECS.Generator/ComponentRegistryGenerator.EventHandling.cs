﻿// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Elegy.ECS.Generator
{
	public partial class ComponentRegistryGenerator
	{
		private void GenerateEventHandling( SourceProductionContext production, SimpleTypeInfo registry,
			IEnumerable<SimpleTypeInfo> components )
		{
			StringBuilder sb = new();
			sb.AppendLine(
			$$"""
			// This madness was auto-generated by Elegy.ECS.Generator

			namespace {{registry.Namespace}};

			public partial {{registry.Kind.ToString().ToLower()}} {{registry.Name}}
			{
				internal static Dictionary<Type, List<Action<fennecs.Entity>>> GeneratedEventHandlers( long[] componentMask )
				{
					Dictionary<Type, List<Action<fennecs.Entity>>> result = new();

					int highest = componentMask.Length * 64;
					for ( int i = 0; i < highest; i++ )
					{
						if ( !global::Elegy.ECS.Archetype.TestPosition( componentMask, i ) )
						{
							continue;
						}

						ExpandEventHandler( result, i );
					}

					return result;
				}

				internal static Dictionary<Type, List<Action<fennecs.Entity, object>>> GeneratedGroupEventHandlers( long[] componentMask )
				{
					Dictionary<Type, List<Action<fennecs.Entity, object>>> result = new();
			
					int highest = componentMask.Length * 64;
					for ( int i = 0; i < highest; i++ )
					{
						if ( !global::Elegy.ECS.Archetype.TestPosition( componentMask, i ) )
						{
							continue;
						}
			
						ExpandGroupEventHandler( result, i );
					}
			
					return result;
				}

				static void ExpandEventHandler( Dictionary<Type, List<Action<fennecs.Entity>>> dictionary, int componentId )
				{
					var handlerList = componentId switch
					{
			"""
			);

			foreach ( var component in components )
			{
				sb.AppendLine(
			$$"""
						(int)ComponentKind.{{component.FullName.Replace( '.', '_' )}} => global::{{component.FullName}}.EventHandlerList,
			"""
				);
			}

			sb.AppendLine(
			"""
						_ => []
					};
			
					foreach ( var handlerPair in handlerList )
					{
						if ( !dictionary.ContainsKey( handlerPair.Item1 ) )
						{
							dictionary[handlerPair.Item1] = new();
						}

						dictionary[handlerPair.Item1].Add( handlerPair.Item2 );
					}
				}
			
				static void ExpandGroupEventHandler( Dictionary<Type, List<Action<fennecs.Entity, object>>> dictionary, int componentId )
				{
					var handlerList = componentId switch
					{
			"""
			);

			foreach ( var component in components )
			{
				sb.AppendLine(
			$$"""
						(int)ComponentKind.{{component.FullName.Replace( '.', '_' )}} => global::{{component.FullName}}.ComplexEventHandlerList,
			"""
				);
			}

			sb.AppendLine(
			"""
						_ => []
					};
			
					foreach ( var handlerPair in handlerList )
					{
						if ( !dictionary.ContainsKey( handlerPair.Item1 ) )
						{
							dictionary[handlerPair.Item1] = new();
						}

						dictionary[handlerPair.Item1].Add( handlerPair.Item2 );
					}
				}
			}
			"""
			);

			production.AddSource( $"{registry.Name}.EventHandling.generated.cs",
				SourceText.From( sb.ToString(), Encoding.UTF8 ) );
		}
	}
}