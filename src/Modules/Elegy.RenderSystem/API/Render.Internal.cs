// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.ConsoleSystem;
using Elegy.RenderSystem.Dummies;
using Elegy.RenderSystem.Interfaces;

namespace Elegy.RenderSystem.API
{
	public static partial class Render
	{
		private static TaggedLogger mLogger = new( "Render" );
		private static IRenderFrontend mFrontend = new RenderNull();

		internal static void SetFrontend( IRenderFrontend frontend )
		{
			mFrontend = frontend;

			Assets.SetRenderFactories(
				Instance.CreateMaterial,
				( textureInfo, data ) => Instance.CreateTexture( textureInfo, data.AsSpan() ) );
		}
	}
}
