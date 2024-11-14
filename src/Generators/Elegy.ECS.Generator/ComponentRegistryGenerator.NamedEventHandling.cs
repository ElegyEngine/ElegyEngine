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
		private void GenerateNamedEventHandling( SourceProductionContext production, SimpleTypeInfo registry,
			List<SimpleTypeInfo> components )
		{
			StringBuilder sb = new();
			sb.AppendLine(
			$$"""
			// This madness was auto-generated by Elegy.ECS.Generator
			
			namespace {{registry.Namespace}};
			
			public partial {{registry.Kind.ToString().ToLower()}} {{registry.Name}}
			{
				public static void DispatchNamedEvent( fennecs.Entity entity, ReadOnlySpan<char> name,
					Action<string> warn )
				{
					switch ( name )
					{
			"""
			);

			foreach ( var component in components )
			{
				foreach ( var method in component.Methods )
				{
					if ( method.HasAttribute( "Input" ) )
					{
						sb.AppendLine(
			$$"""
					case "{{component.Name}}.{{method.Name}}":
						entity.Ref<global::{{component.FullName}}>().{{method.Name}}();
						break;
			"""
						);
					}
				}
			}

			sb.AppendLine(
			"""
					default:
						warn( $"Unknown named event '{name}'" );
						break;
					}
				}
			}
			"""
			);

			production.AddSource( $"{registry.Name}.NamedEventHandling.generated.cs",
				SourceText.From( sb.ToString(), Encoding.UTF8 ) );
		}
	}
}
