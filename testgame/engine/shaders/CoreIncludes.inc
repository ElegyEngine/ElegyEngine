
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
#ifndef SHADER_TYPE
#define VertexInput( id, datatype, name, visibility ) \
layout( location = id ) in datatype name;
#else
#define VertexInput( id, datatype, name, visibility )
#endif

// Defines a pixel shader input/output.
#ifndef SHADER_TYPE
#define PixelInput( id, datatype, name, visibility ) \
layout( location = id ) out datatype name;
#else
#define PixelInput( id, datatype, name, visibility )
#endif

#ifndef SHADER_TYPE
#define PixelOutput( name ) \
layout( location = 0 ) out vec4 name;
#else
#define PixelOutput( name )
#endif

// Defines a simple material parameter.
// Params:
// * set_id       => Resource set ID.
//
// * binding_id   => Binding ID within the resource set.
//
// * datatype     => The shader datatype (vec2, vec3, sampler2D, texture2D...)
//
// * shaderName   => Param name within the shader.
//
// * materialName => Param name within the material template (and game code).
//
// * visibility   => Which ShaderVariants this compiles into.
//                   It is possible to combine it with "except", like 'ALL except GENERAL WIREFRAME'.
//
// * level        => A level of indirection. Determines how, when and by whom this parameter is updated.
//                   Possible values:
//                    * BUILTIN  => "Built-in" as in, the renderer expects it here. Updated at any time.
//                                  Updated usually by the renderer and indirectly by game code. Example usecase: camera variables.
//                    * DATA     => This is material data, initialised from files and normally never touched afterwards.
//                    * GLOBAL   => There is only a single instance of this for all materials.
//                                  Usually is updated by game code or a render style. Example usecase: global weather data.
//                    * INSTANCE => Updated at any time. Typically used for game-specific effects, or by a render style.
//                                  Example usecase: lightmaps, colour gradients, overrideable individual textures...
// Examples:
// MaterialParameter( 1, 1, vec4, pLightmapOffsets, LightmapOffsets, ALL, INSTANCE )
// MaterialParameter( 2, 3, vec4, pEmissiveColour, EmissiveColour, ALL except WIREFRAME, DATA )
// MaterialParameter( 2, 1, vec2, pSnowVector, SnowVector, ALL except NOWEATHER, GLOBAL )
#ifndef SHADER_TYPE
#define MaterialParameter( set_id, binding_id, datatype, shaderName, materialName, visibility, level ) \
layout( set = set_id, binding = binding_id ) uniform datatype shaderName;
#else
#define MaterialParameter( set_id, binding_id, datatype, shaderName, materialName, visibility, level )
#endif

// Defines a more complex material parameter.
// These are typically not editable material definitions. Refer to MaterialParameter's parametres.
// Example:
/*
	MaterialParameterBuffer( 1, 2, CameraBuffer, pCamera, CameraInfo, ALL, BUILTIN,
		mat4 View;
		mat4 Proj;
	);
*/
#ifndef SHADER_TYPE
#define MaterialParameterBuffer( set_id, binding_id, structName, shaderName, materialName, visibility, level, structContents ) \
layout ( set = set_id, binding = binding_id ) uniform __##shaderName \
{ \
	structName shaderName; \
}
#else
#define MaterialParameterBuffer( set_id, binding_id, structName, shaderName, materialName, visibility, level, structContents )
#endif

// Defines the vertex shader region.
#define VertexShader()
// Defines the pixel shader region.
#define PixelShader()
