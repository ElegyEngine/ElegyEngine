// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;
using Veldrid;

namespace Elegy.RenderBackend.Extensions
{
	public static class DeviceExtensions
	{
		/// <summary> Creates a GPU buffer from the provided <paramref name="obj"/>.</summary>
		public static DeviceBuffer CreateBufferFromStruct<T>( this GraphicsDevice device, BufferUsage usage, in T obj )
			where T: unmanaged
		{
			DeviceBuffer buffer = device.ResourceFactory.CreateBufferForStruct<T>( usage );
			device.UpdateBuffer( buffer, 0, obj );
			return buffer;
		}

		/// <summary> Creates a GPU buffer from a span, optionally with only the first <paramref name="numElementsToUpdate"/> elements.</summary>
		public static DeviceBuffer CreateBufferFromSpan<T>( this GraphicsDevice device, BufferUsage usage, ReadOnlySpan<T> span, int numElementsToUpdate = -1 )
			where T: unmanaged
		{
			DeviceBuffer buffer = device.ResourceFactory.CreateBufferForSpan( usage, span );
			device.UpdateBufferFromSpan( buffer, span, numElementsToUpdate );
			return buffer;
		}

		/// <summary> Updates a GPU buffer optionally with only the first <paramref name="numElementsToUpdate"/> elements.</summary>
		public static void UpdateBufferFromSpan<T>( this GraphicsDevice device, DeviceBuffer buffer, ReadOnlySpan<T> span, int numElementsToUpdate = -1 )
			where T: unmanaged
		{
			ReadOnlySpan<T> updateSpan = numElementsToUpdate == -1 ? span : span.Slice( 0, numElementsToUpdate );
			device.UpdateBuffer( buffer, 0, updateSpan );
		}
	}
}
