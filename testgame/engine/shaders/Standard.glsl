
#include "CoreIncludes.inc"

#extension GL_EXT_fragment_shader_barycentric : enable

ShaderTemplate( StandardST );
ShaderVariants( GENERAL, LIGHTMAP, DEPTH, WIREFRAME );

// Shader parametres

// Per-frame data
MaterialParameterSet( BUILTIN, ALL,
	Buffer( uView, ViewData,
		mat4 ViewMatrix;
		mat4 ProjMatrix;
	);
);
// Per-entity data
MaterialParameterSet( BUILTIN, ALL,
	Buffer( uEntity, EntityData,
		mat4 EntityMatrix;
	);
);
// Per-material data
MaterialParameterSet( DATA, ALL except DEPTH WIREFRAME,
	Data( texture2D, uDiffuseTexture, DiffuseMap )
);
MaterialParameterSet( INSTANCE, LIGHTMAP,
	Data( texture2D, uLightmapTexture, LightmapMap )
);
MaterialParameterSet( GLOBAL, ALL except DEPTH WIREFRAME,
	Data( sampler, uSampler, Sampler )
);
MaterialParameterSet( GLOBAL, LIGHTMAP,
	Data( sampler, uLightmapSampler, LightmapSampler )
);

// Vertex shader inputs (from device)
VertexInput( 0, vec3, vPosition, ALL );
VertexInput( 1, vec3, vNormal, ALL except DEPTH WIREFRAME );
VertexInput( 2, vec2, vUv, ALL except DEPTH WIREFRAME );
VertexInput( 3, vec2, vUvLightmap, LIGHTMAP );

// Vertex shader to pixel shader IO
PixelInput( 0, vec3, pPosition, ALL except WIREFRAME );
PixelInput( 1, vec2, pUv, ALL except DEPTH WIREFRAME );
PixelInput( 2, vec2, pUvLightmap, LIGHTMAP );

vec4 CalculateGlPosition( vec3 worldspacePosition )
{
	return uView.ProjMatrix * uView.ViewMatrix * uEntity.EntityMatrix * vec4( worldspacePosition, 1.0 );
}

// GENERAL
VertexShader( GENERAL,
	gl_Position = CalculateGlPosition( vPosition );
	pPosition = vPosition;
	pUv = vUv;
)
PixelShader( GENERAL,
	return texture( sampler2D( uDiffuseTexture, uSampler ), pUv );
)

// LIGHTMAP
float blendOverlay(float base, float blend)
{
	const float condition = float(base < 0.5);
	const float anticondition = float(base >= 0.5);

	return condition * (2.0 * base * blend)
		+ anticondition * (1.0 - 2.0 * (1.0-base) * (1.0-blend));
}

vec3 blendOverlay(vec3 base, vec3 blend)
{
	return vec3(
		blendOverlay( base.r,blend.r ),
		blendOverlay( base.g,blend.g ),
		blendOverlay( base.b,blend.b ) );
}

VertexShader( LIGHTMAP,
	GENERAL();
	pUvLightmap = vUvLightmap;
)
PixelShader( LIGHTMAP,
	const vec4 cDiffuse = GENERAL();
	const vec4 cLightmap = texture( sampler2D( uLightmapTexture, uLightmapSampler ), pUvLightmap );
	return vec4( blendOverlay( cLightmap.rgb, cDiffuse.rgb ), cDiffuse.a );
)

// DEPTH
VertexShader( DEPTH, gl_Position = CalculateGlPosition( vPosition ); )
PixelShader( DEPTH, return vec4( 1.0 ); )

// WIREFRAME
VertexShader( WIREFRAME, gl_Position = CalculateGlPosition( vPosition ); )
PixelShader( WIREFRAME,
	// Further away from the edge = bigger number
	const vec3 barycentricCoords = gl_BaryCoordEXT;
	const float closestEdge = min( barycentricCoords.x, min( barycentricCoords.y, barycentricCoords.z ) );
	// This gives us 1.0 for edges, 0.0 for face area
	const float wireframeAlpha = 1.0 - smoothstep( 0.0, 0.01, closestEdge );

	return vec4( 0.0, 1.0, 0.35, wireframeAlpha );
)
