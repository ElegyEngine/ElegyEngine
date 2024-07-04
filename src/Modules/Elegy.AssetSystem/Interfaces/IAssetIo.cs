// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Interfaces;

namespace Elegy.AssetSystem.Interfaces
{
	/// <summary>
	/// Asset IO interface. <see cref="IAssetIo.Supports(string)"/> is called first, to
	/// check for the file extension, then the appropriate loading method is called,
	/// such as <seealso cref="IModelLoader.LoadModel(string)"/>.
	/// </summary>
	public interface IAssetIo : IPlugin
	{
		/// <summary>
		/// Whether or not this asset loader/writer supports this format.
		/// For example, an OBJ loader may support .obj.
		/// </summary>
		bool Supports( string extension );
	}
}
