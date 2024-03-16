// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.ShaderTool
{
	public enum ShaderKind
	{
		Vertex,
		Pixel,
		Compute
	}

	public class ShaderPermutation
	{
		public ShaderPermutation( string vertexContents, string pixelContents, string variant )
		{
			Variant = variant;
			VertexContents = vertexContents;
			PixelContents = pixelContents;
		}

		public string Variant { get; }
		public string VertexContents { get; }
		public string PixelContents { get; }
	}
}
