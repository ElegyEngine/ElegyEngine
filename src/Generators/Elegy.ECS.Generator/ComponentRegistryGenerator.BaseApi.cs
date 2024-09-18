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
		private void GenerateBaseApi( SourceProductionContext production, SimpleTypeInfo registry, IEnumerable<SimpleTypeInfo> components )
		{
			production.AddSource( $"{registry.Name}.Boilerplate.cs", SourceText.From(
				$$"""
				// This madness was auto-generated by Elegy.ECS.Generator

				using Elegy.ECS;
				
				namespace {{registry.Namespace}};

				public interface IComponent
				{
					ComponentKind Kind { get; }
				}

				public partial {{registry.Kind.ToString().ToLower()}} {{registry.Name}}
				{
					private static ref C GetComp<C>( fennecs.Entity entity ) where C: new()
					{
						if ( !entity.Has<C>() )
						{
							entity.Add<C>();
						}
				
						return ref entity.Ref<C>();
					}
				}
				""", encoding: Encoding.ASCII
			) );
		}
	}
}