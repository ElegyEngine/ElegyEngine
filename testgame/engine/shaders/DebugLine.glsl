
#include "CoreIncludes.inc"

ShaderTemplate( DebugLineST );
ShaderVariants( GENERAL, SCREEN );

// Shader parametres
MaterialParameterSet( BUILTIN, ALL,
	Buffer( uView, ViewData,
		mat4 ViewMatrix;
		mat4 ProjMatrix;
	);
);

// Vertex shader inputs (from device)
VertexInput( 0, vec3, vPosition, ALL );
VertexInput( 1, vec3, vNormal, ALL );
VertexInput( 2, vec2, vUv, ALL );
VertexInput( 3, vec4, vColour, ALL );
// Vertex shader to pixel shader IO
PixelInput( 0, vec4, pColour, ALL );

// TODO: might wanna rename this to ToScreenspace?
vec4 CalculateGlPosition( vec3 worldspacePosition, float w )
{
	return uView.ProjMatrix * uView.ViewMatrix * vec4( worldspacePosition, w );
}

vec2 CalculateOrthogonal2D( vec2 v )
{
	return vec2( -v.y, v.x );
}

// This function basically grabs a screenspace normal, and depending on the thickness & sign
// encoded in the UV, gives us a horizontal expansion relative to the line, i.e. thickens the line
// TODO: take into account depth and shrink the lineFactor by that
vec4 CalculateScreenspaceOffset( vec2 normal, vec2 uv )
{
	vec2 screenspaceNormalTangent = normalize( CalculateOrthogonal2D( normal ) );
	float directionSign = uv.x;
	float thickness = uv.y;
	float lineFactor = 0.005 * thickness * directionSign;

	return vec4( screenspaceNormalTangent * lineFactor, 0.0, 0.0 );
}

// GENERAL
VertexShader( GENERAL,
	vec4 screenspacePosition = CalculateGlPosition( vPosition, 1.0 );
	vec2 screenspaceNormal = CalculateGlPosition( vNormal, 0.0 ).xy;

	gl_Position = screenspacePosition + CalculateScreenspaceOffset( screenspaceNormal, vUv );
	pColour = vColour;
);
PixelShader( GENERAL,
	return pColour;
);

// SCREEN
VertexShader( SCREEN,
	gl_Position = vec4( vPosition.xy, 0.5, 1.0 ) + CalculateScreenspaceOffset( vNormal.xy, vUv );
	pColour = vColour;
);
PixelShader( SCREEN,
	return pColour;
);
