
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
	pUv = vUv;
}

#endif

#if PIXEL_SHADER == 1

// Pixel shader inputs (from vertex shader)
layout( location = 0 ) in vec2 pPosition;
layout( location = 1 ) in vec2 pUv;

// Pixel shader outputs (to framebuffer)
layout( location = 0 ) out vec4 outColour;

void main_ps()
{
	outColour = vec4( 1.0 );
	outColour.rg = pUv.xy;
}

#endif
