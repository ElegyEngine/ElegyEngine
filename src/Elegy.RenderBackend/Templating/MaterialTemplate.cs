// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.RenderBackend.Extensions;
using Veldrid;

namespace Elegy.RenderBackend.Templating
{
	public class ShaderVariant
	{
		public ShaderVariant( MaterialTemplate parent, ResourceLayout[] layouts, Shader vertexShader, Shader pixelShader, Pipeline pipeline )
		{
			Template = parent;
			Layouts = layouts;
			VertexShader = vertexShader;
			PixelShader = pixelShader;
			Pipeline = pipeline;
		}

		public MaterialTemplate Template { get; }

		public ResourceLayout[] Layouts { get; }

		public Shader VertexShader { get; }

		public Shader PixelShader { get; }

		public Pipeline Pipeline { get; }
	}

	public class MaterialTemplate
	{
		public MaterialTemplate( Assets.MaterialTemplate data, Assets.ShaderTemplate shaderTemplate )
		{
			Data = data;
			ShaderTemplate = shaderTemplate;
		}

		public Assets.MaterialTemplate Data { get; }

		public Assets.ShaderTemplate ShaderTemplate { get; }

		public Dictionary<string, ShaderVariant> ShaderVariants { get; } = new();

		public bool CompileResources( GraphicsDevice gd, Framebuffer outputFramebuffer, Func<string, string?>? pathTo = null )
		{
			ResourceFactory factory = gd.ResourceFactory;

			foreach ( var variant in ShaderTemplate.ShaderVariants )
			{
				// 1. Create resource layouts
				ResourceLayout[] layouts = new ResourceLayout[variant.ResourceLayouts.Count];
				for ( int i = 0; i < layouts.Length; i++ )
				{
					layouts[i] = factory.CreateLayout( variant.ResourceLayouts[i] );
				}

				// 2. Create shader objects
				string? shaderBasePath = Utils.PathToShaderVariant( ShaderTemplate, variant );
				if ( pathTo is not null )
				{
					shaderBasePath = pathTo( shaderBasePath );
					if ( shaderBasePath is null )
					{
						throw new FileNotFoundException();
					}
				}
				Shader vertexShader = factory.LoadShaderDirect( shaderBasePath, ShaderStages.Vertex );
				Shader pixelShader = factory.LoadShaderDirect( shaderBasePath, ShaderStages.Fragment );

				// 3. Create pipelines
				Pipeline pipeline = factory.CreatePipeline( Data, variant, vertexShader, pixelShader, layouts, outputFramebuffer );

				ShaderVariants.Add( variant.ShaderDefine, new( this, layouts, vertexShader, pixelShader, pipeline ) );
			}

			return true;
		}
	}
}
