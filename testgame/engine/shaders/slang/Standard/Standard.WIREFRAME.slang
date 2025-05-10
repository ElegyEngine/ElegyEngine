
// For reasoning, refer to Standard.DEPTH.slang.

// Shader template: Standard
// Shader variant: Wireframe
module StandardWireframe;

import CoreIncludes;
import StandardCore;

[shader("vertex")]
float4 vertexMain( InPositionUv i ) : SV_Position
{
	return Entity.CalculateVertexPosition( View, i.Position );
}

[shader("pixel")]
[require(spvFragmentBarycentricKHR)]
float4 pixelMain( float4 i: SV_Position, float3 bary : SV_Barycentrics ) : SV_Target
{
	// Further away from the edge = bigger number
	const float closestEdge = min( bary.x, min( bary.y, bary.z ) );
	// This gives us 1.0 for edges, 0.0 for face area
	const float wireframeAlpha = 1.0 - smoothstep( 0.0, 0.01, closestEdge );

	return float4( 0.0, 1.0, 0.35, wireframeAlpha );
}
