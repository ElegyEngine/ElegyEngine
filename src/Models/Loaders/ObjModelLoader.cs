// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

using Elegy.Assets;

namespace Elegy.Models.Loaders
{
	/// <summary>
	/// Built-in OBJ loader.
	/// </summary>
	public class ObjModelLoader : IModelLoader
	{
		/// <inheritdoc/>
		public bool CanLoad( string path )
		{
			if ( !path.EndsWith( ".obj" ) )
			{
				return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public Model? LoadModel( string path )
		{
			throw new NotImplementedException();
		}
	}
}
