// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Collections.Pooled;
using Elegy.AssetSystem.API;
using Elegy.ConsoleSystem;
using Elegy.FileSystem.API;
using Elegy.RenderBackend.Extensions;
using Elegy.RenderSystem.Objects;
using Elegy.RenderSystem.Resources;
using System.Text;
using Veldrid;

namespace Elegy.RenderSystem.API
{
	public static partial class Render
	{
		private static TaggedLogger mLogger = new( "Render" );

		// TODO: dynamic capacity configuration
		private static PooledSet<MeshEntity> mEntitySet = new( 1024 );
		private static PooledSet<Mesh> mMeshSet = new( 1024 );
		private static PooledSet<RenderMaterial> mMaterialSet = new( 2048 );
		private static PooledSet<RenderTexture> mTextureSet = new( 4096 );
		private static PooledSet<View> mViews = new( 32 );

		private static RenderMaterial mWindowMaterial;

		private static CommandList mRenderCommands;
		private static GraphicsDevice? mDevice = null;

		private static bool InitialiseGraphicsDevice()
		{
			// RenderStyle is guaranteed to be non-null here, you can ignore what Intelli(Non)Sense says
			mDevice = CreateGraphicsDevice( RenderStyle.InstanceExtensions, RenderStyle.DeviceExtensions );
			return mDevice is not null;
		}

		private static Sampler CreateSampler( SamplerFilter filter, SamplerAddressMode addressMode, uint anisoSamples = 0U )
		{
			SamplerDescription samplerDescription = new()
			{
				AddressModeU = addressMode,
				AddressModeV = addressMode,
				AddressModeW = addressMode,
				MaximumAnisotropy = anisoSamples,
				BorderColor = SamplerBorderColor.TransparentBlack,
				Filter = filter,
				LodBias = 0,
				MinimumLod = 0,
				MaximumLod = uint.MaxValue
			};

			return Factory.CreateSampler( samplerDescription );
		}

		private static OutputDescription GetOutputForShaderVariant( RenderBackend.Assets.ShaderVariantEntry entry, bool postprocessHint )
		{
			if ( entry.ShaderDefine.ToLower().Contains( "depth" ) )
			{
				return OutputDescDepthOnly;
			}

			return postprocessHint ? OutputDescBackbuffer : OutputDescNormal;
		}

		private static string? FindShaderBinaryPath( string path )
		{
			string? result = Files.PathTo( $"{path}.ps.spv", PathFlags.File );
			if ( result is not null )
			{
				return result.Replace( ".ps.spv", null );
			}

			result = Files.PathTo( $"{path}.cs.spv", PathFlags.File );
			if ( result is not null )
			{
				return result.Replace( ".cs.spv", null );
			}

			return null;
		}

