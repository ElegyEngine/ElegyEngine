
#include "CoreIncludes.inc"

ShaderTemplate( DebugST );
ShaderVariants( DEFAULT );

// Vertex shader inputs (from device)
VertexInput( 0, vec2, vPosition, DEFAULT );
VertexInput( 1, vec2, vUv, DEFAULT );
// Vertex shader to pixel shader IO
PixelInput( 0, vec2, pPosition, DEFAULT );
PixelInput( 1, vec2, pUv, DEFAULT );
// Pixel shader outputs (to framebuffer)
PixelOutput( outColour );

VertexShader( DEFAULT,
	gl_Position = vec4( vPosition, 0.5, 1.0 );
	pPosition = vPosition;
	pUv = vUv;
)

PixelShader( DEFAULT,
	return vec4( pUv.x, pUv.y, 1.0, 1.0 );
)
