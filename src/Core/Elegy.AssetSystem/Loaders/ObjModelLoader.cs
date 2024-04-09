// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.AssetSystem.Interfaces;

namespace Elegy.AssetSystem.Loaders
{
	/// <summary>
	/// Built-in OBJ loader.
	/// </summary>
	public class ObjModelLoader : BaseAssetLoader, IModelLoader
	{
		/// <inheritdoc/>
		public override string Name => "ObjModelLoader";

		/// <inheritdoc/>
		public override bool CanLoad( string path )
			=> path == ".obj";

		/// <inheritdoc/>
		public Model? LoadModel( string path )
		{
			throw new NotImplementedException();
		}
	}
}
