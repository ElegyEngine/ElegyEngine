﻿// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy
{
	/// <summary>
	/// Asset loader interface. <see cref="IAssetLoader.CanLoad(string)"/> is called first, to
	/// check for the file extension, then the appropriate loading method is called,
	/// such as <seealso cref="IModelLoader.LoadModel(string)"/>.
	/// </summary>
	public interface IAssetLoader
	{
		/// <summary>
		/// Whether or not this asset loader supports this format.
		/// For example, an OBJ loader may support .obj.
		/// </summary>
		bool CanLoad( string path );
	}
}
