
// Shader template: Standard
// Shader variant: Lightmapped
module StandardLightmapped;

import CoreIncludes;
import StandardCore; // View and Entity
import StandardMaterial; // DiffuseTexture and Sampler

[Data] Texture2D LightmapTexture;
[Global] SamplerState LightmapSampler;

struct VertexInput
{
	InPositionNormalUv General;
    float2 LightmapUv : TEXCOORD1;
}

struct PixelInput
{
	float3 Normal;
    float2 Uv;
    float2 LightmapUv;
}

[shader("vertex")]
VertexOutputWith<PixelInput> vertexMain( VertexInput i )
{
	VertexOutputWith<PixelInput> o;
	o.OutputPosition = Entity.CalculateVertexPosition( View, i.General.Position );
	o.Pixel.Normal = Entity.CalculateVertexNormal( i.General.Normal );
	o.Pixel.Uv = i.General.Uv;
	o.Pixel.LightmapUv = i.LightmapUv;
	return o;
}

[shader("pixel")]
float4 pixelMain( PixelInput i : PIXEL_INPUT ) : SV_Target
{
	float4 cDiffuse = CalcDiffuseComponent(DiffuseTexture, Sampler, i.Normal, i.Uv);
	float4 cLightmap = LightmapTexture.Sample(LightmapSampler, i.LightmapUv);
	return cDiffuse * cLightmap;
}
