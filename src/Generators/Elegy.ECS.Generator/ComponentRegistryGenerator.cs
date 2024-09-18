// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Elegy.ECS.Generator
{
	internal class ComponentDependencyEntry
	{
		public ComponentDependencyEntry( SimpleTypeInfo component )
		{
			Component = component;
		}

		public SimpleTypeInfo Component { get; }

		public List<SimpleTypeInfo> Dependencies { get; } = [];
	}

	/// <summary>
	/// 
	/// </summary>
	[Generator]
	public partial class ComponentRegistryGenerator : SimpleGenerator
	{
		protected override void Execute( SourceProductionContext production, SimpleCompilationData data )
		{
			// Save me some typing
			var types = data.Types;

			// To keep the design as simple as can be, support multiple
			// component registries that do the exact same thing.
			var registries = types
				.Where( t => t.HasAttribute( "GenerateComponentRegistry" ) );

			// Cache all these components early on
			var components = types
				.Where( t => t.HasAttribute( "Component" ) );

			// Also cache info for event handling
			var eventModels = types
				.Where( t => t.HasAttribute( "EventModel" ) );

			var customEntityProperties = types
				.Where( t => t.Implements( "IEntityProperty", partial: true ) );

			foreach ( var registry in registries )
			{
				GenerateBaseApi( production, registry, components );
				GenerateComponentKinds( production, registry, components );
				GeneratePerComponent( production, registry, components, eventModels );
				GenerateCreate( production, registry, components );
				GenerateKeyvalue( production, registry, components, customEntityProperties );
				GenerateEventHandling( production, registry, components );
				GenerateGroupEventHandling( production, registry, components, eventModels );
				GenerateNamedEventHandling( production, registry, components );
			}
		}
	}
}
