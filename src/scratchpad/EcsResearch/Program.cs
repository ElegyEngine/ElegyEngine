﻿// SPDX-FileCopyrightText: 2024-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using EcsResearch;
using fennecs;
using System.Numerics;

namespace EcsResearchFunky
{
	namespace ContainingNamespace.Another
	{
		public class ContainingContainingClass
		{
			public struct ContainingContainingStruct
			{
				public class ContainingClass
				{
					public struct ContainingStruct
					{
						[Requires<Transform>]
						[MapComponent]
						public record struct FunkyComponent()
						{
							public string Data { get; set; }
						}
					}
				}

				[Requires<AudioSource>]
				[Requires<Transform>]
				[MapComponent]
				public record struct AnotherFunkyComponent()
				{
					public string Data { get; set; }
				}
			}
		}
	}
}

namespace EcsResearch
{
	#region Internal stuff
	public class InputAttribute : Attribute
	{
	}

	[AttributeUsage( AttributeTargets.Struct, AllowMultiple = true )]
	public abstract class BaseRequiresAttribute : Attribute
	{
		public abstract Type ComponentType { get; }
	}
	
	public class RequiresAttribute<T> : BaseRequiresAttribute
		where T: struct
	{
		public override Type ComponentType => typeof( T );
	}

	[AttributeUsage( AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false )]
	public class MapComponentAttribute : Attribute
	{

	}
	#endregion

	#region Game system stuff
	public record struct OutputEntry( string TargetEntity, string TargetInput, float FireDelay, string Parameters );

	public struct Output
	{
		public Output( GameEntity entity, string name, List<OutputEntry> entries )
		{
			Name = name;
			Entity = entity;
			Entries = entries;
		}

		public static Output FromKeyvalue( Entity entity, string key, string value )
		{
			return new( entity.Ref<GameEntity>(), key, [] );
		}

		public string Name { get; }
		public GameEntity Entity { get; }
		public List<OutputEntry> Entries { get; set; } = new();

		public void Fire()
		{
			foreach ( var entry in Entries )
			{
				Console.WriteLine( $"Firing {Entity.Name}'s output '{Name}' on entity '{entry.TargetEntity}' at '{entry.TargetInput}' ({entry.FireDelay} seconds of delay)" );
			}
		}
	}

	public static partial class ComponentRegistry
	{
		/// <summary>
		/// This gets autogenerated by EcsResearch.Generator.
		/// </summary>
		public static partial bool ParseComponentKeyvalue( Entity entity, ReadOnlySpan<char> key, string value );

		/// <summary>
		/// Creates a component, keeping in mind <see cref="RequiresAttribute{T}"/> i.e. component dependencies.
		/// </summary>
		public static partial ref T Create<T>( Entity entity ) where T : notnull, new();

		public static Output ParseOutput( Entity entity, ReadOnlySpan<char> key, string value )
			=> Output.FromKeyvalue( entity, key.ToString(), value );
	}

	// Entity events
	public delegate void OnSpawnFn( GameEntity entity );
	public delegate void OnUseFn( GameEntity entity, GameEntity user );
	public delegate void OnDamageFn( GameEntity entity, GameEntity attacker, GameEntity inflictor, float damageValue );

	// Physics events
	public delegate void OnCollideStartFn( GameEntity entity, GameEntity collider );
	public delegate void OnCollideHoldFn( GameEntity entity, GameEntity collider );
	public delegate void OnCollideStopFn( GameEntity entity, GameEntity collider );
	public delegate void OnTriggerStartFn( GameEntity entity, GameEntity collider );
	public delegate void OnTriggerHoldFn( GameEntity entity, GameEntity collider );
	public delegate void OnTriggerStopFn( GameEntity entity, GameEntity collider );

	public class GameEntity
	{
		public GameEntity( World world, Dictionary<string, string>? data )
		{
			EcsEntity = world.Spawn()
				.Add( this );

			if ( data is null )
			{
				return;
			}

			foreach ( var pair in data )
			{
				switch ( pair.Key )
				{
					case "classname": break;
					case "targetname": Name = pair.Value; break;
					case "origin": Transform.Position = Vector3.Zero; break;
					case "angles": Transform.Orientation = Quaternion.Identity; break;
					default: ComponentRegistry.ParseComponentKeyvalue( EcsEntity, pair.Key, pair.Value ); break;
				}
			}
		}

		public ref T Get<T>()
		{
			return ref EcsEntity.Ref<T>();
		}

		public bool Has<T>() where T: notnull
		{
			return EcsEntity.Has<T>();
		}

		public Entity EcsEntity { get; set; }

		public string Name { get; set; }

