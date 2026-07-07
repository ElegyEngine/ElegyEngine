// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using BepuPhysics.CollisionDetection;

namespace Game.Shared.PhysicsSystem.Extensions
{
	public static class ManifoldAndContactExtensions
	{
		/// <summary>
		/// Extracts contact data from a manifold. Assumes 4 output contacts.
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveOptimization )]
		public static int ExtractContacts<TManifold>( this TManifold manifold, Span<Contact> output )
			where TManifold : unmanaged, IContactManifold<TManifold>
		{
			int numContacts = manifold.Count;

			// FeatureID of MinValue means the contact is invalid
			output[0].FeatureId = int.MinValue;
			output[1].FeatureId = int.MinValue;
			output[2].FeatureId = int.MinValue;
			output[3].FeatureId = int.MinValue;

			for ( int i = 0; i < numContacts; i++ )
			{
				manifold.GetContact( i, out Contact contact );
				output[i] = contact;
			}

			return numContacts;
		}

		public static bool IsValid( this Contact contact )
			=> contact.FeatureId != int.MinValue;
	}
}
