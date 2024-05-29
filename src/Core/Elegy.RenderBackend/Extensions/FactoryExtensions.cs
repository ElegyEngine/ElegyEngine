// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.RenderBackend.Assets; 
using Veldrid;

namespace Elegy.RenderBackend.Extensions
{
	public static class FactoryExtensions
	{
		public static DeviceBuffer CreateBufferForList<T>( this ResourceFactory factory, BufferUsage usage, IList<T> buffer )
			where T : unmanaged
		{
			BufferDescription desc = new()
			{
				Usage = usage,
				SizeInBytes = Utils.NearestSize16<T>( buffer.Count ),
				StructureByteStride = Utils.StrideOf<T>( usage )
			};

			return factory.CreateBuffer( desc );
		}

		public static DeviceBuffer CreateBufferForSpan<T>( this ResourceFactory factory, BufferUsage usage, ReadOnlySpan<T> span )
			where T : unmanaged
		{
			BufferDescription desc = new()
			{
				Usage = usage,
				SizeInBytes = Utils.NearestSize16<T>( span.Length ),
				StructureByteStride = Utils.StrideOf<T>( usage )
			};

			return factory.CreateBuffer( desc );
		}

		public static DeviceBuffer CreateBufferForStruct<T>( this ResourceFactory factory, BufferUsage usage )
			where T : unmanaged
		{
			BufferDescription desc = new()
			{
				Usage = usage,
				SizeInBytes = Utils.NearestSize16<T>( 1 ),
				StructureByteStride = Utils.StrideOf<T>( usage )
			};

			return factory.CreateBuffer( desc );
		}

		public static Shader LoadShaderDirect( this ResourceFactory factory, string path, ShaderStages stage )
		{
			string stageInfix = stage switch
			{
				ShaderStages.Vertex => "vs",
				ShaderStages.Fragment => "ps",
				_ => "cs"
			};

			return factory.CreateShader( new()
			{
				EntryPoint = $"main_{stageInfix}",
				ShaderBytes = Utils.LoadShaderBytes( $"{path}.{stageInfix}.spv" ),
				Stage = stage
			} ); ;
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

		public static Pipeline CreatePipeline( this ResourceFactory factory, MaterialTemplate materialTemplate, ShaderVariantEntry shaderTemplateEntry,
			Shader vertexShader, Shader pixelShader, ResourceLayout[] layouts, Framebuffer outputFramebuffer )
			=> factory.CreatePipeline( materialTemplate, shaderTemplateEntry, vertexShader, pixelShader, layouts, outputFramebuffer.OutputDescription );

		public static Pipeline CreatePipeline( this ResourceFactory factory, MaterialTemplate materialTemplate, ShaderVariantEntry shaderTemplateEntry,
			Shader vertexShader, Shader pixelShader, ResourceLayout[] layouts, in OutputDescription outputFramebufferDesc )
		{
			GraphicsPipelineDescription pipelineDesc = new()
			{
				PrimitiveTopology = PrimitiveTopology.TriangleList,
				BlendState = Utils.ExtractBlendState( materialTemplate ),
				ResourceBindingModel = ResourceBindingModel.Improved,

				ShaderSet =
				{
					Shaders = [ vertexShader, pixelShader ],
					VertexLayouts = Utils.ExtractVertexLayouts( shaderTemplateEntry )
				},

				RasterizerState = Utils.ExtractRasterizerState( materialTemplate ),
				DepthStencilState = Utils.ExtractDepthStencilState( materialTemplate ),

				ResourceLayouts = layouts,
				Outputs = outputFramebufferDesc
			};

			return factory.CreateGraphicsPipeline( pipelineDesc );
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

		public static ResourceLayout CreateLayout( this ResourceFactory mFactory, Assets.ResourceLayoutEntry entry )
		{
			ResourceLayoutElementDescription[] elementDescs = new ResourceLayoutElementDescription[entry.Elements.Count];
			int i = 0;
			foreach ( var element in entry.Elements )
			{
				elementDescs[i] = new()
				{
					Name = element.Name,
					Stages = ShaderStages.Vertex | ShaderStages.Fragment,
					Kind = Utils.TypeToResourceKind( element.Type )
				};

				i++;
			}

			return mFactory.CreateResourceLayout( new() { Elements = elementDescs } );
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
