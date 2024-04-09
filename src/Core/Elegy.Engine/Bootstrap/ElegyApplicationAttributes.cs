// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Engine.Bootstrap
{
	/// <summary> Marks the entry point for Elegy's application framework. </summary>
	[AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
	public class ElegyMainAttribute : Attribute { }

	/// <summary> Initialise the asset subsystem. </summary>
	[AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
	public class WithAssetSystemAttribute : Attribute { }

	/// <summary> Initialise the audio subsystem. </summary>
	[AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
	public class WithAudioSystemAttribute : Attribute { }

	/// <summary> Initialise the console subsystem. </summary>
	[AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
	public class WithConsoleSystemAttribute : Attribute { }

	/// <summary> Initialise the virtual file system. </summary>
	[AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
	public class WithFileSystemAttribute : Attribute { }

	/// <summary> Initialise the input subsystem. </summary>
	[AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
	public class WithInputSystemAttribute : Attribute { }

	/// <summary> Initialise the plugin subsystem. </summary>
	[AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
	public class WithPluginSystemAttribute : Attribute { }

	/// <summary> Initialise the rendering subsystem. </summary>
	[AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
	public class WithRenderSystemAttribute : Attribute { }

	/// <summary> A shortcut to initialise all needed systems for games. </summary>
	[AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
	public class WithAllGameSystemsAttribute : Attribute { }

	/// <summary> A shortcut to initialise all needed systems for basic tools. </summary>
	[AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
	public class WithMinimalToolSystemsAttribute : Attribute { }
}
