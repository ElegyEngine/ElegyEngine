// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Interfaces.Services;

namespace Elegy.Common.Utilities;

/// <summary>
/// This is a service locator used for a couple of fundamental modules (logging, virtual filesystem etc.)
/// so they don't have to depend on each other. It's what allows the modules to be used as independent modules.
/// </summary>
public static class ElegyInterfaceLocator
{
	/// <summary>
	/// Common services with some builtin defaults.
	/// </summary>
	public static Dictionary<Type, object> Services { get; } = new()
	{
		{ typeof(ILogSystem), new DefaultLogSystem() },
		{ typeof(IFileSystem), new DefaultFileSystem() }
	};

	/// <summary>
	/// Sets an interface. Make sure to explicitly set the generic type param.
	/// </summary>
	public static void Set<T>( T item ) where T: notnull
	{
		Services[typeof(T)] = item;
	}

	/// <summary>
	/// Specialised getter for <see cref="ILogSystem"/>.
	/// Slightly faster than <see cref="Get{T}"/>.
	/// </summary>
	public static ILogSystem GetLogSystem()
		=> (ILogSystem)Services[typeof(ILogSystem)];

	/// <summary>
	/// Specialised getter for <see cref="IFileSystem"/>.
	/// Slightly faster than <see cref="Get{T}"/>.
	/// </summary>
	public static IFileSystem GetFileSystem()
		=> (IFileSystem)Services[typeof(IFileSystem)];

	/// <summary>
	/// Gets the interface, or null if it doesn't exist.
	/// </summary>
	public static T Get<T>()
	{
		if ( Services.TryGetValue( typeof( T ), out var result ) )
		{
			return (T)result;
		}

		throw new KeyNotFoundException();
	}
}
