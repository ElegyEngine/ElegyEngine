// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

using System.Reflection;

namespace Elegy.Reflection
{
	/// <summary>
	/// Utility to easily interact with C# methods.
	/// </summary>
	public class MethodWrapper : MemberWrapper
	{
		/// <summary>
		/// More detailed info about the method.
		/// </summary>
		public MethodInfo Info { get; private set; }

		/// <summary>
		/// The return type of this method.
		/// </summary>
		public Type ReturnType { get; private set; }

		/// <summary>
		/// Constructs a <see cref="MethodWrapper"/> from the given <paramref name="info"/>,
		/// caching its metadata.
		/// </summary>
		public MethodWrapper( MethodInfo info )
			: base( new( info.GetCustomAttributes() ) )
		{
			Info = info;
			ReturnType = info.ReturnType;
		}

		/// <summary>
		/// Invokes this <paramref name="obj"/>'s method, with a return value but no arguments.
		/// </summary>
		public TReturn? Invoke<TReturn>( object obj )
		{
			if ( ReturnType != typeof( TReturn ) )
			{
				return default;
			}

			return (TReturn?)Info.Invoke( obj, null );
		}

		/// <summary>
		/// Invokes this <paramref name="obj"/>'s method, with a return value and one or more arguments.
		/// </summary>
		public TReturn? Invoke<TReturn>( object obj, params object[] objects )
		{
			if ( ReturnType != typeof( TReturn ) )
			{
				return default;
			}

			return (TReturn?)Info.Invoke( obj, objects );
		}

		/// <summary>
		/// Invokes this <paramref name="obj"/>'s method, with no return value and no arguments.
		/// </summary>
		public void Invoke( object obj )
		{
			Info.Invoke( obj, null );
		}

		/// <summary>
		/// Invokes this <paramref name="obj"/>'s method, with no return value.
		/// </summary>
		public void Invoke( object obj, params object[] objects )
		{
			Info.Invoke( obj, objects );
		}
	}
}
