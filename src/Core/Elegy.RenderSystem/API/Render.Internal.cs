// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.ConsoleSystem;
using Elegy.Engine.Dummies;
using Elegy.Engine.Interfaces;

namespace Elegy.RenderSystem.API
{
	public static partial class Render
	{
		private static TaggedLogger mLogger = new( "Render" );
		private static IRenderFrontend mFrontend = new RenderNull()
	}
}