		public event OnSpawnFn? OnSpawn;
		public event OnUseFn? OnUse;
		public event OnDamageFn? OnDamage;
		public event OnCollideStartFn? OnCollideStart;
		public event OnCollideHoldFn? OnCollideHold;
		public event OnCollideStopFn? OnCollideStop;
		public event OnTriggerStartFn? OnTriggerStart;
		public event OnTriggerHoldFn? OnTriggerHold;
		public event OnTriggerStopFn? OnTriggerStop;

		public ref AudioSource AudioSource => ref EcsEntity.Ref<AudioSource>();
		public ref Transform Transform => ref EcsEntity.Ref<Transform>();
	}
	#endregion

	public enum AudioChannel
	{
		Dynamic,
		Ambient1,
		Ambient2,
		Ambient3,
		Ambient4,
		Body1,
		Body2,
		Body3,
		Body4,
		Item1,
		Item2,
		Item3,
		Item4,
	}

	[MapComponent]
	public struct AudioSource
	{
		public void PlaySound( string soundScript, AudioChannel channel = AudioChannel.Dynamic, bool fromStart = false )
		{

		}

		public void PauseSound( AudioChannel channel = AudioChannel.Dynamic )
		{

		}

		public void StopSound( AudioChannel channel = AudioChannel.Dynamic )
		{

		}
	}

	public enum DoorState
	{
		Closing,
		WasClosing,
		Closed,
		Opening,
		WasOpening,
		Open
	}

	[Requires<AudioSource>]
	[Requires<Transform>]
	[MapComponent]
	public struct Door
	{
		public Door() { }

		#region Properties
		public float OpenAngle { get; set; } = 90.0f;
		public float InitialAngle { get; set; } = 0.0f;
		public float Speed { get; set; } = 90.0f;
		public float TravelFraction { get; set; } = 0.0f;
		public DoorState State { get; set; } = DoorState.Closed;
		public bool IsOpen => State == DoorState.Open;
		public bool IsPartiallyOpen => State == DoorState.Open || State == DoorState.WasOpening || State == DoorState.WasClosing;
		public bool IsMoving => State == DoorState.Closing || State == DoorState.Opening;
		public bool NeedsUpdate => IsMoving;
		#endregion

		#region Utilities
		public void PlayDoorSound( GameEntity entity, bool open )
		{
			entity.AudioSource.PlaySound( "Door.Moving1", AudioChannel.Body1, true );
			entity.AudioSource.PlaySound( open ? "Door.Open1" : "Door.Close1", AudioChannel.Body2, true );
		}
		public void ContinueMovingSound( GameEntity entity )
		{
			entity.AudioSource.PlaySound( "Door.Moving1", AudioChannel.Body1 );
		}
		public void PauseMovingSound( GameEntity entity )
		{
			entity.AudioSource.PauseSound( AudioChannel.Body1 );
		}
		public void StopDoorSound( GameEntity entity, bool open )
		{
			PauseMovingSound( entity );
			entity.AudioSource.PlaySound( open ? "Door.OpenFinish1" : "Door.OpenFinish2", AudioChannel.Body2, true );
		}
		#endregion

		#region Events
		[Input]
		public void Open( GameEntity entity )
		{
			if ( State == DoorState.Open || State == DoorState.Opening )
			{
				return;
			}

			DoorState oldState = State;
			State = DoorState.Opening;

			HandleStateChange( oldState, entity );
		}

		[Input]
		public void Close( GameEntity entity )
		{
			if ( State == DoorState.Closed || State == DoorState.Closing )
			{
				return;
			}

			DoorState oldState = State;
			State = DoorState.Closing;

			HandleStateChange( oldState, entity );
		}

		[Input]
		public void Stop( GameEntity entity )
		{
			if ( !IsMoving )
			{
				return;
			}

			DoorState oldState = State;
			State = oldState == DoorState.Opening ? DoorState.WasOpening : DoorState.WasClosing;

			HandleStateChange( oldState, entity );
		}

		public void WhenOpenStart( GameEntity entity )
		{
			PlayDoorSound( entity, true );
			OnOpenStart.Fire();
		}

		public void WhenCloseStart( GameEntity entity )
		{
			PlayDoorSound( entity, false );
			OnCloseStart.Fire();
		}

		public Output OnOpenStart { get; set; }
		public Output OnCloseStart { get; set; }
		public Output OnOpenEnd { get; set; }
		public Output OnCloseEnd { get; set; }

		public void WhenOpenPause( GameEntity entity ) => PauseMovingSound( entity );

		public void WhenClosePause( GameEntity entity ) => PauseMovingSound( entity );

		public void WhenOpenContinue( GameEntity entity ) => ContinueMovingSound( entity );

		public void WhenCloseContinue( GameEntity entity ) => ContinueMovingSound( entity );

