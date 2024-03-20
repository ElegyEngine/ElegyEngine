// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Common.Utilities.Interfaces
{
	/// <summary>
	/// A thing containing an ID.
	/// </summary>
	public interface IIdentifiable
	{
		/// <summary>
		/// ID of the object.
		/// </summary>
		int Id { get; init; }
	}
}
