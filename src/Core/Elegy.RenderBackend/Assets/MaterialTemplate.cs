// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using FaceCullMode = Veldrid.FaceCullMode;

namespace Elegy.RenderBackend.Assets
{
	public enum Blending
	{
		Opaque,
		Transparent,
		Additive,
		Multiply,
		MiddleGray
	}

	public class PipelineInfo
	{
		public FaceCullMode FaceCulling { get; set; } = FaceCullMode.Back;

		public Blending BlendMode { get; set; } = Blending.Opaque;

		public bool DepthTest { get; set; } = true;
		
		public bool AlphaTest { get; set; } = false;

		public float AlphaTestThreshold { get; set; } = 0.5f;
	}

	public class MaterialTemplate
	{
		public string Name { get; set; } = string.Empty;

		public string ShaderTemplate { get; set; } = string.Empty;

		public string? Polyshader { get; set; }

		public PipelineInfo PipelineInfo { get; set; } = new();
	}
}
