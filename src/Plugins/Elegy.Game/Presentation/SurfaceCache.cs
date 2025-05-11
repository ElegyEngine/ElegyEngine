using Collections.Pooled;
using Elegy.RenderSystem.Interfaces;
using Elegy.RenderSystem.Resources;

namespace Game.Presentation
{
	public class SurfaceCache
	{
		public struct CacheElement
		{
			public RenderMaterial Material;
			public PooledList<RenderSurface> Surfaces;
		}
		
		public PooledList<CacheElement> Cache { get; } = new( capacity: 512 );

		public PooledList<RenderSurface> GetOrAdd( RenderMaterial material )
		{
			var span = Cache.Span;
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
			var span = Cache.Span;
			for ( int i = 0; i < span.Length; i++ )
			{
				span[i].Surfaces.Clear();
			}
		}
	}
}
