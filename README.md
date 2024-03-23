
# Elegy.Engine

C# and Vulkan game engine, specialised for developing retro-style first-person shooters and similar games from the late 90s and early 2000s.  

Check out [Elegy.TestGame](src/Elegy.TestGame) for an example game module, as well as the other tools and plugins:
* [Elegy.DevConsole](src/Elegy.DevConsole) - external developer console plugin
* [Elegy.MapCompiler](src/Elegy.MapCompiler) - level compiler tool for TrenchBroom and other Quake map editors
* [Elegy.MaterialGenerator](src/Elegy.MaterialGenerator) - quickly generate materials from a folder full of texture images
* [Elegy.ShaderTool](src/Elegy.ShaderTool) - generate shaders & shader templates from GLSL

For level designers who use TrenchBroom, a [game config](config/trenchbroom) is provided. Other level editors are likely unsupported but could work depending on how well they do Quake 3.

*Note: early WiP, check back in Q3 2024!*

# Building

### Prerequisites
* .NET 8 ([link](https://dotnet.microsoft.com/en-us/download))
* Vulkan SDK 1.3.224 or newer. Related to it you'll need:
	* `glslang`
		* Comes with Vulkan SDK
		* Just make sure it's in your `PATH` variable
		* i.e. you can just call `glslang` from the commandline
	* a GPU that supports `VK_KHR_fragment_shader_barycentric` (find out if yours [supports it here](https://vulkan.gpuinfo.org/listdevicescoverage.php?extension=VK_KHR_fragment_shader_barycentric&platform=all))
* PowerShell 7 ([link](https://github.com/PowerShell/PowerShell))

### Building the code
If you're brave enough to touch this code, you can get started with simply running `build/setup_firsttime.ps1`.

Should you do it manually:
* you can build all projects from `src/Elegy.sln`, you'll specifically need:
	* Elegy.Engine
	* Elegy.RenderStandard
	* Elegy.DevConsole
	* Elegy.TestGame
	* Elegy.ShaderTool (Release config)
* compiled modules can than then be copied into `testgame/` via `build/copy_dlls.ps1`.
* shaders can then be built with `build/build_shaders.ps1`

Optional notes:
* if you decide to make NuGet packages, you may find some of the other scripts in `build/` useful, just make sure to adjust the paths

# Licence

[MIT](LICENSE.md) licence. The Elegy.Common module [contains](legal/Godot/README.md) code adapted from Godot Engine.
