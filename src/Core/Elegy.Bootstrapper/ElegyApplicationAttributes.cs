// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System;

namespace Elegy.Bootstrapper
{
	[AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
	public class ElegyMainAttribute : Attribute
	{

	}

	[AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
	public class WithAssetSystemAttribute : Attribute
	{

	}

	[AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
	public class WithConsoleSystemAttribute : Attribute
	{

	}

	[AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
	public class WithPluginSystemAttribute : Attribute
	{

	}
}
