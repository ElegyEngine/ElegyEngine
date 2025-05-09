using System.Diagnostics;
using System.Runtime.InteropServices;
using Elegy.RenderBackend.Extensions;
using Veldrid;
using Veldrid.Vulkan;

namespace VisResearch;

internal static class StopwatchExtensions
{
	public static double GetSeconds( this Stopwatch stopwatch )
		=> (double)stopwatch.ElapsedTicks / Stopwatch.Frequency;

	public static double GetMilliseconds( this Stopwatch stopwatch )
		=> stopwatch.GetSeconds() * 1000.0;

	public static double GetMicroseconds( this Stopwatch stopwatch )
		=> stopwatch.GetMilliseconds() * 1000.0;
}

[StructLayout( LayoutKind.Sequential )]
internal struct VisibilityBakingInfo
{
	/// <summary>
	/// Which probe view to start on. This can be non-zero in case the workload is split across multiple dispatches.
	/// E.g. the GPU could only do 512 texture layers but we needed 600 of them. Must be a multiple of 6, since
	/// the shader assumes it's so.
	/// </summary>
	public uint Start;

	/// <summary>
	/// The compute shader needs to know where exactly it transitions from one chunklet to the next.
	/// Calculates at 6 x number of probes in chunklet.
	/// </summary>
	public uint ProbeStride;

	/// <summary>
	/// How many chunks there are in the map.
	/// </summary>
	public uint ChunkStride;

	private uint mDummy;
}

/*
Ideas on how to computationally structure this mess:

Let's assume a somewhat larger map, made up of 250 chunks (NumChunks = 250).
A 4x4x4 chunk with simple 1-probe chunklets (NumChunklets = 64, NumProbes = 1) has 384 views. (96k views for 250 chunks)

Each chunk would have one visibility buffer associated with it, laid out like so:
uint[NumChunks * NumChunklets * NumProbes]

Computation, with numbers laid out above:

Create 90° square aspect ratio perspective matrix.
ALTERNATIVE: 6 premultiplied viewprojection matrices.

For each chunk: (MT)
	Create array texture (384):
		If the array texture's layers exceed the GPU's max layers, create more.
	Create visibility buffer. (250 x 64 indices, so 250 potential IDs for each chunklet)
	Create baking information. (strides, counts, offsets...)

	For every view:
		Create view matrices, premultiplied with perspective matrix above.
		ALTERNATIVE: Create view offsets instead.

For each chunk:
	For every [max multiview] views:
		Render sequentially into above array texture, using multiview.

For each chunk:
	Process array texture, either in whole or in parts:
		Fill the visibility buffer using baking information and above rendered images.

For each chunk: (MT)
	Compress visibility buffer:
		Generate sorted list of visible chunk IDs.
		For each chunklet:
			Generate a bitfield where each bit represents one of the chunk IDs.
*/

public static class Program
{
	// All input images will be 256x256
	private const int ImageResolution = 256;
	private const int ComputeGroupSize = 32;
	private const int DispatchSize = ImageResolution / ComputeGroupSize;

	// TODO: get optimal number of layers from device limits
	// If it's 1024, take 1020 layers (divisible by 6)
	private const int NumLayers = ExampleNumChunklets * ExampleNumProbes * 6;
	private const int NumIterations = ExampleNumChunks;
	private const int NumViews = NumLayers * NumIterations;

	private const int ExampleNumChunks = 150;
	private const int ExampleNumChunklets = 4 * 4 * 4;
	private const int ExampleNumProbes = 1;

	private static Stopwatch mStopwatch;

