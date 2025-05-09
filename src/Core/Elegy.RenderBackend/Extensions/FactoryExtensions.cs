// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.RenderBackend.Assets; 
using Veldrid;

namespace Elegy.RenderBackend.Extensions
{
	public static class FactoryExtensions
	{
		public static DeviceBuffer CreateBufferForSpan<T>( this ResourceFactory factory, BufferUsage usage, ReadOnlySpan<T> span )
			where T : unmanaged
		{
			BufferDescription desc = new()
			{
				Usage = usage,
				SizeInBytes = Utils.NearestSize16<T>( span.Length ),
				StructureByteStride = Utils.StrideIfStructured<T>( usage )
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
				StructureByteStride = Utils.StrideIfStructured<T>( usage )
			};

			return factory.CreateBuffer( desc );
		}

		public static Shader LoadShaderDirect( this ResourceFactory factory, string path, ShaderStages stage, bool debug = false )
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
				Stage = stage,
				Debug = debug
			} );
		}

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

		public static ResourceLayout CreateLayout( this ResourceFactory mFactory, params ResourceLayoutElementDescription[] elements )
		{
			ResourceLayoutDescription layoutDesc = new()
			{
				Elements = elements
			};
			return mFactory.CreateResourceLayout( layoutDesc );
		}

		public static ResourceLayout CreateLayout( this ResourceFactory mFactory, IReadOnlyList<Assets.MaterialParameter> entry )
		{
			ResourceLayoutElementDescription[] elementDescs = new ResourceLayoutElementDescription[entry.Count];
			int i = 0;
			foreach ( var element in entry )
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
