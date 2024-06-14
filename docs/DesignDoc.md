
This is a rough sketch of the final engine. Treat this as some type of "this is how a 1.0 will look".

## Overall architecture

- Elegy.Common - utility library
- Elegy.Framework - engine core, handles engine configuration etc.
- Engine modules
- Elegy.GameSDK - the game SDK
- Elegy.RenderBackend - Veldrid/Vulkan utilities

## Engine modules

- Elegy.AssetSystem
	- Quake 3-style materials
	- Data-driven shaders
	- Model asset loading (plugin-based)
	- Texture asset loading (plugin-based)
- Elegy.AudioSystem (plugin-based)
	- Sound sources, listener
	- Sound FX
	- Geometric acoustics
- Elegy.ConsoleSystem
	- Logging, console frontends, CVars, console commands
- Elegy.FileSystem
	- Mounting game/mod paths
	- Mounting addon paths
- Elegy.Input
	- Keyboard, mouse input
	- Gamepad & joysticks
- Elegy.NetworkSystem
	- Utility layer on top of ENet or RiptideNetworking
- Elegy.PlatformSystem
	- Windowing (injected)
	- Time
	- Fundamental engine configuration, e.g. headless mode
- Elegy.PluginSystem
	- Full-blown dynamic plugin system
- Elegy.RenderSystem
	- Renderable objects (entities, batches, volumes, lights...)
	- Views & rendering into windows
	- Debug rendering
	- Render styles (plugin-based)
- Elegy.RenderWorld
	- Occlusion culling (injected)
	- Render loop
	- High-level graphics API for games

Legend:
- plugin-based: implemented in a plugin, engine just provides API
- injected: implemented externally (e.g. in a launcher), engine just sees API

## Elegy.GameSDK subsystems

- AI
	- Goal-oriented action planning
	- Use Recast/DotRecast
- Animation
	- Animation playback and management
	- Animation blending
	- Animation channels
	- Inverse kinematics
- Client
	- Client controllers (handle input and interaction with the game world)
	- Keybind system
	- View bobbing and viewport management
- Entity system
	- Component system with “traits” and Quake-style triggering, later Source-style I/O
	- Scripting
- Game sessions
	- Menu state, loading state, playing state, paused state etc.
	- Linking a client into the game
- Gamemodes
	- Campaign, deathmatch, team deathmatch, co-op etc.
- Netcode
	- Mainly intended for LAN co-op
	- Quake-style client-server with prediction and rollback
	- Singleplayer bridge
- Particles
- Physics
	- Rigid bodies
	- Constraints
- Save-load system
- UI
	- ImGui for quick'n'easy stuff
	- Custom game UI system or Squid (*todo: R&D this*)

Optional modules:
- Vehicle system
- Vegetation system
- Virtual reality through OpenXR

## Workflow and features
### Overall:
- Quake-style workflow.
- There’s an engine executable in the root folder, accompanied by game folders and a base folder.
- The base folder contains base engine assets, e.g. a “missing model” model, a “missing texture” texture and whatnot.
- Each game/mod folder contains a configuration file that describes it as well as its plugins and dependencies.

### Mapping:
- You’d use an external level editor like TrenchBroom, J.A.C.K. or NetRadiant-custom.
- You’d also use a map compiler, either as part of your level editor’s compile config, or through a console command.

### Modelling:
- You’d use a model editor like Blender.
- Export GLTF for basic props, import via custom tool for extra attributes for complex models.

### Texturing, sound design and other trivial asset types:
- Quite simply place your PNGs/WAVs/FLACs into a textures/sounds folder, it’s just there.

### UI:
- Declarative C# UI files sitting in a directory.

### Scripting:
- Naked C# scripts, S&box-style.
- Level scripts
- UI scripts
- NPC scripts?
- Weapon scripts?
- Vehicle scripts?
