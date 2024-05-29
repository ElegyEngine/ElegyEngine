
#include "CoreIncludes.inc"

ShaderTemplate( WindowST );
ShaderVariants( DEFAULT );

// Shader parametres
MaterialParameterSet( 0, DEFAULT, BUILTIN,
	Data( texture2D, uViewTexture, ViewTexture ),
	Data( sampler, uViewSampler, ViewSampler )
);
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
	pUv = vec2( vUv.x, 1.0 - vUv.y );
)

PixelShader( DEFAULT,
	return texture( sampler2D( uViewTexture, uViewSampler ), pUv );
)
