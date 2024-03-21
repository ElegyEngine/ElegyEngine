// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Engine.Interfaces;

namespace Elegy.Engine.API
{
	public static partial class Assets
	{
		public static Model? LoadModel( string path )
			=> mAssetSystem.LoadModel( path );

		public static IModelLoader? FindModelLoader( params string[] extensions )
			=> mAssetSystem.FindModelLoader( extensions );

		public static IReadOnlyCollection<Model> AllModels
			=> mAssetSystem.GetModelList();
	}
}
