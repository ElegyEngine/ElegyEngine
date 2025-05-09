
ShaderTemplate( Standard_DynamicTexST );
ShaderVariants( GENERAL );

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
MaterialParameterSet( BUILTIN, ALL except DEPTH WIREFRAME,
	Data( texture2D, uDiffuseTexture, DiffuseMap )
);
MaterialParameterSet( GLOBAL, ALL,
	Data( sampler, uSampler, Sampler )
);

// Vertex shader inputs (from device)
VertexInput( 0, vec3, vPosition, ALL );
VertexInput( 1, vec3, vNormal, ALL except DEPTH WIREFRAME );
VertexInput( 2, vec2, vUv, ALL except DEPTH WIREFRAME );
VertexInput( 3, vec2, vUvLightmap, LIGHTMAP );

// Vertex shader to pixel shader IO
PixelInput( 0, vec3, pPosition, ALL except WIREFRAME );
PixelInput( 1, vec3, pNormal, ALL except WIREFRAME );
PixelInput( 2, vec2, pUv, ALL except DEPTH WIREFRAME );
PixelInput( 3, vec2, pUvLightmap, LIGHTMAP );

vec4 CalculateGlPosition( vec3 worldspacePosition )
{
	return uView.ProjMatrix * uView.ViewMatrix * uEntity.EntityMatrix * vec4( worldspacePosition, 1.0 );
}

// GENERAL
VertexShader( GENERAL,
	gl_Position = CalculateGlPosition( vPosition );
	pPosition = vPosition;
	pNormal = vNormal;
	pUv = vUv;
)
PixelShader( GENERAL,
	float halfLambert = dot( pNormal, normalize( vec3( 10.0, 20.0, 50.0 ) ) ) * 0.5 + 0.5;

	vec4 cDiffuse = texture( sampler2D( uDiffuseTexture, uSampler ), pUv );
	cDiffuse.rgb *= halfLambert;

	return cDiffuse + vec4( 0.01, 0.02, 0.025, 0.0 );
)
