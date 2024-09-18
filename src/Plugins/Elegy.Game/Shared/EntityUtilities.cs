// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.ECS;
using System.Runtime.CompilerServices;

namespace Game.Shared
{
	/// <summary>
	/// Manages everything pertaining to entity components and such.
	/// </summary>
	[GenerateComponentRegistry]
	public partial class EntityUtilities
	{
		public static bool DispatchEvent<T>( fennecs.Entity entity )
			where T : Delegate
		{
			var type = typeof( T );

			var eventHandlers = entity.Ref<Entity>().Archetype.EventHandlers.GetValueOrDefault( type );
			if ( eventHandlers is null )
			{
				return false;
			}

			foreach ( var handler in eventHandlers )
			{
				handler( entity );
			}

			return true;
		}

		public static bool DispatchEvent<T>( fennecs.Entity entity, T param )
			where T : notnull
		{
			var type = typeof( T );

			var complexEventHandlers = entity.Ref<Entity>().Archetype.ComplexEventHandlers.GetValueOrDefault( type );
			if ( complexEventHandlers is null )
			{
				return false;
			}

			foreach ( var handler in complexEventHandlers )
			{
				handler( entity, param );
			}

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void DispatchGroup<T>( fennecs.World world, T data ) where T : notnull
			=> DispatchGroupEvent<T>( world, data );

		private static List<Archetype> mArchetypes = [];

		public static void FinishSpawningEntity( fennecs.Entity entity )
		{
			var components = entity.Components;

			List<int> componentIds = new( components.Count );
			foreach ( var component in components )
			{
				if ( component.Box.Value is not IComponent )
				{
					continue;
				}

				componentIds.Add( (int)((IComponent)component.Box.Value).Kind );
			}

			long[] componentMask = Archetype.EncodeComponentMask( componentIds );
			foreach ( var archetype in mArchetypes )
			{
				if ( archetype.Matches( componentMask ) )
				{
					entity.Add( archetype );
					break;
				}
			}

			if ( !entity.Has<Archetype>() )
			{
				mArchetypes.Add( new()
				{
					ComponentMask = componentMask,
					EventHandlers = GeneratedEventHandlers( componentMask ),
					ComplexEventHandlers = GeneratedGroupEventHandlers( componentMask )
				} );

				entity.Add( mArchetypes.Last() );
			}
		}
	}
}
