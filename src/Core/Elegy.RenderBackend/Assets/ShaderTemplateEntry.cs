// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.RenderBackend.Assets
{
	public enum ShaderDataType
	{
		Byte,
		Short,
		Int,
		Float,
		Vec2,
		Vec3,
		Vec4,
		Vec2Byte,
		Vec3Byte,
		Vec4Byte,
		Mat22,
		Mat33,
		Mat44,
		Buffer,
		BufferRW,
		Texture1D,
		Texture2D,
		Texture3D,
		Sampler
	}

	public enum MaterialParameterLevel
	{
		Builtin,
		Data,
		Global,
		Instance
	}

	public class VertexLayoutEntry
	{
		public string Name { get; set; } = string.Empty;

		public Veldrid.VertexElementFormat Type { get; set; } = Veldrid.VertexElementFormat.Float1;
	}

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

	public class ShaderTemplateEntry
	{
		public string ShaderDefine { get; set; } = string.Empty;

		public List<VertexLayoutEntry> VertexLayouts { get; set; } = new();

		public List<ResourceLayoutEntry> ResourceLayouts { get; set; } = new();
	}
}