	public static void Main( string[] args )
	{
		VkGraphicsDevice device = (VkGraphicsDevice)GraphicsDevice.CreateVulkan( new()
		{
			HasMainSwapchain = false,
			Debug = true,
			ResourceBindingModel = ResourceBindingModel.Improved
		} );
		VkCommandList commandList = (VkCommandList)device.ResourceFactory.CreateCommandList();

		// Beware of the pipeline.
		ComputePipelineDescription pipelineDesc = new()
		{
			ComputeShader = device.ResourceFactory.LoadShaderDirect( "VisibilityMaskGenerator", ShaderStages.Compute, debug: true ),
			ResourceLayouts =
			[
				device.ResourceFactory.CreateResourceLayout( new()
				{
					Elements =
					[
						new() { Kind = ResourceKind.TextureReadOnly, Stages = ShaderStages.Compute },
						new() { Kind = ResourceKind.StructuredBufferReadWrite, Stages = ShaderStages.Compute },
						new() { Kind = ResourceKind.UniformBuffer, Stages = ShaderStages.Compute }
					]
				} )
			]
		};
		VkPipeline pipeline = (VkPipeline)device.ResourceFactory.CreateComputePipeline( pipelineDesc );

		// Shader parametres
		VkBuffer visibilityBuffer = (VkBuffer)device.ResourceFactory.CreateBuffer( new()
		{
			SizeInBytes = ExampleNumChunks * ExampleNumChunklets * 4,
			Usage = BufferUsage.StructuredBufferReadWrite,
			StructureByteStride = 4
		} );

		// We're gonna use a staging buffer to read back the processed stuff
		VkBuffer visibilityReadBuffer = (VkBuffer)device.ResourceFactory.CreateBuffer( new()
		{
			SizeInBytes = ExampleNumChunks * ExampleNumChunklets * 4,
			Usage = BufferUsage.StagingRead
		} );
		MappedResourceView<uint> visibilityReadData = device.Map<uint>( visibilityReadBuffer, MapMode.Read );

		// Buffer with workload info
		VkBuffer bakingInfo = (VkBuffer)device.ResourceFactory.CreateBufferForStruct<VisibilityBakingInfo>( BufferUsage.UniformBuffer );
		device.UpdateBuffer( bakingInfo, 0, new VisibilityBakingInfo
		{
			Start = 0,
			ProbeStride = 6 * ExampleNumProbes,
			ChunkStride = ExampleNumChunks
		} );

		// Quick little texture with patterns
		VkTexture visibilityMap = (VkTexture)device.ResourceFactory.CreateTexture( new()
		{
			Width = ImageResolution,
			Height = ImageResolution,
			Depth = 1,
			MipLevels = 1,
			ArrayLayers = NumLayers,
			Format = PixelFormat.B8_G8_R8_A8_UNorm,
			Type = TextureType.Texture2D,
			Usage = TextureUsage.Sampled,
			SampleCount = TextureSampleCount.Count1
		} );

		mStopwatch = Stopwatch.StartNew();

		byte[] visibilityMapData = new byte[ImageResolution * ImageResolution * 4];

		for ( int layer = 0; layer < visibilityMap.ArrayLayers; layer++ )
		{
			for ( int i = 0; i < 256 * 256; i++ )
			{
				visibilityMapData[i * 4 + 0] = (byte)((layer * 4 + i * 8) % ExampleNumChunks);
				visibilityMapData[i * 4 + 1] = (byte)((layer * 4 + i * 8) % ExampleNumChunks);
				visibilityMapData[i * 4 + 2] = (byte)((layer * 4 + i * 8) % ExampleNumChunks);
				visibilityMapData[i * 4 + 3] = 255;
			}

			device.UpdateTexture( visibilityMap, visibilityMapData.AsSpan(), 0, 0, 0, 256, 256, 1, 0, (uint)layer );
		}

		mStopwatch.Stop();

		Console.WriteLine( $"Elapsed texture generation time: {mStopwatch.GetMilliseconds():F2} ms" );

		VkResourceSet set = (VkResourceSet)device.ResourceFactory.CreateSet(
			pipelineDesc.ResourceLayouts[0], visibilityMap, visibilityBuffer, bakingInfo );

		mStopwatch.Restart();

		commandList.Begin();
		commandList.SetPipeline( pipeline );

		// Too lazy to generate more varied parametres so we'll simulate the other
		// chunks by just repeating everything we did thus far.
		for ( int i = 0; i < NumIterations; i++ )
		{
			commandList.SetComputeResourceSet( 0, set );
			commandList.Dispatch( DispatchSize, DispatchSize, visibilityMap.ArrayLayers );
		}

		commandList.End();
		device.SubmitCommands( commandList );
		device.WaitForIdle();

		commandList.Begin();
		commandList.CopyBuffer( visibilityBuffer, 0, visibilityReadBuffer, 0, visibilityBuffer.SizeInBytes );
		commandList.End();
		device.SubmitCommands( commandList );
		device.WaitForIdle();

		mStopwatch.Stop();

		var visibilitySpan = visibilityReadData.AsSpan();

		Console.WriteLine( $"CPU and GPU time: {mStopwatch.GetMilliseconds():F2} ms for {NumViews} views" );
		Console.WriteLine( $"                  ({mStopwatch.GetMicroseconds() / NumViews:F2} us per view)" );

		double viewsPerSecond = NumViews            * (1.0 / mStopwatch.GetSeconds());
		double chunkletsPerSecond = viewsPerSecond  / 6.0;         // 1 probe x 6 sides
		double chunksPerSecond = chunkletsPerSecond / (4 * 4 * 4); // assumes a 4x4x4 chunk, which will probably be the average

		Console.WriteLine( $"Speed: {viewsPerSecond:F2} views per second" );
		Console.WriteLine( $"       {chunkletsPerSecond:F2} chunklets per second" );
		Console.WriteLine( $"       {chunksPerSecond:F2} chunks per second" );
		Console.WriteLine( "Visibility buffer:" );
		for ( int i = 0; i < 256; i++ )
		{
			if ( visibilitySpan[i] != 0 )
			{
				Console.WriteLine( $"* {i} - VISIBLE" );
			}
		}

		set.Dispose();
		device.Unmap( visibilityReadBuffer );
		visibilityReadBuffer.Dispose();
		visibilityBuffer.Dispose();
		visibilityMap.Dispose();
		bakingInfo.Dispose();
		pipeline.Dispose();
		pipelineDesc.ResourceLayouts[0].Dispose();
		pipelineDesc.ComputeShader.Dispose();
		commandList.Dispose();
		device.Dispose();
	}
}
