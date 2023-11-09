// SPDX-FileCopyrightText: 2023 Admer Šuko
// SPDX-License-Identifier: MIT

using Elegy.MapCompiler.Assets;
using Elegy.MapCompiler.Data.Processing;

namespace Elegy.MapCompiler.Processors
{
	public class VisibilityProcessor
	{
		public ProcessingData Data { get; }
		public MapCompilerParameters Parameters { get; }

		public VisibilityProcessor( ProcessingData data, MapCompilerParameters parameters )
		{
			Data = data;
			Parameters = parameters;
		}
	}
}
