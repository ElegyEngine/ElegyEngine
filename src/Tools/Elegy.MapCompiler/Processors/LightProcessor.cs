// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.ConsoleSystem;
using Elegy.MapCompiler.Assets;

namespace Elegy.MapCompiler.Processors
{
	public class LightProcessor
	{
		public ElegyMapDocument Data { get; }
		public MapCompilerParameters Parameters { get; }

		TaggedLogger mLogger = new( "Light" );

		public LightProcessor( ElegyMapDocument data, MapCompilerParameters parameters )
		{
			mLogger.Log( "Init" );

			Data = data;
			Parameters = parameters;
		}
	}
}
