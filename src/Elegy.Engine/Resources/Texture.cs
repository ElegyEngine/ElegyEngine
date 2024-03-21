// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Engine.Interfaces.Rendering;

namespace Elegy.Engine.Resources
{
	/// <summary></summary>
	public class Texture
	{
		/// <summary></summary>
		public Texture FromData( int width, int height, Span<byte> bytes )
		{

		}

		/// <summary></summary>
		public int Width { get; }
		/// <summary></summary>
		public int Height { get; }

		/// <summary></summary>
		public ITexture? RenderTexture { get; } = null;
	}
}
