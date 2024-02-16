// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System;
using System.Runtime.InteropServices;
using Veldrid;

namespace Elegy.RenderBackend.Extensions
{
	public static class FactoryExtensions
	{
		private static uint StrideOf<T>( BufferUsage usage )
			where T : unmanaged
		{
			if ( usage.HasFlag( BufferUsage.StructuredBufferReadOnly | BufferUsage.StructuredBufferReadWrite) )
			{
				return (uint)Marshal.SizeOf<T>();
			}

			return 0U;
		}

		private static uint NearestSize16<T>( int numElements )
			where T : unmanaged
		{
			uint bytes = (uint)(numElements * Marshal.SizeOf<T>());

			return bytes + (bytes % 16);
		}

		public static DeviceBuffer CreateBufferForList<T>( this ResourceFactory factory, BufferUsage usage, IList<T> buffer )
			where T : unmanaged
		{
			BufferDescription desc = new()
			{
				Usage = usage,
				SizeInBytes = NearestSize16<T>( buffer.Count ),
				StructureByteStride = StrideOf<T>( usage )
			};

			return factory.CreateBuffer( desc );
		}

		public static DeviceBuffer CreateBufferForSpan<T>( this ResourceFactory factory, BufferUsage usage, ReadOnlySpan<T> span )
			where T : unmanaged
		{
			BufferDescription desc = new()
			{
				Usage = usage,
				SizeInBytes = NearestSize16<T>( span.Length ),
				StructureByteStride = StrideOf<T>( usage )
			};

			return factory.CreateBuffer( desc );
		}

		public static DeviceBuffer CreateBufferForStruct<T>( this ResourceFactory factory, BufferUsage usage )
			where T : unmanaged
		{
			BufferDescription desc = new()
			{
				Usage = usage,
				SizeInBytes = NearestSize16<T>( 1 ),
				StructureByteStride = StrideOf<T>( usage )
			};

			return factory.CreateBuffer( desc );
		}

		public static (Shader vertexShader, Shader pixelShader) LoadShaders( this ResourceFactory mFactory, string filePrefix )
		{
			ShaderDescription vertexShaderDesc = new()
			{
				EntryPoint = "main_vs",
				ShaderBytes = Utils.LoadShaderBytes( $"{filePrefix}.vs.spv" ),
				Stage = ShaderStages.Vertex
			};
			Shader vertexShader = mFactory.CreateShader( vertexShaderDesc );

			ShaderDescription pixelShaderDesc = new()
			{
				EntryPoint = "main_ps",
				ShaderBytes = Utils.LoadShaderBytes( $"{filePrefix}.ps.spv" ),
				Stage = ShaderStages.Fragment
			};
			Shader pixelShader = mFactory.CreateShader( pixelShaderDesc );

			return (vertexShader, pixelShader);
		}

		public static Shader LoadComputeShader( this ResourceFactory mFactory, string filePrefix )
		{
			ShaderDescription pixelShaderDesc = new()
			{
				EntryPoint = "main_cs",
				ShaderBytes = Utils.LoadShaderBytes( $"{filePrefix}.cs.spv" ),
				Stage = ShaderStages.Fragment
			};

			return mFactory.CreateShader( pixelShaderDesc );
		}

		public static RenderPipeline CreateGraphicsPipeline<TVertex>( this ResourceFactory mFactory, string shaderPath, RasterPreset preset,
			Framebuffer targetFramebuffer, params ResourceLayout[] resourceLayouts ) where TVertex : struct
		{
			(Shader vertexShader, Shader pixelShader) = mFactory.LoadShaders( shaderPath );

			GraphicsPipelineDescription pipelineDesc = new()
			{
				PrimitiveTopology = PrimitiveTopology.TriangleList,
				BlendState = BlendStateDescription.SingleOverrideBlend,
				ResourceBindingModel = ResourceBindingModel.Improved,

				ShaderSet =
				{
					Shaders = [ vertexShader, pixelShader ],
					VertexLayouts = Utils.GenerateVertexLayoutFor<TVertex>()
				},

				RasterizerState = new()
				{
					CullMode = preset switch
					{
						RasterPreset.OpaqueTwoSided or
						RasterPreset.NoDepthTwoSided => FaceCullMode.None,

						RasterPreset.OpaqueReverse or
						RasterPreset.NoDepthReverse => FaceCullMode.Front,

						_ => FaceCullMode.Back,
					},
					FillMode = PolygonFillMode.Solid,
					FrontFace = FrontFace.CounterClockwise,
					DepthClipEnabled = true,
					ScissorTestEnabled = false
				},

				DepthStencilState = new()
				{
					DepthTestEnabled = preset < RasterPreset.NoDepth,
					DepthWriteEnabled = preset < RasterPreset.NoDepth,
					StencilTestEnabled = false,
					DepthComparison = ComparisonKind.LessEqual
				},

				ResourceLayouts = resourceLayouts,
				Outputs = targetFramebuffer.OutputDescription
			};

			return new RenderPipeline( mFactory.CreateGraphicsPipeline( pipelineDesc ), pipelineDesc.ShaderSet.Shaders, preset, resourceLayouts );
		}

		public static RenderPipeline CreateGraphicsPipeline<TVertex>( this ResourceFactory mFactory, string shaderPath, RasterPreset preset,
			OutputDescription outputDescription, params ResourceLayout[] resourceLayouts ) where TVertex : struct
		{
			(Shader vertexShader, Shader pixelShader) = mFactory.LoadShaders( shaderPath );

			GraphicsPipelineDescription pipelineDesc = new()
			{
				PrimitiveTopology = PrimitiveTopology.TriangleList,
				BlendState = BlendStateDescription.SingleOverrideBlend,
				ResourceBindingModel = ResourceBindingModel.Improved,

				ShaderSet =
				{
					Shaders = [ vertexShader, pixelShader ],
					VertexLayouts = Utils.GenerateVertexLayoutFor<TVertex>()
				},

				RasterizerState = new()
				{
					CullMode = preset switch
					{
						RasterPreset.OpaqueTwoSided or
						RasterPreset.NoDepthTwoSided => FaceCullMode.None,

						RasterPreset.OpaqueReverse or
						RasterPreset.NoDepthReverse => FaceCullMode.Front,

						_ => FaceCullMode.Back,
					},
					FillMode = PolygonFillMode.Solid,
					FrontFace = FrontFace.CounterClockwise,
					DepthClipEnabled = true,
					ScissorTestEnabled = false
				},

				DepthStencilState = new()
				{
					DepthTestEnabled = preset < RasterPreset.NoDepth,
					DepthWriteEnabled = preset < RasterPreset.NoDepth,
					StencilTestEnabled = false,
					DepthComparison = ComparisonKind.LessEqual
				},

				ResourceLayouts = resourceLayouts,
				Outputs = outputDescription
			};

			return new RenderPipeline( mFactory.CreateGraphicsPipeline( pipelineDesc ), pipelineDesc.ShaderSet.Shaders, preset, resourceLayouts );
		}

		public static ResourceLayout CreateLayout( this ResourceFactory mFactory, params ResourceLayoutElementDescription[] elements )
		{
			ResourceLayoutDescription layoutDesc = new()
			{
				Elements = elements
			};
			return mFactory.CreateResourceLayout( layoutDesc );
		}

		public static ResourceSet CreateSet( this ResourceFactory mFactory, ResourceLayout layout, params BindableResource[] resources )
		{
			ResourceSetDescription setDesc = new()
			{
				Layout = layout,
				BoundResources = resources
			};
			return mFactory.CreateResourceSet( setDesc );
		}
	}
}
