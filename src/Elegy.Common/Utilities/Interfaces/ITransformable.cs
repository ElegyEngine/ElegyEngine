// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Common.Utilities.Interfaces
{
	/// <summary>
	/// A thing containing a transformation matrix.
	/// </summary>
	public interface ITransformable
	{
		/// <summary>
		/// Transformation matrix of the object.
		/// </summary>
		Matrix4x4 Transform { get; set; }
	}
}
