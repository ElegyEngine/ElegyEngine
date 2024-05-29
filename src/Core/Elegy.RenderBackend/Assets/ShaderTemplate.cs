// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.RenderBackend.Assets
{
	public class ResourceLayoutElementEntry
	{
		public int Binding { get; set; } = 0;

		public string Name { get; set; } = string.Empty;

		public ShaderDataType Type { get; set; } = ShaderDataType.Buffer;
	}

	public class ResourceLayoutEntry
	{
		public int Set { get; set; } = 0;

		public List<ResourceLayoutElementEntry> Elements { get; set; } = new();

		public MaterialParameterLevel Level { get; set; } = MaterialParameterLevel.Builtin;
	}

	public class ShaderTemplate
	{
		public string Name { get; set; } = string.Empty;

		public string ShaderBinaryBasePath { get; set; } = string.Empty;

		public bool PostprocessHint { get; set; } = false;

		public List<MaterialParameterSet> ParameterSets { get; set; } = new();

		public List<ShaderVariantEntry> ShaderVariants { get; set; } = new();

		public List<ResourceLayoutEntry> ResourceLayouts { get; set; } = new();
	}
}
