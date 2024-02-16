
# Elegy.Engine

C# and Vulkan game engine, specialised for developing retro-style first-person shooters and similar games from the late 90s and early 2000s.  

Check out [Elegy.TestGame](src/Elegy.TestGame) for an example game module, as well as the other tools and plugins:
* [Elegy.DevConsole](src/Elegy.DevConsole) - external developer console plugin
* [Elegy.MapCompiler](src/Elegy.MapCompiler) - level compiler tool for TrenchBroom and other Quake map editors
* [Elegy.MaterialGenerator](src/Elegy.MaterialGenerator) - quickly generate materials from a folder full of texture images

For level designers who use TrenchBroom, a [game config](config/trenchbroom) is provided. Other level editors are likely unsupported but could work depending on how well they do Quake 3.

*Note: early WiP, check back in Q3 2024!*

# Building

If you're brave enough to touch this code:
* you can build all projects from `src/Elegy.sln`, you'll specifically need:
	* Elegy.Engine
	* Elegy.RenderStandard
	* Elegy.DevConsole
	* Elegy.TestGame
* compiled modules can than then be copied into `testgame/` via `build/copy_dlls.ps1`

Optional notes:
* if you decide to make NuGet packages, you may find some of the scripts in `build/` useful, just make sure to adjust the paths

# Licence

[MIT](LICENSE.md) licence. The Elegy.Common module [contains](legal/Godot/README.md) code adapted from Godot Engine.
