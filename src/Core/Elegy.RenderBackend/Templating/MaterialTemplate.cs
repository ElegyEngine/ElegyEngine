// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.RenderBackend.Extensions;
using System.Text;
using Veldrid;

namespace Elegy.RenderBackend.Templating
{
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

		public ResourceLayout[] ResourceLayouts { get; private set; }

		public Dictionary<string, ShaderVariant> ShaderVariants { get; } = new();

		public bool HasVariant( string name )
			=> ShaderVariants.ContainsKey( name );

		public Pipeline GetVariantPipeline( string name )
			=> ShaderVariants[name].Pipeline;

		public ShaderVariant GetVariant( int id )
			=> ShaderVariants.ElementAt( id ).Value;

		public bool ValidateDataExists( Func<string, string?> pathTo, StringBuilder errorStrings )
		{
			bool okay = true;

			foreach ( var variant in ShaderTemplate.ShaderVariants )
			{
				string pathToShaderVariant = Utils.PathToShaderVariant( ShaderTemplate, variant );
				string? shaderBasePath = pathTo( pathToShaderVariant );
				if ( shaderBasePath is null )
				{
					errorStrings.AppendLine( $"* {pathToShaderVariant}" );
					okay = false;
				}
			}

			return okay;
		}

		public bool CompileResources( GraphicsDevice gd,
			Func<Assets.ShaderVariantEntry, bool, OutputDescription> outputDescriptionFunc,
			Func<string, string?> pathTo )
		{
			ResourceFactory factory = gd.ResourceFactory;

			// 1. Create resource layouts
			ResourceLayouts = ShaderTemplate.ParameterSets.Select( layout => factory.CreateLayout( layout.Parameters ) ).ToArray();

			foreach ( var variant in ShaderTemplate.ShaderVariants )
			{
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
							Assets.VertexSemantic.Uv => numUvChannels++,
							Assets.VertexSemantic.Colour => numColourChannels++,
							_ => 0
						},
						Id = (uint)variant.VertexLayouts[i].Id
					};
				}

				// 3. Create resource mapping table
				List<int> globalMappings = new( ResourceLayouts.Length );
				List<int> perMaterialMappings = new( ResourceLayouts.Length );
				List<int> perInstanceMappings = new( ResourceLayouts.Length );
				for ( int i = 0; i < ResourceLayouts.Length; i++ )
				{
					var layout = ShaderTemplate.ParameterSets[i];

					// Do not create mappings for parameters that aren't visible to this shader variant
					if ( !variant.ParameterSetIds.Contains( layout.ResourceSetId ) )
					{
						continue;
					}

					// Builtin params are set manually while rendering
					// Instance params have resource sets generated elsewhere
					if ( layout.Level == Assets.MaterialParameterLevel.Builtin )
					{
						continue;
					}

					var mappingList = layout.Level switch
					{
						Assets.MaterialParameterLevel.Data => perMaterialMappings,
						Assets.MaterialParameterLevel.Global => globalMappings,
						Assets.MaterialParameterLevel.Instance => perInstanceMappings
					};

					mappingList.Add( layout.ResourceSetId );
				}

				// 4. Create shader objects
				string shaderBasePath = pathTo( Utils.PathToShaderVariant( ShaderTemplate, variant ) );
				// shaderBasePath is guaranteed to exist, it is validated upfront
				Shader vertexShader = factory.LoadShaderDirect( shaderBasePath, ShaderStages.Vertex );
				Shader pixelShader = factory.LoadShaderDirect( shaderBasePath, ShaderStages.Fragment );

				// 5. Create pipelines
				ResourceLayout[] layouts = variant.ParameterSetIds.Select( id => ResourceLayouts[id] ).ToArray();

				Pipeline pipeline = factory.CreatePipeline( Data, variant, vertexShader, pixelShader, layouts,
					outputDescriptionFunc( variant, ShaderTemplate.PostprocessHint ) );

				ShaderVariants.Add( variant.ShaderDefine,
					new( this, variant, layouts, vertexShader, pixelShader, pipeline, attributes,
						perMaterialMappings.ToArray(), perInstanceMappings.ToArray(), globalMappings.ToArray() ) );
			}

			return true;
		}
	}
}
