// SPDX-FileCopyrightText: 2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy.MapCompiler.Data.Processing
{
	public class ProcessingData
	{
		public Box3 MapBoundaries { get; set; } = new();
		public List<Entity> Entities { get; set; } = new();
	}
}
