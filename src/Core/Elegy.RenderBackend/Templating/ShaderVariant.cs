// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.RenderBackend.Assets;
using Veldrid;

namespace Elegy.RenderBackend.Templating
{
	public struct VariantVertexAttribute
	{
		public VertexSemantic Semantic;
		public int Channel;
	}

	public class ShaderVariant
	{
		public ShaderVariant( MaterialTemplate parent, ShaderVariantEntry data, ResourceLayout[] layouts,
			Shader vertexShader, Shader pixelShader, Pipeline pipeline,
			VariantVertexAttribute[] attributes, int[] mappings,
			int[] perInstanceMappings, int[] globalMappings )
		{
			Data = data;
			Template = parent;
			Layouts = layouts;
			VertexShader = vertexShader;
			PixelShader = pixelShader;
			Pipeline = pipeline;
			VertexAttributes = attributes;
			ResourceMappingsGlobal = globalMappings;
			ResourceMappingsPerMaterial = mappings;
			ResourceMappingsPerInstance = perInstanceMappings;
		}

		public int[] ResourceMappingsGlobal { get; }

		public int[] ResourceMappingsPerMaterial { get; }

		public int[] ResourceMappingsPerInstance { get; }

		public VariantVertexAttribute[] VertexAttributes { get; }

		public MaterialTemplate Template { get; }

		public ResourceLayout[] Layouts { get; }

		public Shader VertexShader { get; }

		public Shader PixelShader { get; }

		public Pipeline Pipeline { get; }

		public ShaderVariantEntry Data { get; }
	}
}
