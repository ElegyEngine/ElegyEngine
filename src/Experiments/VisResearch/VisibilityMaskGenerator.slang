
module VisMaskGen;

struct VisBakingInfo
{
	uint4 mValue;
	
	property uint Start
	{
		get {
			return mValue.x;
		}
	}
	
	property uint ProbeStride
	{
		get {
			return mValue.y;
		}
	}
	
	property uint ChunkStride
	{
		get {
			return mValue.z;
		}
	}
}

[vk::binding(0, 0)]
Texture2DArray VisMap;

[vk::binding(1, 0)]
RWStructuredBuffer<uint> VisMask;

[vk::binding(2, 0)]
ConstantBuffer<VisBakingInfo> VisParams;

[shader("compute")]
[numthreads(32, 32, 1)]
void main_cs(uint3 thread: SV_DispatchThreadID)
{
	uint4 location = uint4(thread.x, thread.y, thread.z, 0);

	// The chunk ID visible on this view
	uint baseChunkIndex = (uint)(VisMap.Load(location).x * 256.0f);
	
	// Stride = 6 * numProbes. Find out which chunklet this view belongs to
	uint viewIndex = VisParams.Start + thread.z;
	uint chunkletIndex = viewIndex / VisParams.ProbeStride;
	
	// ChunkStride = numChunks
	VisMask[baseChunkIndex + chunkletIndex * VisParams.ChunkStride] = 1;
}
