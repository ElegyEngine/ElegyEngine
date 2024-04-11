// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Framework.Bootstrap
{
	/// <summary> Initialise the asset subsystem. </summary>
	[AttributeUsage( AttributeTargets.Class, AllowMultiple = false )]
	public class ElegyBootstrapAttribute : Attribute { }

	/// <summary> Initialise the asset subsystem. </summary>
	[AttributeUsage( AttributeTargets.Class, AllowMultiple = false )]
	public class WithAssetSystemAttribute : Attribute { }

/// <summary> Initialise the audio subsystem. </summary>
[AttributeUsage( AttributeTargets.Class, AllowMultiple = false )]
	public class WithAudioSystemAttribute : Attribute { }

	/// <summary> Initialise the console subsystem. </summary>
	[AttributeUsage( AttributeTargets.Class, AllowMultiple = false )]
	public class WithConsoleSystemAttribute : Attribute { }

	/// <summary> Initialise the virtual file system. </summary>
	[AttributeUsage( AttributeTargets.Class, AllowMultiple = false )]
	public class WithFileSystemAttribute : Attribute { }

	/// <summary> Initialise the input subsystem. </summary>
	[AttributeUsage( AttributeTargets.Class, AllowMultiple = false )]
	public class WithInputSystemAttribute : Attribute { }

	/// <summary> Initialise the platform subsystem. </summary>
	[AttributeUsage( AttributeTargets.Class, AllowMultiple = false )]
	public class WithPlatformSystemAttribute : Attribute { }

	/// <summary> Initialise the plugin subsystem. </summary>
	[AttributeUsage( AttributeTargets.Class, AllowMultiple = false )]
	public class WithPluginSystemAttribute : Attribute { }

	/// <summary> Initialise the rendering subsystem. </summary>
	[AttributeUsage( AttributeTargets.Class, AllowMultiple = false )]
	public class WithRenderSystemAttribute : Attribute { }
}
