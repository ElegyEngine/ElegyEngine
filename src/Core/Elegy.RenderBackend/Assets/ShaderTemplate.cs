// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.RenderBackend.Assets
{
	public class ShaderTemplate
	{
		public string Name { get; set; } = string.Empty;

		public string ShaderBinaryBasePath { get; set; } = string.Empty;

		public bool PostprocessHint { get; set; } = false;

		public List<MaterialParameterSet> ParameterSets { get; set; } = new();

		public List<ShaderVariantEntry> ShaderVariants { get; set; } = new();
	}
}
