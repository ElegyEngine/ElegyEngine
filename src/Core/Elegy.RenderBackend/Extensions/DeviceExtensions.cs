﻿// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;
using Veldrid;

namespace Elegy.RenderBackend.Extensions
{
	public static class DeviceExtensions
	{
		public static DeviceBuffer CreateBufferFromList<T>( this GraphicsDevice device, BufferUsage usage, List<T> list )
			where T: unmanaged
		{
			ReadOnlySpan<T> span = CollectionsMarshal.AsSpan( list );
			var buffer = device.ResourceFactory.CreateBufferForList( usage, list );
			device.UpdateBuffer( buffer, 0, span );
			return buffer;
		}

		public static DeviceBuffer CreateBufferFromStruct<T>( this GraphicsDevice device, BufferUsage usage, in T obj )
			where T: unmanaged
		{
			var buffer = device.ResourceFactory.CreateBufferForStruct<T>( usage );
			device.UpdateBuffer( buffer, 0, obj );
			return buffer;
		}

		public static DeviceBuffer CreateBufferFromSpan<T>( this GraphicsDevice device, BufferUsage usage, ReadOnlySpan<T> span )
			where T: unmanaged
		{
			var buffer = device.ResourceFactory.CreateBufferForSpan( usage, span );
			device.UpdateBuffer( buffer, 0, span );
			return buffer;
		}
	}
}
