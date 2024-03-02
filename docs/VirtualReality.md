
# VR integration plan

I'm cataloguing my thoughts here about VR integration in the future.

## Goals

* OpenXR integration
	* I prefer this to OpenVR or anything else, as I have the impression it's the most widespread thing a.t.m. A bit similar to my choice for Vulkan: one size fits most!
* Stereo rendering
	* Basically rendering two views, one for each eye. There is a small technical challenge in this.
* Plug'n'play
	* This would be an optional plugin to the game SDK and shouldn't require too many modifications to the engine itself.
* Mobile VR
	* Ideally this should run on standalone headsets like the Quest and Pico.

## Design

There would be an `Elegy.VR` namespace with a few singletons and utilities for VR.
* `Core` singleton
	* XR boilerplate, session management
	* Platform backend implementation (naturally, for mobile VR)
* `Input` singleton
	* `Headset` object
		* Position and orientation
	* `Hand` objects
		* Position and orientation
		* Trigger and grip
		* Thumbstick
* `Locomotion` namespace
	* `ILocomotionHandler` interface
		* Generates player movement vectors from `Headset` and `Hand` input
	* `HeadLocomotion` - smooth loco based on head direction
	* `HandLocomotion` - smooth loco based on hand direction
	* `TeleportLocomotion` - teleporting
* `Render` singleton
	* XR swapchain and view creation
	* Compositing views (`VrView` could have render injection just like typical render views)
	* XR view rendering

## Challenges

For this to be easy to integrate, there will need to be a degree of support in the render backend. It is important to make it easy to have two view matrices, for multiview support.

Since the core loop is handled in the launcher itself, it would make sense to have the `Elegy.VR` module be a dependency of an imaginary `Elegy.LauncherVR`.

```cs
Update( deltaTime )
{
	MyGamePlugin.Update( deltaTime )
	{
		VrInput.Update( deltaTime );
		VrInput.Hands...
	}
}

RenderToVr( xrView, deltaTime )
{
	VrRender.BeginFrame();
	VrRender.RenderView( xrView );
	VrRender.EndFrame();
	VrRender.PresentView( xrView );
}

// Renders at a separate framerate from VR
// TODO: separate in-world camera?
RenderToWindow( xrView, sdlWindow, deltaTime )
{
	VrRender.RenderEyeIntoWindow( sdlWindow, xrView, Eye.Right );
}
```

All in all, it would require the render backend & shader system to dictate a couple parts. The easiest way to go about it is to force having two view matrices in vertex shaders, and when in flatscreen, only using the first one. It is also possible to compile VR versions of shaders.

```glsl
#extension GL_EXT_multiview : enable

layout(std140, binding = 0) uniform RenderView
{
	mat4 projectionMatrix; // do we need two maybe?
	mat4 viewMatrix[2];
};
```

It is possible to have both of these options, and choose between them depending on how important VR is to one's game (VR-only, VR-first, VR-second).

## Future expansion

Hand tracking, full body tracking and face tracking are possible venues of exploration.

Face and FBT would probably need a unified VR skeleton.
