// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Common.Interfaces.Rendering
{
	/// <summary></summary>
	public interface ITexture
	{
		/// <summary></summary>
		int Width { get; }

		/// <summary></summary>
		int Height { get; }

		/// <summary></summary>
		int Depth { get; }

		/// <summary></summary>
		Span<byte> ReadPixels();

		/// <summary></summary>
		void UpdatePixels( Span<byte> newPixels );
	}
}
