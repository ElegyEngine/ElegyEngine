// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Interfaces;

namespace Game.Server.Packets
{
	/// <summary>
	/// See <see cref="ServerPacketType.Disconnect"/>.
	/// </summary>
	public struct DisconnectPacket : IByteSerialisable
	{
		public string Reason { get; set; }

		public void Deserialise( IByteBuffer buffer )
		{
			Reason = buffer.ReadStringUtf8( StringLength.Short );
		}

		public void Serialise( IByteBuffer buffer )
		{

			buffer.WriteStringUtf8( Reason, StringLength.Medium );
		}
	}
}
