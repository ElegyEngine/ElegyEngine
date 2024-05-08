// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.RenderBackend.Assets
{
	public class MaterialParameter
	{
		public string Name { get; set; } = string.Empty;

		public string ShaderName { get; set; } = string.Empty;

		public ShaderDataType Type { get; set; } = ShaderDataType.Float;

		public int ResourceSetId { get; set; } = 0;
	}

	public class ShaderTemplate
	{
		public string Name { get; set; } = string.Empty;

		public string ShaderBinaryBasePath { get; set; } = string.Empty;

		public bool PostprocessHint { get; set; } = false;

		public List<MaterialParameter> Parameters { get; set; } = new();

		public List<ShaderTemplateEntry> ShaderVariants { get; set; } = new();
	}
}
