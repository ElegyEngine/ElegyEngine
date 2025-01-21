
# Elegy.Engine

*Note: early WiP, not production-ready. You can track progress in ["Roadmap to 1.0"](https://github.com/ElegyEngine/ElegyEngine/issues/18).*  

This is a C# and Vulkan game engine, specialising in the development of retro-style first-person shooters and similar games from the late 90s and early 2000s.  

Check out [Elegy.Game](src/Plugins/Elegy.Game) for an example game module, as well as the other tools and plugins:
* [Elegy.DevConsole](src/Plugins/Elegy.DevConsole) - external developer console plugin
* [Elegy.MapCompiler](src/Tools/Elegy.MapCompiler) - level compiler tool for TrenchBroom and other Quake map editors
* [Elegy.MaterialGenerator](src/Tools/Elegy.MaterialGenerator) - quickly generate materials from a folder full of texture images
* [Elegy.ShaderTool](src/Tools/Elegy.ShaderTool) - generate shaders & shader templates from GLSL

For level designers who use TrenchBroom, a [game config](config/trenchbroom) is provided. Other level editors are likely unsupported but could work depending on how well they do Quake 3.

# Building

### Prerequisites
* .NET 8 ([link](https://dotnet.microsoft.com/en-us/download))
* Vulkan SDK 1.3.224 or newer. Related to it you'll need:
	* `glslang`
		* Comes with Vulkan SDK
		* Just make sure it's in your `PATH` variable
		* i.e. you can just call `glslang` from the commandline
	* a GPU that supports `VK_KHR_fragment_shader_barycentric` (find out if yours [supports it here](https://vulkan.gpuinfo.org/listdevicescoverage.php?extension=VK_KHR_fragment_shader_barycentric&platform=all)) - this may not be a restriction [later on](https://github.com/ElegyEngine/ElegyEngine/issues/10)
* PowerShell 7 ([link](https://github.com/PowerShell/PowerShell))

### Building the code
If you're brave enough to touch this code, you can get started with simply running `build/setup_firsttime.ps1`.  
It will create all the projects, compile the shaders and copy everything needed into `testgame/`.

**For debugging** it's a good idea to create a debug profile for `Elegy.Launcher2` whose working directory points to `testgame/`. Also every time you build one of the projects in `Plugins/` you will need to copy them using `build/copy_dlls.ps1`.

Other notes:
* if you decide to make NuGet packages, you may find some of the other scripts in `build/` useful, just make sure to adjust the paths
* building & testing your game/plugin code will be more streamlined in the future

# Licence

[MIT](LICENSE.md) licence. The Elegy.Common module [contains](legal/Godot/README.md) code adapted from Godot Engine.
