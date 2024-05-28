// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Interfaces;
using Elegy.RenderSystem.Objects;

using Veldrid;

namespace Elegy.RenderSystem.Interfaces
{
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
		void RenderBatches( CommandList renderCommand, View view, ReadOnlySpan<Batch> billboards, ReadOnlySpan<Light> lights );

		/// <summary>Draw one or multiple billboards, illuminated by one or more lights.</summary>
		void RenderBillboards( CommandList renderCommand, View view, ReadOnlySpan<Billboard> billboards, ReadOnlySpan<Light> lights );

		/// <summary>Draw one or multiple mesh entities, illuminated by one or more lights.</summary>
		void RenderMeshEntities( CommandList renderCommand, View view, ReadOnlySpan<MeshEntity> entities, ReadOnlySpan<Light> lights );

		/// <summary>Draw a single volume, illuminated by one or more lights.</summary>
		void RenderVolume( CommandList renderCommand, View view, Volume volume, ReadOnlySpan<Light> lights );
	}
}
