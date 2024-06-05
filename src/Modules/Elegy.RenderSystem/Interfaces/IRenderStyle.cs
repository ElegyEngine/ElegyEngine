// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Interfaces;
using Elegy.RenderBackend;
using Elegy.RenderSystem.Objects;
using Elegy.RenderSystem.Resources;
using Veldrid;

namespace Elegy.RenderSystem.Interfaces
{
	/// <summary>
	/// A render surface, nice for rendering in batches.
	/// It represents one draw call essentially: a mesh to render, together with shader parametres.
	/// </summary>
	public struct RenderSurface
	{
		/// <summary> The render mesh. </summary>
		public ArrayMesh Mesh;
		/// <summary> The parameter pool. </summary>
		public MaterialParameterPool ParameterPool;
		/// <summary> The per-entity resource set. </summary>
		public ResourceSet PerEntitySet;
	}

	/// <summary>
	/// Renderer frontend plugin. Implements rendering techniques for a particular graphical style.
	/// </summary>
	public interface IRenderStyle : IPlugin
	{
		/// <summary>Extra instance extensions.</summary>
		string[] InstanceExtensions { get; }

		/// <summary>Extra device extensions, e.g. barycentric coordinates.</summary>
		string[] DeviceExtensions { get; }

		/// <summary>Creates and fills pipelines.</summary>
		bool CreateCorePipelines();

		/// <summary>Draw one or multiple batches, illuminated by one or more lights.</summary>
		void RenderBatches( CommandList renderCommand, View view, ReadOnlySpan<Batch> batches, ReadOnlySpan<Light> lights );

		/// <summary>Draw one or multiple billboards, illuminated by one or more lights.</summary>
		void RenderBillboards( CommandList renderCommand, View view, ReadOnlySpan<Billboard> billboards, ReadOnlySpan<Light> lights );

		/// <summary>Draw one or multiple surfaces, illuminated by one or more lights.</summary>
		void RenderSurfaces( CommandList renderCommand, View view, ReadOnlySpan<RenderSurface> surfaces, RenderMaterial material, ReadOnlySpan<Light> lights );

		/// <summary>Draw a single volume, illuminated by one or more lights.</summary>
		void RenderVolume( CommandList renderCommand, View view, Volume volume, ReadOnlySpan<Light> lights );
	}
}
