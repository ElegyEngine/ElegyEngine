// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.RenderBackend.Assets
{
	public class VertexLayoutEntry
	{
		public string Name { get; set; } = string.Empty;

		public Veldrid.VertexElementFormat Type { get; set; } = Veldrid.VertexElementFormat.Float1;

		public int Id { get; set; } = 0;
	}

	public class ShaderVariantEntry
	{
		public string ShaderDefine { get; set; } = string.Empty;

		public string ShaderBinaryPath { get; set; } = string.Empty;

		/// <summary>
		/// Vertex inputs that are visible to this shader variant.
		/// </summary>
		public List<VertexLayoutEntry> VertexLayouts { get; set; } = new();

		/// <summary>
		/// IDs of parameter sets that are visible to this shader variant.
		/// </summary>
		public List<int> ParameterSetIds { get; set; } = new();
	}
}
