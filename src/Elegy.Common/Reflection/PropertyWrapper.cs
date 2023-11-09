// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

using System.Reflection;

namespace Elegy.Reflection
{
	/// <summary>
	/// Utility to easily interact with C# properties.
	/// </summary>
	public class PropertyWrapper : MemberWrapper
	{
		/// <summary>
		/// More detailed info about the property.
		/// </summary>
		public PropertyInfo Info { get; private set; }

		/// <summary>
		/// The type of the value behind the property.
		/// </summary>
		public Type Type { get; private set; }

		/// <summary>
		/// Constructs a <see cref="PropertyWrapper"/> from the given <paramref name="info"/>,
		/// caching its metadata.
		/// </summary>
		public PropertyWrapper( PropertyInfo info )
			: base( new( info.GetCustomAttributes() ) )
		{
			Info = info;
			Type = info.PropertyType;
		}

		/// <summary>
		/// Sets the value of this property in the current object <paramref name="obj"/>.
		/// </summary>
		public void Set<T>( object obj, T value )
		{
			if ( Type != typeof( T ) )
			{
				return;
			}

			Info.SetValue( obj, value );
		}

		/// <summary>
		/// Gets the value of this property in the current object <paramref name="obj"/>.
		/// </summary>
		public T? Get<T>( object obj )
		{
			if ( Type != typeof( T ) )
			{
				return default;
			}

			return (T?)Info.GetValue( obj );
		}
	}
}
