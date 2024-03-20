// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;

namespace Elegy.MapCompiler.Data.Processing
{
	public class Material
	{
		public string Name { get; set; } = string.Empty;
		public int Width { get; set; } = 0;
		public int Height { get; set; } = 0;
		public ToolMaterialFlag Flags { get; set; } = ToolMaterialFlag.None;

		public bool HasFlag( ToolMaterialFlag flag ) => Flags.HasFlag( flag );
	}
}
