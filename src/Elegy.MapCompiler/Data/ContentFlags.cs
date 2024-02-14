// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

// This could maybe get moved to Elegy.Common

namespace Elegy.MapCompiler.Data
{
	/// <summary>
	/// Brush content flags.
	/// </summary>
	[Flags]
	public enum ContentFlag
	{
		None = 0,

		/// <summary>
		/// This brush is an origin brush.
		/// </summary>
		Origin = 1,

		/// <summary>
		/// This surface acts as a runtime occluder.
		/// </summary>
		Occluder = 2,

		/// <summary>
		/// This brush has no collision.
		/// </summary>
		NoCollision = 4
	};
}
