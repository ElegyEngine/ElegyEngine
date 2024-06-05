
// These defines here help set some things up differently between vertex & pixel shaders.
#define SHADER_VERTEX 1
#define SHADER_PIXEL 2

// Defines a shader template.
// ShaderTemplate( StandardST );
#define ShaderTemplate( name )

// Defines shader variants. 'ALL' is a reserved variant, encompassing all of them.
// ShaderVariants( GENERAL, LIGHTMAPPED, DEPTH, WIREFRAME )
#define ShaderVariants()

// Defines a vertex input.
// VertexInput( 0, vec3, vPosition, ALL );
// VertexInput( 1, vec2, vUv, ALL );
// VertexInput( 2, vec2, vUvLightmap, LIGHTMAPPED );
#define VertexInput( id, datatype, name, visibility )

// Defines a pixel shader input/output.
#define PixelInput( id, datatype, name, visibility )
#define PixelOutput( name )

// Defines a simple material parameter.
// Params:
// * datatype     => The shader datatype (vec2, vec3, sampler2D, texture2D...)
//
// * shaderName   => Param name within the shader.
//
// * materialName => Param name within the material template (and game code).
//
// Examples:
// Param( vec4, pLightmapOffsets, LightmapOffsets ),
// Param( vec4, pEmissiveColour, EmissiveColour ),
// Param( vec2, pSnowVector, SnowVector )
#define Param( datatype, shaderName, materialName )

// Defines a more complex material parameter.
// These are typically not editable material definitions. Refer to MaterialParameter's parametres.
// Example:
/*
	Buffer( pCamera, CameraInfo,
		mat4 View;
		mat4 Proj;
	);
*/
#define Buffer( shaderName, materialName, structContents )

// Defines a set of material parameters.
// Params:
// * set_id     => Resource set ID.
//
// * visibility => Which ShaderVariants this compiles into.
//                   It is possible to combine it with "except", like 'ALL except GENERAL WIREFRAME'.
//
// * level      => A level of indirection. Determines how, when and by whom this parameter is updated.
//                   Possible values:
//                    * BUILTIN  => "Built-in" as in, the renderer expects it here. Updated at any time.
//                                  Updated usually by the renderer and indirectly by game code. Example usecase: camera variables.
//                    * DATA     => This is material data, initialised from files and normally never touched afterwards.
//                    * GLOBAL   => There is only a single instance of this for all materials.
//                                  Usually is updated by game code or a render style. Example usecase: global weather data.
//                    * INSTANCE => Updated at any time. Typically used for game-specific effects, or by a render style.
//                                  Example usecase: lightmaps, colour gradients, overrideable individual textures...
// Examples:
// MaterialParameterSet( 0, ALL, BUILTIN,
//   Buffer( CameraBuffer, pCamera, CameraInfo,
//     mat4 View;
//     mat4 Proj;
//   )
// )
// MaterialParameterSet( 1, INSTANCE, ALL,
//   Data( vec3, pPositionOffset, PositionOffset ),
//   Data( vec2, pUvOffset, UvOffset ),
//   Data( float, pTime, Time )
// )
// MaterialParameterSet( 2, GLOBAL, ALL except SUMMER,
//   Param( vec2, pSnowVector, SnowVector )
// )
#define MaterialParameterSet( set_id, level, visibility, parameters )

// Defines the vertex shader region.
#define VertexShader()
// Defines the pixel shader region.
#define PixelShader()
