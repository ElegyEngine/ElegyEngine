// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy
{
	public static partial class Render
	{
		internal static void SetRenderFrontend( IRenderFrontend? renderFrontend )
			=> mRenderFrontend = renderFrontend;

		private static IRenderFrontend? mRenderFrontend;
	}
}
