
// Shader template: Standard
// Shader variant: General
module StandardGeneral;

import CoreIncludes;
import StandardCore; // View and Entity
import StandardMaterial; // DiffuseTexture and Sampler

struct PixelInput
{
	float3 Normal;
	float2 Uv;
}

[shader("vertex")]
VertexOutputWith<PixelInput> vertexMain( InPositionNormalUv i )
{
	VertexOutputWith<PixelInput> o;
	o.OutputPosition = Entity.CalculateVertexPosition( View, i.Position );
	o.Pixel.Normal = Entity.CalculateVertexNormal( i.Normal );
	o.Pixel.Uv = i.Uv;
	return o;
}

[shader("pixel")]
float4 pixelMain( PixelInput i : PIXEL_INPUT ) : SV_Target
{
	return CalcDiffuseComponent(DiffuseTexture, Sampler, i.Normal, i.Uv);
}