		private static bool InitialiseGraphics()
		{
			if ( !InitialiseGraphicsDevice() )
			{
				mLogger.Error( "Failed to create graphics device" );
				return false;
			}

			bool allOkay = true;
			StringBuilder errorStrings = new();
			foreach ( var template in MaterialTemplates )
			{
				if ( !template.ValidateDataExists( FindShaderBinaryPath, errorStrings ) )
				{
					mLogger.Error( "One or more shaders are missing:" );
					mLogger.Error( errorStrings.ToString() );
				}
			}

			foreach ( var template in MaterialTemplates )
			{
				if ( !template.CompileResources( mDevice, GetOutputForShaderVariant, FindShaderBinaryPath ) )
				{
					mLogger.Error( $"Failed to compile pipeline for material template '{template.Data.Name}'" );
					allOkay = false;
				}
			}

			if ( !allOkay )
			{
				return false;
			}

			mRenderCommands = Factory.CreateCommandList();

			var windowMaterial = LoadMaterial( "materials/builtin/window_default" );
			if ( windowMaterial is null )
			{
				mLogger.Error( "Cannot load the default view material" );
				return false;
			}

			mWindowMaterial = windowMaterial;

			Samplers.Nearest = CreateSampler( SamplerFilter.MinPoint_MagPoint_MipLinear, SamplerAddressMode.Wrap );
			Samplers.Linear = CreateSampler( SamplerFilter.MinLinear_MagLinear_MipLinear, SamplerAddressMode.Wrap );
			Samplers.Aniso2x = CreateSampler( SamplerFilter.Anisotropic, SamplerAddressMode.Wrap, 2 );
			Samplers.Aniso4x = CreateSampler( SamplerFilter.Anisotropic, SamplerAddressMode.Wrap, 4 );
			Samplers.Aniso8x = CreateSampler( SamplerFilter.Anisotropic, SamplerAddressMode.Wrap, 8 );
			Samplers.Aniso16x = CreateSampler( SamplerFilter.Anisotropic, SamplerAddressMode.Wrap, 16 );

			Samplers.NearestClamp = CreateSampler( SamplerFilter.MinPoint_MagPoint_MipLinear, SamplerAddressMode.Clamp );
			Samplers.LinearClamp = CreateSampler( SamplerFilter.MinLinear_MagLinear_MipLinear, SamplerAddressMode.Clamp );
			Samplers.Aniso2xClamp = CreateSampler( SamplerFilter.Anisotropic, SamplerAddressMode.Clamp, 2 );
			Samplers.Aniso4xClamp = CreateSampler( SamplerFilter.Anisotropic, SamplerAddressMode.Clamp, 4 );
			Samplers.Aniso8xClamp = CreateSampler( SamplerFilter.Anisotropic, SamplerAddressMode.Clamp, 8 );
			Samplers.Aniso16xClamp = CreateSampler( SamplerFilter.Anisotropic, SamplerAddressMode.Clamp, 16 );

			Samplers.NearestClamp = CreateSampler( SamplerFilter.MinPoint_MagPoint_MipLinear, SamplerAddressMode.Border );
			Samplers.LinearClamp = CreateSampler( SamplerFilter.MinLinear_MagLinear_MipLinear, SamplerAddressMode.Border );
			Samplers.Aniso2xClamp = CreateSampler( SamplerFilter.Anisotropic, SamplerAddressMode.Border, 2 );
			Samplers.Aniso4xClamp = CreateSampler( SamplerFilter.Anisotropic, SamplerAddressMode.Border, 4 );
			Samplers.Aniso8xClamp = CreateSampler( SamplerFilter.Anisotropic, SamplerAddressMode.Border, 8 );
			Samplers.Aniso16xClamp = CreateSampler( SamplerFilter.Anisotropic, SamplerAddressMode.Border, 16 );

			Layouts.Window = Factory.CreateLayout(
				new ResourceLayoutElementDescription( "ViewTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment ),
				new ResourceLayoutElementDescription( "ViewSampler", ResourceKind.Sampler, ShaderStages.Fragment )
			);

			Layouts.PerView = Factory.CreateLayout(
				new ResourceLayoutElementDescription( "uView", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment )
			);

			Layouts.PerEntity = Factory.CreateLayout(
				new ResourceLayoutElementDescription( "uEntity", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment )
			);

			Meshes.FullscreenQuad = CreateMesh( new()
			{
				Name = "Fullscreen quad",
				FullPath = "__fullscreen_quad",
				Meshes =
				[
					new()
					{
						Name = "Fullscreen Quad Mesh",
						MaterialName = "materials/builtin/window_default",
						Positions2D =
						[
							new( -1.0f, -1.0f ),
							new( -1.0f, 1.0f ),
							new( 1.0f, 1.0f ),
							new( 1.0f, -1.0f )
						],
						Uv0 =
						[
							new( 0.0f, 0.0f ),
							new( 0.0f, 1.0f ),
							new( 1.0f, 1.0f ),
							new( 1.0f, 0.0f )
						],
						Indices =
						[
							0, 1, 2,
							0, 2, 3
						]
					}
				]
			} );

			return true;
		}
	}
}
