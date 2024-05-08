// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.RenderBackend.Assets;

namespace Elegy.RenderBackend.Extensions
{
	public static class ShaderDataTypeExtensions
	{
		public static bool IsTexture( this ShaderDataType type )
			=> type switch
			{
				ShaderDataType.Texture1D => true,
				ShaderDataType.Texture2D => true,
				ShaderDataType.Texture3D => true,
				_ => false
			};
	}

}
