
# Material templates and Polyshaders
*and other pipeline shenanigans...*

In this idea I'll talk about:
* Material templates *(abbr: MT)*
* "Polyshaders" *(abbr: none)*

Problem: modern rendering APIs (DirectX 12 and Vulkan) have something called pipeline state objects *(abbr. PSO)*, which I'll call "pipelines" from now on.

The idea is that various things you could dynamically configure in OpenGL (face culling mode, fill mode, blending mode, depth testing, the shader program etc.) are known in advance. In the world of dynamic shader generation from material node setups, this doesn't exactly fit so well:

https://twitter.com/actualGraphite/status/1553590351218212865

Yet at the same time, we want to be able to control these nicely + have custom shaders authored by modders.

Now, here's what I think: **I don't want to wrestle.** I don't want to have framerate drops at runtime because of dynamic shader compilation, either.

Because Elegy is meant to look like it's from 1999, its set of render pipelines shouldn't be too complex. I'd like to have a maximum of 50 pipelines or so total, and even that is probably a bit too much.

How do we do that though?

# Goals

Well first, we need to know what we want:
* All shaders must be precompiled!
	* No runtime anything. It bloats the filesize of the engine and game, and it makes for massive potential runtime costs. Big no.
	* Shader permutations are okay **in moderation.**
* Simple runtime code.
	* Simple runtime = good performance, also easy to maintain.
* A fixed number of pipelines per common material properties, e.g. 4.
	* If 500 materials are twosided, yeah, that's probably a pipeline or so.
* A degree of customisability for modders.
	* Your mod may have custom post-processing shaders, or perhaps introduce a special terrain shader. I'd like to support this.

# Initial thought

First of all, let's start from the pipeline itself. What does a pipeline have? Well, lots of things actually:
* Face culling mode (part of its raster state description)
* Blending mode
* Depth testing (part of the depth stencil state description)
* Stencil testing
* Shaders
* Resource layout (textures, buffers etc.)
* Vertex layout

So, what does this mean for the material system? Right off the bat, we **can't** control things like face culling (twosided-ness) or depth testing & comparison per material, without some wicked sorting/precaching algorithm. Quake 3 and idTech 4 style materials are straight-up not possible. I mean yes, it's possible if we do some sort of pipeline caching... but why? That's just troublesome.

Now, alright. Let's say we've made a little layer above it, some type of **material template**, let's call it. You can have `Standard` and say `StandardTwoSided`. Question becomes: why do you need two-sided materials? Why do we need a whole another material template which is just a copy-paste of the former?

Is it for vegetation? If yes, you're in for an absolute treat: write a `Vegetation` material template! Is it *also* for very thin alphatested surfaces...? Hmm. Think about it. Eventually you'll realise you want a `StandardTwoSidedAlphatested`. And I think that's kinda convenient, apart from having to type the whole thing.

Then you could end up with a `Water` template too. In the end you'd have these material templates:
* `DecalTransparent` - decals such as stains
* `DecalAlphatested` - stamps that are alphatested
* `ParticleAdditive` - additive particles like fire, can be used for generic sprites too though
* `ParticleTransparent` - alpha-faded particles like smoke 
* `Standard` - standard opaque rendering, generic surfaces
* `StandardTwoSidedAlphatested` - standard alphatested two-sided rendering, grates, fences etc.
* `StandardTransparent` - glass and such
* `Water` - water, lava etc.
* `Vegetation` - special vegetation pipeline
* `UI` - 2D user interface stuff, probably the simplest one here

Okay, we have all these material templates now. We haven't really thought about the way they're going to be lit. `Standard` could in theory be used on any surface: an animated model, a lightmapped surface... a lightmapped surface? Oh no, do we need a `StandardLightmapped` now?!

**No.** Allow me to explain below.

# Approaching the solution

So far we've been maybe thinking "1 MT = 1 pipeline". But let's think of it in some other way. What if `Standard` could have 1 PSO for models, 1 PSO for lightmapped surfaces, and for the fun of it, let's say 1 PSO for wireframe mode & one for a depth pass?

This also means each of these will have their own special shader.

That's amazing actually. Now `Standard`, `StandardTransparent`, `StandardTwoSidedAlphatested` and `Water` can be lightmapped!  
But wait... what about `UI`? Why does `UI` need a lightmap pass? Hmm...

Different material templates **sometimes** need different sets of pipelines. So we'll need to come up with some sort of material template... templates? Let's call them *Shader Templates*. They will define, at base, the vertex layouts, and shaders needed.

So the structure goes as follows:
* Shader Templates: vertex layouts, shader sets, resource layouts
* Material Templates: pipeline layouts, resource mapping
* Materials: providing resources

