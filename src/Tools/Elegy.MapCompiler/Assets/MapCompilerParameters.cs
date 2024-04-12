// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.MapCompiler.ConsoleArguments;

namespace Elegy.MapCompiler.Assets
{
	public class MapCompilerParameters
	{
		[PathParam( "-map" )]
		public string MapFile { get; set; } = string.Empty;

		[PathParam( "-out" )]
		public string OutputPath { get; set; } = string.Empty;

		[PathParam( "-gamedirectory" )]
		public string GameDirectory { get; set; } = string.Empty;

		[FloatParam( "-debugfreeze", minValue: 0.0f, maxValue: 60.0f )]
		public float DebugFreeze { get; set; } = 0.0f;
	}
}
