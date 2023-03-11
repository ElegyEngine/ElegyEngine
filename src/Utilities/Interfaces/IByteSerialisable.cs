// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy.Utilities.Interfaces
{
	/// <summary>
	/// This type can be serialised into an array of bytes, and then
	/// subconsequently used in IByteBuffer.Write/ReadObject.
	/// </summary>
	public interface IByteSerialisable
	{
		public void Serialise( IByteBuffer buffer );
	
		public void Deserialise( IByteBuffer buffer );
	}
}
