// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

// NOTE: This is a draft! It hasn't been tested but I'm
// putting this here so I don't have to do it later

using System.Reflection;

namespace Elegy.Reflection
{
	/// <summary>
	/// Caches a bunch of metadata about a given datatype.
	/// </summary>
	public class TypeInfo
	{
		private Type mType;

		/// <summary>
		/// The attributes this type has.
		/// </summary>
		public IReadOnlyList<Attribute> Attributes { get; }

		/// <summary>
		/// This type's properties.
		/// </summary>
		public IReadOnlyList<PropertyWrapper> Properties => mProperties;
		private List<PropertyWrapper> mProperties = new();

		/// <summary>
		/// This type's methods.
		/// </summary>
		public IReadOnlyList<MethodWrapper> Methods => mMethods;
		private List<MethodWrapper> mMethods = new();

		/// <summary>
		/// Gathers all type information from <typeparamref name="T"/>.
		/// </summary>
		public static TypeInfo From<T>()
		{
			return new TypeInfo( typeof( T ) );
		}

		/// <summary>
		/// Gathers all type information from the given type.
		/// </summary>
		public TypeInfo( Type type )
		{
			mType = type;
			Attributes = new List<Attribute>( type.GetCustomAttributes() );

			var methods = mType.GetMethods();
			for ( int i = 0; i < methods.Length; i++ )
			{
				mMethods.Add( new( methods[i] ) );
			}

			var properties = mType.GetProperties();
			for ( int i = 0; i < properties.Length; i++ )
			{
				mProperties.Add( new( properties[i] ) );
			}
		}
	}
}
