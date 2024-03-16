
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
// MaterialParameter( 1, 1, vec4, pLightmapOffsets, LightmapOffsets, ALL )
#ifndef SHADER_TYPE
#define MaterialParameter( set_id, binding_id, datatype, shaderName, materialName, visibility ) \
layout( set = set_id, binding = binding_id ) uniform datatype shaderName;
#else
#define MaterialParameter( set_id, binding_id, datatype, shaderName, materialName, visibility )
#endif

// Defines a more complex material parameter.
// These are typically not editable material definitions.
/*
struct CameraBuffer { mat4 View; mat4 Proj; };
MaterialParameterBuffer( 1, 2, CameraBuffer, pCamera, CameraInfo, ALL );
*/
#ifndef SHADER_TYPE
#define MaterialParameterBuffer( set_id, binding_id, structName, shaderName, materialName, visibility ) \
layout ( set = set_id, binding = binding_id ) uniform __##shaderName \
{ \
	structName shaderName; \
}
#else
#define MaterialParameterBuffer( set_id, binding_id, structName, shaderName, materialName, visibility )
#endif

// Defines the vertex shader region.
#define VertexShader()
// Defines the pixel shader region.
#define PixelShader()
