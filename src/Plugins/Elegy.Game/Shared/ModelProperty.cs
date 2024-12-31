// SPDX-FileCopyrightText: 2024-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.ECS;
using Game.Server;

namespace Game.Shared
{
	public struct ModelProperty : IEntityProperty<ModelProperty>
	{
		public string Name { get; private set; }
		public Model? Data { get; private set; }

		public ModelProperty( ReadOnlySpan<char> value )
		{
			Init( value.ToString() );
		}
		
		public void Init( string name )
		{
			Name = name;
			Data = AssetCache.LoadModel( name );
			AssetCache.LinkModel( name );
		}

		public static ModelProperty ParseEntityProperty( ref fennecs.Entity entity, ReadOnlySpan<char> key, ReadOnlySpan<char> value )
			=> new( value );

		public void SetModel( string name )
		{
			AssetCache.UnlinkModel( Name );
			Init( name );
		}
	}
}
