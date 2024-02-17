
# Elegy roadmap

Here you can read about the overall plans, long-term and short-term, for Elegy Engine and its plugins.

# Short-term (until mid 2024)

The short-term plan is to get a basic thing up and running by mid 2024, so you can make Half-Life-style game prototypes.

You can read about it [here (milestone for 0.1)](https://github.com/ElegyEngine/ElegyEngine/issues/1).

## Workflow - mapping
You'd use TrenchBroom to make levels, and a custom compiler to compile these levels into an optimised format for Elegy.

## Workflow - modelling
You'd import models by... not importing them. Just place a GLTF somewhere in the models folder, and you're done. Same goes for textures.

## Workflow - materials
To support TrenchBroom (and J.A.C.K.?) easily, materials would be written like Quake 3 shaders:
```
textures/my_texture
{
	materialTemplate Standard
	{
		map textures/images/my_texture.png
	}
}
```

## Workflow - scripting
In terms of "dynamic" scripting, nothing at all at this moment. All game code is compiled into a DLL.

## Workflow - UI
This is a tricky one. UI would be code-only for now, probably ImGui or something just to get started.

## Game SDK
The game SDK will feature a few fundamental things:
- Component system
	- Quake-style trigger-target mechanism
	- Component trait system
- Basic weapon system
- Basic player controller
- Basic AI system
- Basic [BepuPhysics](https://github.com/bepu/bepuphysics2) integration
- Very crude main menu

# Middle-term (mid 2024 - 2025)
Some notable things here include:
- Game SDK will be more complete:
	- Polished player movement
	- [Recast Navigation](https://github.com/recastnavigation/recastnavigation) integration via [DotRecast](https://github.com/ikpil/DotRecast) - basically, proper pathfinding for NPCs
	- Main menu with options
- Custom shader workflow.
- Particle FX.
- Entry-level guides on wiki pages in the repo.

A couple milestones will be hit, 0.2 and potentially 0.3.

## Workflow - scripting
Level scripts might be introduced as an alternative to scripting via map entities, useful for simplifying complicated entity setups and having something like gamemodes/minigames in custom maps.

## Workflow - UI
It will be a lot more iterative, as at this point, you'll be able to reload your GUI scripts/files, they are no longer code-only. RmlUI could be a very nice option for in-game GUIs and main menus.

# Long-term (2025-2026)

Once everything settles, we may begin doing some more serious stuff:
- [LoudPizza](https://github.com/TechPizzaDev/LoudPizza) backend with custom geometric accoustics tech, or alternatively Steam Audio/Phonon.
- Networking/multiplayer.
- GUI project wizard to easily create Elegy projects and download relevant code templates for game modules, plugin modules etc.
- Vehicle system in the game SDK?
- Documentation website with guides and an API reference.

Half of these entries, like the new physics engine, are milestones by themselves, so once this is finished, you may expect Elegy to reach version 0.5 or something.

It's very hard to be certain about any of this, therefore treat the middle-and-long-term sections as "this would be the best-case scenario, but it most likely won't happen". It represents my vision for the future development of this project, but definitely not how it'll play out.
