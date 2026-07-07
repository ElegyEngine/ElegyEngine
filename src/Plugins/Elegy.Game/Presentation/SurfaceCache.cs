// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.RenderSystem.Interfaces;
using Elegy.RenderSystem.Resources;

namespace Game.Presentation
{
	public class SurfaceCache
	{
		public struct CacheElement
		{
			public RenderMaterial Material;
			public List<RenderSurface> Surfaces;
		}

		public List<CacheElement> Cache { get; } = new( capacity: 512 );

		public List<RenderSurface> GetOrAdd( RenderMaterial material )
		{
			var span = Cache.AsSpan();
			for ( int i = 0; i < span.Length; i++ )
			{
				if ( span[i].Material == material )
				{
					return span[i].Surfaces;
				}
			}

			Cache.Add( new()
			{
				Material = material,
				Surfaces = new( capacity: 128 )
			} );

			return Cache[^1].Surfaces;
		}

		public void Clear()
		{
			var span = Cache.AsSpan();
			for ( int i = 0; i < span.Length; i++ )
			{
				span[i].Surfaces.Clear();
			}
		}
	}
}
