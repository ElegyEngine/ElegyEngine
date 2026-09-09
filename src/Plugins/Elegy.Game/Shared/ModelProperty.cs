// SPDX-FileCopyrightText: 2024-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.ECS;
using Game.Server;

namespace Game.Shared
{
	/// <summary>
	/// Handles model loading for entities. Embedded brush models, GLTF models and the like.
	/// </summary>
	public struct ModelProperty : IEntityProperty<ModelProperty>
	{
		public string Name { get; private set; }
		public Model? Data { get; private set; }
		public bool IsBrushModel => Name.StartsWith( '*' );
		public bool IsBrushCollisionModel => Name.StartsWith( "*c" );

		public static ModelProperty BrushVisual( int renderMeshId )
			=> new()
			{
				Name = $"*{renderMeshId}",
				Data = AssetCache.LoadModel( $"*{renderMeshId}" )
			};

		public static ModelProperty BrushCollision( int collisionMeshId )
			=> new()
			{
				Name = $"*c{collisionMeshId}",
				Data = AssetCache.LoadCollisionModel( collisionMeshId )
			};

		public static ModelProperty Generic( ReadOnlySpan<char> path )
			=> new()
			{
				Name = $"{path}",
				Data = AssetCache.LoadModel( path.ToString(), incrementLinks: true )
			};

		public static ModelProperty ParseEntityProperty( ref Entity entity, ReadOnlySpan<char> key, ReadOnlySpan<char> value )
			=> Generic( value );

		public void SetModel( string name )
		{
			if ( !IsBrushModel )
			{
				AssetCache.UnlinkModel( Name );
			}

			Name = name;
			if ( IsBrushModel )
			{
				int brushModelId = int.Parse( Name[1..] );

				if ( IsBrushCollisionModel )
				{
					this = BrushCollision( brushModelId );
				}
				else
				{
					this = BrushVisual( brushModelId );
				}
			}
			else
			{
				this = Generic( Name );
			}
		}
	}
}
