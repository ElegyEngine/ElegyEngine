// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Veldrid;

namespace Elegy.RenderBackend
{
	public enum RasterPreset
	{
		/// <summary>
		/// Backface culling, depth testing.
		/// </summary>
		Opaque,

		/// <summary>
		/// No culling, depth testing.
		/// </summary>
		OpaqueTwoSided,

		/// <summary>
		/// Frontface culling, depth testing.
		/// </summary>
		OpaqueReverse,

		/// <summary>
		/// Backface culling, no depth testing.
		/// </summary>
		NoDepth,

		/// <summary>
		/// No culling, no depth testing.
		/// </summary>
		NoDepthTwoSided,

		/// <summary>
		/// Frontface culling, no depth testing.
		/// </summary>
		NoDepthReverse,
	}

	public struct RenderPipeline : IDisposable
	{
		public Pipeline Pipeline { get; }
		public Shader[] Shaders { get; }
		public RasterPreset RasterPreset { get; }
		public ResourceLayout[] ResourceLayouts { get; }

		public RenderPipeline( Pipeline pipeline, Shader[] shaders, RasterPreset rasterPreset, ResourceLayout[] resourceLayouts )
		{
			Pipeline = pipeline;
			Shaders = shaders;
			RasterPreset = rasterPreset;
			ResourceLayouts = resourceLayouts;
		}

		public void Dispose()
		{
			Pipeline.Dispose();

			for ( int i = 0; i < ResourceLayouts.Length; i++ )
			{
				ResourceLayouts[i].Dispose();
			}

			for ( int i = 0; i < Shaders.Length; i++ )
			{
				Shaders[i].Dispose();
			}
		}
	}
}
