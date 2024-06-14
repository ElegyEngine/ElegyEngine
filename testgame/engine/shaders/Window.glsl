
#include "CoreIncludes.inc"

ShaderTemplate( WindowST );
ShaderVariants( DEFAULT );
ShaderPostProcessHint();

// Shader parametres
MaterialParameterSet( BUILTIN, ALL,
	Data( texture2D, uViewTexture, ViewTexture ),
	Data( sampler, uViewSampler, ViewSampler )
);
// Vertex shader inputs (from device)
VertexInput( 0, vec2, vPosition, ALL );
VertexInput( 1, vec2, vUv, ALL );
// Vertex shader to pixel shader IO
PixelInput( 0, vec2, pPosition, ALL );
PixelInput( 1, vec2, pUv, ALL );

VertexShader( DEFAULT,
	gl_Position = vec4( vPosition, 0.5, 1.0 );
	pPosition = vPosition;
	pUv = vec2( vUv.x, 1.0 - vUv.y );
)

PixelShader( DEFAULT,
	return texture( sampler2D( uViewTexture, uViewSampler ), pUv );
)
