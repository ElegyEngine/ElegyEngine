// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy.Reflection
{
	/// <summary>
	/// Wrapper for reflected C# members.
	/// </summary>
	public abstract class MemberWrapper
	{
		/// <summary>
		/// Attributes of this property or method.
		/// </summary>
		public List<Attribute> Attributes { get; private set; }

		internal MemberWrapper( List<Attribute> attributes )
		{
			Attributes = attributes;
		}

		/// <summary>
		/// Returns whether or not this property has an instace of <typeparamref name="TAttribute"/>.
		/// </summary>
		public bool HasAttribute<TAttribute>() where TAttribute : Attribute
		{
			for ( int i = 0; i < Attributes.Count; i++ )
			{
				if ( Attributes[i] is TAttribute )
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Gets an attribute if it's applied to this property.
		/// </summary>
		public TAttribute? GetAttribute<TAttribute>() where TAttribute : Attribute
		{
			for ( int i = 0; i < Attributes.Count; i++ )
			{
				if ( Attributes[i] is TAttribute )
				{
					return Attributes[i] as TAttribute;
				}
			}

			return null;
		}
	}
}
