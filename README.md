
# Elegy.Engine

The core of a "virtual game engine" that runs on top of Godot 4.0, specialised for developing retro-style first-person shooters and similar games from the late 90s and early 2000s.  
It is launched by [Elegy.Launcher](https://github.com/ElegyEngine/ElegyLauncher) which also provides it access to a GodotSharp instance.

Check out [Elegy.TestGame](https://github.com/ElegyEngine/Elegy.TestGame) for an example game module.

*Note: early WiP, check back in Q4 2023!*

# Building

If you're brave enough to touch this code:
* you can build all projects from `src/Elegy.sln`
* if you decide to make NuGet packages, you may find some of the scripts in `build/` useful, just make sure to adjust the paths
* there is a script that copies DLLs to a game folder, `build/copy_dlls.ps1`, make sure to change the first two paths there too
* when running the scripts, make sure to do it from this directory, such as:
  ```
  ./build/copy_dlls.ps1
  ```

