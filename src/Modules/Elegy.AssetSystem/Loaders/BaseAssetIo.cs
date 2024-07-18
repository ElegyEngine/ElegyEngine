// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.Interfaces;

namespace Elegy.AssetSystem.Loaders
{
	/// <summary>
	/// Base model loader, makes implementing the <see cref="IAssetIo"/> interface quicker.
	/// </summary>
	public abstract class BaseAssetIo : IAssetIo
	{
		/// <inheritdoc/>
		public abstract string Name { get; }
		/// <inheritdoc/>
		public string Error => string.Empty;
		/// <inheritdoc/>
		public bool Initialised => true;

		/// <inheritdoc/>
		public virtual bool Init() => true;

		/// <inheritdoc/>
		public virtual void Shutdown() { }

		/// <inheritdoc/>
		public abstract bool Supports( string path );
	}
}