		public void WhenOpenFinish( GameEntity entity )
		{
			StopDoorSound( entity, true );
			OnOpenEnd.Fire();
		}

		public void WhenCloseFinish( GameEntity entity )
		{
			StopDoorSound( entity, false );
			OnCloseEnd.Fire();
		}
		#endregion

		public void HandleStateChange( DoorState oldState, GameEntity entity )
		{
			switch ( (oldState, State) )
			{
				// Was closed, now it's opening
				case (DoorState.Closed, DoorState.Opening): WhenOpenStart( entity ); break;
				// Was open, now it's closing
				case (DoorState.Open, DoorState.Closing): WhenCloseStart( entity ); break;
				// Was opening, now it's stopped
				case (DoorState.Opening, DoorState.WasOpening): WhenOpenPause( entity ); break;
				// Was Closing, now it's stopped
				case (DoorState.Closing, DoorState.WasClosing): WhenClosePause( entity ); break;

				// When you open/close the door midway
				case (DoorState.WasOpening, DoorState.Opening): WhenOpenContinue( entity ); break;
				case (DoorState.WasOpening, DoorState.Closing): WhenOpenContinue( entity ); break;
				case (DoorState.WasClosing, DoorState.Opening): WhenCloseContinue( entity ); break;
				case (DoorState.WasClosing, DoorState.Closing): WhenCloseContinue( entity ); break;

				// Door fully opened/closed normally
				case (DoorState.Open, DoorState.Opening): WhenOpenFinish( entity ); break;
				case (DoorState.Open, DoorState.WasOpening): WhenOpenFinish( entity ); break;
				case (DoorState.Closed, DoorState.Closing): WhenCloseFinish( entity ); break;
				case (DoorState.Closed, DoorState.WasClosing): WhenCloseFinish( entity ); break;
			}

			if ( oldState != State )
			{
				Console.WriteLine( $"Old state: {oldState}, new state: {State}" );
			}
		}

		private static Stream<GameEntity, Transform, Door>? mProcessStream = null;
		public static void Process( World world, float deltaTime )
		{
			if ( mProcessStream is null )
			{
				mProcessStream = world.Query<GameEntity, Transform, Door>().Stream();
			}

			mProcessStream.For( ( ref GameEntity entity, ref Transform transform, ref Door door ) =>
			{
				if ( !door.NeedsUpdate )
				{
					return;
				}

				DoorState oldState = door.State;

				float fractionTraveled = deltaTime * (door.Speed / door.OpenAngle);

				door.TravelFraction += door.State switch
				{
					DoorState.Closing => -fractionTraveled,
					DoorState.Opening => fractionTraveled,
					_ => 0.0f
				};

				(door.State, door.TravelFraction) = door.TravelFraction switch
				{
					>= 1.0f => (DoorState.Open, 1.0f),
					<= 0.0f => (DoorState.Closed, 0.0f),
					_ => (door.State, door.TravelFraction)
				};

				door.HandleStateChange( oldState, entity );

				// Update entity transform with this
				transform.SetYaw( door.InitialAngle + door.OpenAngle * door.TravelFraction );
				Console.WriteLine( $"Angle: {door.InitialAngle + door.OpenAngle * door.TravelFraction}" );
			} );
		}
	}

	[MapComponent]
	public struct Trigger
	{
		public Trigger() { }

		public bool Once { get; set; }
		public Output OnClientEnter { get; set; }
	}

	public struct Transform
	{
		public Transform() { }

		public Vector3 Position { get; set; } = Vector3.Zero;
		public Quaternion Orientation { get; set; } = Quaternion.Identity;

		public void SetYaw( float degrees )
		{
			Orientation = Quaternion.CreateFromYawPitchRoll( degrees, 0.0f, 0.0f );
		}
	}

	internal class Program
	{
		static void Main( string[] args )
		{
			World world = new();
			GameEntity triggerEntity = new( world, new()
			{
				{ "targetname", "bob" },
				{ "Trigger.Once", "1" },
				{ "@Trigger.OnClientEnter", "billy.Door.Open" }
			} );

			GameEntity doorEntity = new( world, new()
			{
				{ "targetname", "billy" },
				{ "Door.OpenAngle", "120" },
				{ "Door.Speed", "180" }
			} );

			// TODO: add a physics engine and spawn a player inside the trigger volume :)
			//triggerEntity.FireOutput<Trigger>( "OnClientEnter" );
			triggerEntity.Get<Trigger>().OnClientEnter.Fire();

			for ( int i = 0; i < 100; i++ )
			{
				Door.Process( world, 0.016f );
				Console.WriteLine( $"TravelFraction: {doorEntity.EcsEntity.Ref<Door>().TravelFraction}" );
			}
		}
	}
}
