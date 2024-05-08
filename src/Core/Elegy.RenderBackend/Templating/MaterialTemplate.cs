// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.RenderBackend.Assets;
using Elegy.RenderBackend.Extensions;
using Veldrid;

namespace Elegy.RenderBackend.Templating
{
	public struct VariantVertexAttribute
	{
		public VertexSemantic Semantic;
		public int Channel;
	}

	public struct VariantResourceMapping
	{
		public int LayoutId;
		public int SetId;
	}

	public class ShaderVariant
	{
		public ShaderVariant( MaterialTemplate parent, ResourceLayout[] layouts,
			Shader vertexShader, Shader pixelShader, Pipeline pipeline,
			VariantVertexAttribute[] attributes, VariantResourceMapping[] mappings )
		{
			Template = parent;
			Layouts = layouts;
			VertexShader = vertexShader;
			PixelShader = pixelShader;
			Pipeline = pipeline;
			VertexAttributes = attributes;
			ResourceMappings = mappings;
		}

		public VariantResourceMapping[] ResourceMappings { get; }

		public VariantVertexAttribute[] VertexAttributes { get; }

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
			// ShaderVariants is filled in in CompileResources
		}

		public Assets.MaterialTemplate Data { get; }

		public Assets.ShaderTemplate ShaderTemplate { get; }

		public Dictionary<string, ShaderVariant> ShaderVariants { get; } = new();

		public bool HasVariant( string name )
			=> ShaderVariants.ContainsKey( name );

		public Pipeline GetVariantPipeline( string name )
			=> ShaderVariants[name].Pipeline;

		public bool CompileResources( GraphicsDevice gd,
			Func<ShaderTemplateEntry, bool, OutputDescription> outputDescriptionFunc,
			int[] skippedSetIds,
			Func<string, string?>? pathTo = null )
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

				// 2. Create vertex attribute metadata for linking
				VariantVertexAttribute[] attributes = new VariantVertexAttribute[variant.VertexLayouts.Count];
				int numUvChannels = 0;
				int numColourChannels = 0;
				for ( int i = 0; i < attributes.Length; i++ )
				{
					var semantic = Utils.GetVertexSemantic( variant.VertexLayouts[i].Name );
					attributes[i] = new()
					{
						Semantic = semantic,
						Channel = semantic switch
						{
							VertexSemantic.Uv => numUvChannels++,
							VertexSemantic.Colour => numColourChannels++,
							_ => 0
						}
					};
				}

				// 3. Create resource mapping table
				List<VariantResourceMapping> mappings = new( variant.ResourceLayouts.Count );
				for ( int i = 0; i < variant.ResourceLayouts.Count; i++ )
				{
					var layout = variant.ResourceLayouts[i];
					if ( skippedSetIds.Contains( layout.Set ) )
					{
						continue;
					}

					mappings.Add( new()
					{
						LayoutId = i,
						SetId = layout.Set
					} );
				}

				// 4. Create shader objects
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

				// 5. Create pipelines
				Pipeline pipeline = factory.CreatePipeline( Data, variant, vertexShader, pixelShader, layouts,
					outputDescriptionFunc( variant, ShaderTemplate.PostprocessHint ) );

				ShaderVariants.Add( variant.ShaderDefine, new( this, layouts, vertexShader, pixelShader, pipeline, attributes, mappings.ToArray() ) );
			}

			return true;
		}
	}
}
