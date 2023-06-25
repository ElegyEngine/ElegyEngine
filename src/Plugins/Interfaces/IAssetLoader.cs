// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy
{
	/// <summary>
	/// Asset loader interface.
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
