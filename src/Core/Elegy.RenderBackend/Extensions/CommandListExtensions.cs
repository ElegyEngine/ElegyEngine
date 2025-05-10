// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Veldrid;

namespace Elegy.RenderBackend.Extensions;

public static class CommandListExtensions
{
	/// <summary> Updates a GPU buffer optionally with only the first <paramref name="numElementsToUpdate"/> elements.</summary>
	public static void UpdateBufferFromSpan<T>( this CommandList commandList, DeviceBuffer buffer, ReadOnlySpan<T> span, int numElementsToUpdate = -1 )
		where T: unmanaged
	{
		ReadOnlySpan<T> updateSpan = numElementsToUpdate == -1 ? span : span.Slice( 0, numElementsToUpdate );
		commandList.UpdateBuffer( buffer, 0, updateSpan );
	}
}
