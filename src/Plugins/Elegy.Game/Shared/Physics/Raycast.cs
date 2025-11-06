// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Trees;
using BepuUtilities.Memory;
using Elegy.Common.Parallel;
using Parallel = Elegy.Common.Parallel.Parallel;

namespace Game.Shared.Physics
{
	public unsafe struct ConfigurableRayHitHandler : IRayHitHandler, ISweepHitHandler
	{
		public Span<RaycastResult> Results
		{
			set => ResultBase = (RaycastResult*)Unsafe.AsPointer( ref value[0] );
		}

		public IntPtr ResultPtr => (IntPtr)ResultBase;

		// DANGER: this handler must be pretty short-lived because of this pointer
		public RaycastResult* ResultBase;
		public CollidableReference Ignore;

		public bool AllowTest( CollidableReference collidable )
		{
			if ( collidable.RawHandleValue == Ignore.RawHandleValue )
			{
				return false;
			}

			return true;
		}

		public bool AllowTest( CollidableReference collidable, int childIndex )
		{
			if ( collidable.RawHandleValue == Ignore.RawHandleValue )
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Handles successful ray tests.
		/// </summary>
		public void OnRayHit( in RayData ray, ref float maximumT, float t, Vector3 normal, CollidableReference collidable, int childIndex )
		{
			maximumT = t; // Prevent testing past this point

			RaycastResult* result = ResultBase + ray.Id; // Support ray batching
			result->HitPosition = ray.Origin + ray.Direction * t;
			result->SurfaceNormal = normal;
			result->Distance = t;
			result->PhysicsHandle = collidable;
			result->ChildIndex = childIndex;
		}

		/// <summary>
		/// Handles successful shape tests.
		/// </summary>
		public void OnHit( ref float maximumT, float t, Vector3 hitLocation, Vector3 hitNormal, CollidableReference collidable )
		{
			maximumT = t; // Prevent testing past this point

			ResultBase->HitPosition = hitLocation;
			ResultBase->SurfaceNormal = hitNormal;
			ResultBase->Distance = t;
			ResultBase->PhysicsHandle = collidable;
			ResultBase->ChildIndex = 0;
		}

		public void OnHitAtZeroT( ref float maximumT, CollidableReference collidable )
		{
		}
	}

	public struct Ray
	{
		public Vector3 Start;
		public Vector3 Direction;
		public float MaxDistance;
	}

	public record struct RaycastResult( Vector3 HitPosition, Vector3 SurfaceNormal, float Distance, CollidableReference PhysicsHandle, int ChildIndex );

	public struct Raycast
	{
		public ConfigurableRayHitHandler RaycastHandler;

		public Raycast Ignore( CollidableReference physicsHandle )
		{
			RaycastHandler.Ignore = physicsHandle;
			return this;
		}

		public Raycast Ignore( BodyHandle bodyHandle )
		{
			CollidableMobility mobility = Physics.Simulation.Bodies[bodyHandle].Kinematic
				? CollidableMobility.Kinematic
				: CollidableMobility.Dynamic;
			RaycastHandler.Ignore = new( mobility, bodyHandle );
			return this;
		}

		public Raycast Ignore( StaticHandle staticHandle )
		{
			RaycastHandler.Ignore = new( staticHandle );
			return this;
		}

		public unsafe RaycastResult Fire( Vector3 start, Vector3 direction, float distance )
		{
			RaycastResult result = new();
			RaycastHandler.ResultBase = &result;
			Physics.Simulation.RayCast( start, direction, distance, Physics.Simulation.BufferPool, ref RaycastHandler );
			return result;
		}

	}
}