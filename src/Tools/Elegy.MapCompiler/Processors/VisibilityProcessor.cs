// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.MapCompiler.Assets;

namespace Elegy.MapCompiler.Processors
{
	public class VisibilityProcessor
	{
		public ElegyMapDocument Data { get; }
		public MapCompilerParameters Parameters { get; }

		public VisibilityProcessor( ElegyMapDocument data, MapCompilerParameters parameters )
		{
			Data = data;
			Parameters = parameters;
		}
	}
}
