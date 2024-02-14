
#version 450

#if VERTEX_SHADER == 1

// Vertex shader inputs (from device)
layout( location = 0 ) in vec2 vPosition;
layout( location = 1 ) in vec2 vUv;

// Vertex shader outputs (to pixel shader)
layout( location = 0 ) out vec2 pPosition;
layout( location = 1 ) out vec2 pUv;

void main_vs()
{
	gl_Position = vec4( vPosition, 0.5, 1.0 );
	pPosition = vPosition;
	pUv = vec2( vUv.x, 1.0 - vUv.y );
}

#endif

#if PIXEL_SHADER == 1

// Pixel shader inputs (from vertex shader)
layout( location = 0 ) in vec2 pPosition;
layout( location = 1 ) in vec2 pUv;

// Pixel shader outputs (to framebuffer)
layout( location = 0 ) out vec4 outColour;

layout( set = 0, binding = 0 ) uniform texture2D ViewTexture;
layout( set = 0, binding = 1 ) uniform sampler ViewSampler;

void main_ps()
{
	outColour = texture( sampler2D( ViewTexture, ViewSampler ), pUv );
}

#endif
