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

	public class MaterialParameter
	{
		public string Name { get; set; } = string.Empty;

		public string ShaderName { get; set; } = string.Empty;

		public ShaderDataType Type { get; set; } = ShaderDataType.Float;

		public int ResourceBindingId { get; set; } = 0;
	}

	public class MaterialParameterSet
	{
		public int ResourceSetId { get; set; } = 0;

		public MaterialParameterLevel Level { get; set; } = MaterialParameterLevel.Builtin;

		public List<MaterialParameter> Parameters { get; set; } = new();
	}

	public class GlobalParameterSet
	{
		public List<MaterialParameter> Parameters { get; set; } = new();

		public override int GetHashCode()
		{
			return Parameters.GetHashCode();
		}
	}
}
