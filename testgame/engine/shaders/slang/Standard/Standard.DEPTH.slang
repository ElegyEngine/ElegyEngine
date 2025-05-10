
// The reason we have a depth pass as a shader variant is:
// water surfaces and vegetation might be bending the vertices one
// way or another, so their depth output will be a little different.

// In depth shader variants, all that's done is logic that modifies
// the vertex position. That's all. We're only interested in the clipspace
// position of the vertex.

// Shader template: Standard
// Shader variant: Depth
module StandardDepth;

import CoreIncludes;
import StandardCore; // View and Entity

[shader("vertex")]
float4 vertexMain( InPositionUv i ) : SV_Position
{
	// With water, we would have a situation like this:
	// float3 worldPosition = i.Position + Water.CalculateVertexOffset(Time.Seconds);
	// return Entity.CalculateVertexPosition(View, worldPosition);

	// To reduce code duplication, we can use utility functions, methods
	// like in Entity, or even take advantage of interfaces and generics.
	return Entity.CalculateVertexPosition(View, i.Position);
}

[shader("pixel")]
float4 pixelMain( float4 i : PIXEL_INPUT ) : SV_Target
{
	return float4( float3( 1.0 ), i.w );
}
