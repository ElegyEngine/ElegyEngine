
# Elegy roadmap

Here you can read about the overall plans, long-term and short-term, for Elegy Engine and its plugins.

# Short-term (until late 2023)

The short-term plan is to get a basic thing up and running by mid 2024, so you can make Half-Life-style game prototypes.

You can read about it [here (milestone for 0.1)](https://github.com/ElegyEngine/ElegyEngine/issues/1).

## Workflow - mapping
You'd use TrenchBroom to make levels, a custom compiler to compile these levels into an optimised format for Elegy and Godot's occlusion culling system.

## Workflow - modelling
You'd import models by... not importing them. Just place a GLTF somewhere in the models folder, and you're done. Same goes for textures.

## Workflow - materials
To support TrenchBroom easily, materials would be written like Quake 3 shaders:
```
textures/my_texture
{
	{
		map "textures/my_texture.png"
		alphaTest 0.5
	}
}
```

## Workflow - scripting
In terms of "dynamic" scripting, nothing at all at this moment. All game code is compiled into a DLL.

## Workflow - UI
This is a tricky one. UI would be code-only for now. No JSON or anything, just C#.

## Game SDK
The game SDK will feature a few fundamental things:
- Half-Life-style entity system with triggering
- Basic weapon system
- Basic AI system
- Main menu implementation with an options menu
- Save-load system

## Synchronising with Godot
Currently, Elegy is on Godot 4.0, but it is expected to migrate to 4.1 or 4.2 over time.

# Middle-term (late 2023 - mid 2024)

Some notable things here include:
- [ImGui](https://github.com/pkdawson/imgui-godot) and/or [RmlUI](https://github.com/mikke89/RmlUi) as alternatives to Godot UI.
- Game SDK will no longer rely on Godot nodes, but rather, physics nodes and renderable nodes will be separated.
- Game SDK will also be more complete, featuring entity components and a vegetation system.
- Entry-level guides on wiki pages in the repo.

A couple milestones will be hit, 0.2 and potentially 0.3.

## Workflow - scripting
Level scripts might be introduced as an alternative to scripting via map entities, useful for simplifying complicated entity setups and having something like gamemodes/minigames in custom maps.

## Workflow - UI
It will be a lot more iterative, as at this point, you'll be able to reload your GUI scripts/files, they are no longer code-only. RmlUI could be a very nice option for in-game GUIs and main menus.

# Long-term (2024-2026)

Once everything settles, we may begin doing some more serious stuff:
- Documentation website with guides and an API reference.
- [BepuPhysics](https://github.com/bepu/bepuphysics2) integration for performance & stability.
- [MiniAudio](https://github.com/mackron/miniaudio) backend with custom geometric accoustics tech, or alternatively Steam Audio/Phonon.
- GUI project wizard to easily create Elegy projects and download relevant code templates for game modules, plugin modules etc.
- Vehicle system in the game SDK?
- Elegy's API will provide more abstractions over Godot, explained why in "A little problem" below.

Half of these entries, like the new physics engine, are milestones by themselves, so once this is finished, you may expect Elegy to reach version 0.6.

It's very hard to be certain about any of this, therefore treat the middle-and-long-term sections as "this would be the best-case scenario, but it most likely won't happen". It represents my vision for the future development of this project, but definitely not how it'll play out.

## A little problem
At some point, Godot won't be an integral part of Elegy.

There are some issues with using Godot the way Elegy is doing it. Namely, some friction here and there:
- The launcher executable is 100+ MB. This makes it pretty uncomfy to download, at least on my slow Internet connection.
- `ShaderMaterial` is rather limited, as there's no mechanism to load precompiled shader binaries, or save them for that matter. Elegy materials with custom shaders would have to get compiled every time upon startup, and this could affect loading times really badly. This is why 0.1 and 0.2 won't even have custom shaders.
- Godot nodes override certain properties after `_Ready` is executed, so in Elegy we need to use `SetDeferred` and such. It's a little bit uncomfy, but thankfully there isn't a lot of it. It's mostly noticeable on some container UI nodes and omni lights when changing their blurriness through `LightSize`.

As time goes on, I imagine Godot's filesize will keep growing slightly. Right now, the launcher is 138 MB, and it may go up to 200 MB in a few years. One possible route is to have a fork of Godot with certain subsystems removed simply for the purpose of being more lightweight.

Initially, Elegy was intended to have lightmapping without modern dynamic lights. It's meant to be a specialised "1999 FPS engine" after all. I'm uncertain how to prebake lightmaps externally in Godot, so we're just relying on dynamic lights and SDFGI for now.

## Solving the problem

There are a couple ways to go about this all. One is maintaining a fork of Godot that doesn't bundle .NET with the exported project and strips off unneeded things where applicable. The other is writing a backend from scratch, removing Godot in the process.

It would be a rather gradual set of changes. Elegy already has a bit of an engine-like infrastructure, with its own console system, independent of Godot's; and its own virtual filesystem.

What could happen is a lot of the Godot stuff getting abstracted away. Elegy's current API doesn't have *anything* regarding input, but it very well could, to make the transition easier in the end.

Similarly, instead of using `MeshInstance` nodes in the game code, you'd create "render entities" via the (imaginary) `RenderSystem`.
```cs
RenderEntity re = RenderSystem.CreateEntity( renderEntityDesc );
...
re.UpdateTransform( transformationMatrix );
```
Having something like this would mean half of the work's been done upfront.

This is certainly subject to change, but here's how it might play out, partially in order:
- Avoid Godot audio with MiniAudio
- Avoid Godot physics with BepuPhysics
- `Input` abstraction
- Avoid Godot UI with a combination of RmlUI and ImGui

At that point, the only Godot subsystem used will be the renderer (and the core Godot types like `AABB`, `Plane` etc.). Having everything else in place, it will be a bit easier to write the renderer than going completely from scratch. Nonetheless, it's a very large undertaking, and that's why it probably won't happen until much later on.
