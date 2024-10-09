---
slug: world-renderer
title: World rendering
authors: [admer456]
tags: [idea]
---

# World rendering

Elegy's designed to be pretty modular in some regard. If you don't need game-specific world rendering, you can still use the render backend. You can still use the shader templating system and all of that. That, at least, is the idea.


So I've decided to write a bit about rendering architecture here again and what I wanna have by the end of the year. Simply put, here are the layers:
* `RenderBackend` - Vulkan stuff, shader templating business
* `RenderSystem` - defines renderable object types (meshes, materials, lights, views etc.), provides facilities to actually use shader templates and generate resource sets for them and such, TL;DR handles "mid-level" rendering stuff <!-- truncate -->
* `RenderStyle` - defines how the renderable objects above are shaded, provides shader binaries and material templates
* `RenderWorld` - uses all of the above to actually render a scene, handles occlusion culling, provides an API for the game to interact with renderable objects
* `Game` - creates renderable objects, updates their transforms, and updates material parameters

So, from the game's perspective, there will be a few components like `MeshInstance` and `Sprite` and whatnot. These will be very thin wrappers around the objects originating from `RenderSystem`.

When `RenderWorld` wants to draw something, it'll ask `RenderStyle` to do it. `RenderWorld` ultimately implements the render loop which handles sorting, occlusion culling and other kinds of rendering strategy stuff.

While this is starting to look like lasagna, it does give me full flexibility here and basically what I need:
* The level compiler can just use `RenderBackend` for GPU baking
* The game can use the whole stack (duh)
* A potential level editor or model importer could use half of the stack, implementing their own specialised `WorldRenderer`s

`RenderWorld` can also provide you with places to bolt on your own rendering code, e.g. for ImGui or other things.