Shader templates:  
* `DecalST` may have `general` and `wireframe`. (2 pipelines)
* `ParticleST` may have `cpu`, `gpu`, `depth` and `wireframe`. (4 pipelines)
* `StandardST` may have `animated`, `lightmapped`, `wireframe`, and `depth`. (4 pipelines)
* `VegetationST` may have `general`, `depth` and `wireframe`. (3 pipelines)
* `WaterST` may have `general` and `wireframe`. (2 pipelines)
* `UIST` may have `general` and `wireframe`. (2 pipelines)

So now we have a hierarchy:
* `DecalST` (2 PSOs)
	* `DecalAlphatested`
	* `DecalTransparent`
* `ParticleST` (4 PSOs)
	* `ParticleAdditive`
	* `ParticleTransparent`
* `StandardST` (4 PSOs)
	* `Standard`
	* `StandardTwoSidedAlphatested`
	* `StandardTransparent`
* `VegetationST` (3 PSOs)
	* `Vegetation`
* `WaterST` (2 PSOs)
	* `Water`
* `UIST` (2 PSOs)
	* `UI`

So, in total, we're gonna have about 30 pipelines. 2 for `DecalAlphatested`, 2 for `DecalTransparent`, 4 for `ParticleAdditive` etc.

Now, here's the thing. We have `Vegetation`, `Water` and `UI` as single instances of their shader templates. This system feels a bit overkill, why take this crazy templating route? *And what about custom shaders?*

# Polyshaders

Well, allow me to introduce you to the last piece of the puzzle, which I also think is the beauty of this system.

For the sake of simplicity, let's just focus on `StandardST` and its 4 pipelines. It having 4 pipelines really just means it having 4 different shaders, the raster state and depth-stencil state are generally the same.

So here are its shaders: (actually the wireframe one is not included here)
* `standard_base.glsl`
```glsl
uniform PerFrameBuffer
{
	mat4 ProjectionMatrix;
	mat4 ViewMatrix;
};

uniform PerEntityBuffer
{
	mat4 EntityMatrix;
	// We'll omit skeletal animation here for simplicity's sake
}

in vec3 vPosition;
in vec2 vUv;
#if SHADERTYPE != DEPTH
in vec3 vNormal;
in vec4 vColour;
#endif
#if SHADERTYPE == LIGHTMAPPED
in vec2 vUvLightmapped;
#endif

// Input for the polyshader
out vec3 POSITION; // in local position
out vec3 POSITION_WORLD; // in-out world position
out vec3 NORMAL; // in local normal
out vec3 NORMAL_WORLD; // in-out world normal
out vec2 UV; // in-out UV
out vec4 COLOUR; // in-out vertex colour

#if SHADERTYPE == LIGHTMAPPED
uniform texture2D LightmapTexture;
out vec2 pLightmappedUv;
#endif

void main_vs()
{
	POSITION = vPosition;
	POSITION_WORLD = (EntityMatrix * vec4( POSITION, 1.0 )).xyz;
	NORMAL = vNormal;
	UV = vUv;
	COLOUR = vColour;

	// Invoke the polyshader
	polyshader_vertex();

	gl_Position = ProjectionMatrix * ViewMatrix * POSITION_WORLD;
#if SHADERTYPE == LIGHTMAPPED
	pLightmappedUv = vLightmappedUv;
#endif
}

// Input for the polyshader's pixel pass
in vec3 POSITION; // in local position
in vec3 POSITION_WORLD; // in-out world position
in vec3 NORMAL; // in local normal
in vec3 NORMAL_WORLD; // in-out world normal
in vec2 UV; // in-out UV
in vec4 COLOUR; // in-out vertex colour

vec3 DIFFUSE; // out diffuse colour
float ALPHA; // out alpha

out vec4 outColour;

void main_ps()
{
	DIFFUSE = vec3( 1.0 );
	ALPHA = 1.0;

#if SHADERTYPE != DEPTH
	// Invoke the polyshader's pixel & opacity routines
	polyshader_pixel();
#else
	polyshader_alpha();
#endif

#if SHADERTYPE == GENERAL
	outColour.rgb = DIFFUSE;

#elif SHADERTYPE == LIGHTMAPPED
	vec3 lightmapColour = texture( LightmapTexture, pLightmapUv ).rgb;
	outColour.rgb = HardLight( DIFFUSE, lightmapColour );

#elif SHADERTYPE == DEPTH
	outColour.rgb = vec3( ALPHA );

#endif
	outColour.a = ALPHA;
}
```

Notice the lines with `polyshader_*`. For `Standard`, we'll have this polyshader:
* `standard_polyshader.glsl`
```glsl
uniform texture2D DiffuseTexture;

void vertex()
{
	// Can be omitted
}

void pixel()
{
	vec4 colour = texture( DiffuseTexture, UV );
	DIFFUSE = colour.rgb;
	ALPHA = colour.a;
}

void alpha()
{
	ALPHA = texture( DiffuseTexture, UV ).a;
}
```

