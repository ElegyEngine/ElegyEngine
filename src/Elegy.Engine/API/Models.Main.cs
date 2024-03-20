// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Engine.Interfaces;

namespace Elegy.Engine.API
{
	public static partial class Models
	{
		public static Model? LoadModel( string path )
		{
			throw new NotImplementedException();
		}

		public static IModelLoader? FindModelLoader( params string[] extensions )
		{
			throw new NotImplementedException();
		}

		public static IReadOnlyList<Model> AllModels { get; }
	}
}
