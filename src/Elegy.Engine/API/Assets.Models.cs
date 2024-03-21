// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Engine.Interfaces;

namespace Elegy.Engine.API
{
	public static partial class Assets
	{
		/// <summary>
		/// Loads a model from the path.
		/// </summary>
		public static Model? LoadModel( string path )
			=> mAssetSystem.LoadModel( path );

		/// <summary>
		/// Finds an appropriate <see cref="IModelLoader"/> according to one of the <paramref name="extensions"/>.
		/// </summary>
		public static IModelLoader? FindModelLoader( params string[] extensions )
			=> mAssetSystem.FindModelLoader( extensions );

		/// <summary>
		/// A collection of all loaded models.
		/// </summary>
		public static IReadOnlyCollection<Model> AllModels
			=> mAssetSystem.GetModelList();
	}
}
