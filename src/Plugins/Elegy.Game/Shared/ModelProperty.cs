// SPDX-FileCopyrightText: 2024-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.ECS;
using Game.Server;

namespace Game.Shared
{
	public struct ModelProperty : IEntityProperty
	{
		public string Name { get; private set; }
		public Model? Data { get; private set; }

		public ModelProperty()
		{
		}

		public ModelProperty( string name )
		{
			Name = name;
			Data = AssetCache.LoadModel( name );
			AssetCache.LinkModel( name );
		}

		public void ParseEntityProperty( fennecs.Entity entity, ReadOnlySpan<char> key, ReadOnlySpan<char> value )
		{
			this = new( value.ToString() );
		}

		public void SetModel( string name )
		{
			AssetCache.UnlinkModel( Name );
			this = new( name );
		}
	}
}
