// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Engine.Interfaces;

namespace Elegy.Engine.AssetLoaders
{
	/// <summary>
	/// Base model loader, makes implementing the <see cref="IPlugin"/> interface quicker.
	/// </summary>
	public abstract class BaseAssetLoader : IAssetLoader
	{
		/// <inheritdoc/>
		public abstract string Name { get; }
		/// <inheritdoc/>
		public string Error => string.Empty;
		/// <inheritdoc/>
		public bool Initialised => true;

		/// <inheritdoc/>
		public bool Init() => true;

		/// <inheritdoc/>
		public void Shutdown() { }

		/// <inheritdoc/>
		public abstract bool CanLoad( string path );
	}
}
