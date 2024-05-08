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

		[PathParam( "-root" )]
		public string RootPath { get; set; } = string.Empty;

		[BoolParam("-debugfreeze")]
		public bool DebugFreeze { get; set; } = false;
	}
}
