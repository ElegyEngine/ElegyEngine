// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.API;
using Elegy.Common.Assets;
using Elegy.Common.Assets.MeshData;
using Elegy.ConsoleSystem;
using Game.Shared;

namespace Game.Server
{
	/// <summary>
	/// Handles asset precaching on the server so it can be sent to clients.
	/// </summary>
	public static class AssetCache
	{
		public static AssetRegistry Registry { get; } = new();
		public static ElegyMapDocument MapDocument { get; private set; }

		public static Dictionary<string, int> ModelRefs { get; } = new();
		public static Dictionary<string, int> MaterialRefs { get; } = new();
		public static Dictionary<string, int> SoundRefs { get; } = new();
		public static Dictionary<string, int> OtherFileRefs { get; } = new();

		public static void InitLevel( ElegyMapDocument mapDocument )
		{
			MapDocument = mapDocument;
		}

		public static Model? LoadModel( string name )
		{
			if ( Registry.Models.TryGetValue( name, out Model? model ) )
			{
				if ( model is not null )
				{
					return model;
				}
			}

			model = Assets.LoadModel( name );
			Registry.Models[name] = model;
			return model;
		}

		public static void LinkModel( string name )
		{
			if ( ModelRefs.TryGetValue( name, out int count ) )
			{
				ModelRefs[name] = count + 1;
			}
		}

		public static void UnlinkModel( string name )
		{
			if ( ModelRefs.TryGetValue( name, out int count ) )
			{
				ModelRefs[name] = count - 1;
			}
		}
	}
}
