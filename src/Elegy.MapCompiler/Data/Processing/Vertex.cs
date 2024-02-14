// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.MapCompiler.Data.Processing
{
	public struct Vertex
	{
		public Vector3 Position { get; set; }
		public Vector3 Normal { get; set; }
		public Vector2 Uv { get; set; }
		public Vector4 Colour { get; set; }
	}
}
