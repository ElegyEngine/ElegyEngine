
module CoreIncludes;

// Data inputs

public struct ViewData
{
	public float4x4 Projection;
	public float4x4 View;

	public float4x4 ViewProjection;
}

public struct EntityData
{
	public float4x4 World;

	public float4 Misc1;
	public float4 Misc2;
	public float4 Misc3;
	public float4 Misc4;

	public float4 CalculateVertexPosition( ViewData view, float3 worldPosition )
	{
		float4x4 wvp = mul(view.ViewProjection, World);
		return mul(wvp, float4(worldPosition, 1.0));
	}

	public float3 CalculateVertexNormal( float3 worldNormal )
	{
		return mul(World, float4(worldNormal, 0.0)).xyz;
	}
}

public struct EntityJointData
{
	public float4x4 Temp;
}

public struct BatchData
{
	public float4x4 Temp;
}

// Shader IO

public struct InPositionUv
{
	public float3 Position : POSITION;
	public float2 Uv : TEXCOORD0;
}

public struct InPositionNormalUv
{
	public float3 Position : POSITION;
	public float3 Normal : NORMAL;
	public float2 Uv : TEXCOORD0;
}

public struct VertexOutputWith<TPixelInput>
{
	public TPixelInput Pixel : PIXEL_INPUT;
	public float4 OutputPosition : SV_Position;
}

// Shading utilities

public float4 CalcDiffuseComponent(Texture2D texture, SamplerState sampler, float3 normal, float2 uv)
{
	float halfLambert = dot(normal, normalize(float3(10.0, 20.0, 50.0))) * 0.5 + 0.5;

	float4 cDiffuse = texture.Sample(sampler, uv);
	cDiffuse.rgb *= halfLambert;

	return cDiffuse + float4(0.01, 0.02, 0.025, 0.0);
}

// Elegy-specific attributes

[__AttributeUsage(_AttributeTargets.Var)]
public struct BuiltinAttribute
{
}

[__AttributeUsage(_AttributeTargets.Var)]
public struct InstanceAttribute
{
}

[__AttributeUsage(_AttributeTargets.Var)]
public struct DataAttribute
{
}

[__AttributeUsage(_AttributeTargets.Var)]
public struct GlobalAttribute
{
}
