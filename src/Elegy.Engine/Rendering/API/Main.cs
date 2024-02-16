// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy
{
	/// <summary>
	/// Elegy renderer interface.
	/// </summary>
	public static partial class Render
	{
		/// <summary>
		/// Gets the current render frontend instance.
		/// Temporary until we work out the API.
		/// </summary>
		public static IRenderFrontend Instance => mRenderFrontend;
	}
}
