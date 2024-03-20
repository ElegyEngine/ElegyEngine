// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;

namespace Elegy.Common.Extensions
{
	/// <summary>
	/// Elegy-specific <see cref="Span{T}"/> extensions.
	/// </summary>
	public static class SpanExtensions
	{
		/// <summary>
		/// A shortcut to <see cref="MemoryMarshal.GetReference{T}(Span{T})"/>.
		/// </summary>
		public static ref T GetRef<T>( this Span<T> span )
		{
			return ref MemoryMarshal.GetReference( span );
		}
	}
}
