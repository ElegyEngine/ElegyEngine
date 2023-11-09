// SPDX-FileCopyrightText: 2023 Admer Šuko
// SPDX-License-Identifier: MIT

using Elegy.MapCompiler.Assets;
using Elegy.MapCompiler.Data.Processing;

namespace Elegy.MapCompiler.Processors
{
	public class LightProcessor
	{
		public ProcessingData Data { get; }
		public MapCompilerParameters Parameters { get; }

		public LightProcessor( ProcessingData data, MapCompilerParameters parameters )
		{
			Data = data;
			Parameters = parameters;
		}
	}
}
