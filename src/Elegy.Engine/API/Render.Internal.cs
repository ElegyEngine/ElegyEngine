// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Engine.Interfaces;

namespace Elegy.Engine.API
{
	public static partial class Render
	{
		internal static void SetRenderFrontend( IRenderFrontend? renderFrontend )
			=> mRenderFrontend = renderFrontend;

		private static IRenderFrontend? mRenderFrontend;
	}
}