After all this work, we finally arrive to the point of this document. Congrats on reading so far. This polyshader is ultimately what modders will deal with. You can make variations of this polyshader to support 3-way texture blending for instance, still using the `StandardST` shader template, which gives you lightmapping support for free. Something simple stupid like so:
```glsl
uniform texture2D DiffuseTextureA;
uniform texture2D DiffuseTextureB;
uniform texture2D DiffuseTextureC;

void vertex()
{

}

void pixel()
{
	vec3 colourA = texture( DiffuseTextureA, UV ) * COLOUR.r;
	vec3 colourB = texture( DiffuseTextureB, UV ) * COLOUR.g;
	vec3 colourC = texture( DiffuseTextureC, UV ) * COLOUR.b;
	// There are waaaaaay better ways to do this,
	// but but, just to get the idea across
	DIFFUSE = colourA + colourB + colourC;
}

void alpha()
{
	ALPHA = 1.0;
}
```

# Todo

I think the primary aspect which will be worked on is simplification. In either case, a custom shader tool will need to be written, which uses `glslValidator` to compile the shaders to SPIR-V.

Still left to figure out is how the data-driven part of this will integrate with the renderer API. Shader templates and the renderer need to be in agreement on what they want from each other.

Material templates just need to correspond to a shader template. The rest is more or less a free-for-all. This requires a nice, automated shader resource mapping system.

# Vertical slice example

Let's focus on one shader template, with one material template, and one material.

Starting off with `materials/stemplates/standard.json`:
```json
{
	"name": "StandardST",
	"shaderFile": "materials/stemplates/standard_template.glsl",

	// This here expands to:
	// #define GENERAL 1
	// #define LIGHTMAPPED 2
	// #define DEPTH 3
	"shaderTypes": [ "GENERAL", "LIGHTMAPPED," "DEPTH" ],
	"shaders":
	[
		// General shader, for general usecases
		{
			"shaderTypeDefine": "GENERAL", // #define SHADERTYPE GENERAL
			"vertexLayout":
			[
				{ "name": "Position", "type": "vec3" },
				{ "name": "Uv", "type": "vec2" },
				{ "name": "Normal", "type": "vec3" },
				{ "name": "Colour", "type": "vec4" }
			],
			"resourceLayout":
			[
				// Projection matrix, view matrix etc. change per frame and go here
				{ "elements": [ { "name": "PerFrameBuffer", "type": "buffer" } ] },
				// Model matrix, with the addition of bone transformation matrices elsewhere
				{ "elements": [ { "name": "PerEntityBuffer", "type": "buffer" } ] }
			]
		},
		// Lightmapped shader, for lightmapped surfaces
		{
			"shaderTypeDefine": "LIGHTMAPPED", // #define SHADERTYPE LIGHTMAPPED
			"vertexLayout":
			[
				{ "name": "Position", "type": "vec3" },
				{ "name": "Uv", "type": "vec2" },
				{ "name": "Normal", "type": "vec3" },
				{ "name": "Colour", "type": "vec4" }
				{ "name": "LightmapUv", "type": "vec2" },
			],
			"resourceLayout":
			[
				// Projection matrix, view matrix etc. change per frame and go here
				{ "elements": [ { "name": "PerFrameBuffer", "type": "buffer" } ] },
				// Model matrix, with the addition of bone transformation matrices elsewhere
				{ "elements": [ { "name": "PerEntityBuffer", "type": "buffer" } ] },
				// Lightmap texture
				{ "elements": [ { "name": "LightmapTexture", "type": "texture" } ] }
			]
		},
		// Depth prepass shader
		{
			"shaderTypeDefine": "DEPTH", // #define SHADERTYPE DEPTH
			"vertexLayout":
			[
				{ "name": "Position", "type": "vec3" },
				{ "name": "Uv", "type": "vec2" }
			],
			"resourceLayout":
			[
				// Projection matrix, view matrix etc. change per frame and go here
				{ "elements": [ { "name": "PerFrameBuffer", "type": "buffer" } ] },
				// Model matrix, with the addition of bone transformation matrices elsewhere
				{ "elements": [ { "name": "PerEntityBuffer", "type": "buffer" } ] }
			]
		}
	]
}
```

Then moving to `materials/mtemplates/standard_twosided_alphatested.json`:
```json
{
	"name": "StandardTwoSidedAlphatested",
	"shaderTemplate": "StandardST",
	"polyshaderFile": "materials/shaders/standard_polyshader.glsl",
	"pipelineInfo":
	{
		"faceCulling": "back",
		"blending": "opaque"
	},
	"parameters":
	[
		{ "name": "diffuse", "type": "texture", "boundResource": "DiffuseTexture" }
	]
}
```

The shader code can remain the same as it is above, so I don't copy-paste again.
