// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Common.Interfaces
{
	/// <summary>
	/// This type can be serialised into an array of bytes, and then
	/// subconsequently used in IByteBuffer.Write/ReadObject.
	/// </summary>
	public interface IByteSerialisable
	{
		/// <summary></summary>
		public void Serialise( IByteBuffer buffer );

		/// <summary></summary>
		public void Deserialise( IByteBuffer buffer );
	}
}
