// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Collections.Pooled;
using Elegy.ConsoleSystem;
using Elegy.FileSystem.API;
using Elegy.RenderBackend.Extensions;
using Elegy.RenderSystem.Objects;
using Elegy.RenderSystem.Resources;
using System.Text;
using Elegy.RenderBackend;
using Veldrid;

namespace Elegy.RenderSystem.API
{
	public static partial class Render
	{
		private static TaggedLogger mLogger = new( "Render" );

		// TODO: dynamic capacity configuration
		private static MeshEntitySystem mEntitySystem = new( 1024 );
		private static PooledSet<Mesh> mMeshSet = new( 1024 );
		private static PooledSet<RenderMaterial> mMaterialSet = new( 2048 );
		private static PooledSet<RenderTexture> mTextureSet = new( 4096 );
		private static PooledSet<View> mViews = new( 32 );

		private static RenderMaterial mWindowMaterial;
		private static CommandList mRenderCommands;
		private static GraphicsDevice mDevice;

		private static bool InitialiseGraphicsDevice()
		{
			var device = CreateGraphicsDevice( RenderStyle!.InstanceExtensions, RenderStyle.DeviceExtensions );
			if ( device is null )
			{
				return false;
			}

			mDevice = device;
			return true;
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

		private static void InitialiseBuiltinMeshes()
		{
			InitialiseDebugMeshes();

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
		}

		private static void InitialiseGraphicsConstants()
		{
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

			Samplers.NearestBorder = CreateSampler( SamplerFilter.MinPoint_MagPoint_MipLinear, SamplerAddressMode.Border );
			Samplers.LinearBorder = CreateSampler( SamplerFilter.MinLinear_MagLinear_MipLinear, SamplerAddressMode.Border );
			Samplers.Aniso2xBorder = CreateSampler( SamplerFilter.Anisotropic, SamplerAddressMode.Border, 2 );
			Samplers.Aniso4xBorder = CreateSampler( SamplerFilter.Anisotropic, SamplerAddressMode.Border, 4 );
			Samplers.Aniso8xBorder = CreateSampler( SamplerFilter.Anisotropic, SamplerAddressMode.Border, 8 );
			Samplers.Aniso16xBorder = CreateSampler( SamplerFilter.Anisotropic, SamplerAddressMode.Border, 16 );

			ShaderStages commonShaderStages = ShaderStages.Vertex | ShaderStages.Fragment;

			Layouts.Window = Factory.CreateLayout(
				new ResourceLayoutElementDescription( "ViewTexture", ResourceKind.TextureReadOnly, commonShaderStages ),
				new ResourceLayoutElementDescription( "ViewSampler", ResourceKind.Sampler, commonShaderStages )
			);

			Layouts.PerView = Factory.CreateLayout(
				new ResourceLayoutElementDescription( "uView", ResourceKind.UniformBuffer, commonShaderStages )
			);

			Layouts.PerEntity = Factory.CreateLayout(
				new ResourceLayoutElementDescription( "uEntity", ResourceKind.UniformBuffer, commonShaderStages )
			);
		}

		private static bool InitialiseGraphics()
		{
			bool allOkay = true;
			StringBuilder errorStrings = new();
			foreach ( var template in MaterialTemplates )
			{
				if ( !template.ValidateDataExists( FindShaderBinaryPath, errorStrings ) )
				{
					allOkay = false;
					mLogger.Error( $"Skipping material template '{template.Data.Name}', missing shaders (read below)" );
					continue;
				}

				if ( !template.CompileResources( mDevice, GetOutputForShaderVariant, FindShaderBinaryPath ) )
				{
					mLogger.Error( $"Failed to compile pipeline for material template '{template.Data.Name}'" );
					allOkay = false;
				}
			}

			if ( !allOkay )
			{
				if ( errorStrings.Length > 0 )
				{
					mLogger.Error( "One or more shaders are missing:" );
					mLogger.Error( errorStrings.ToString() );
					mLogger.Error(
						"Look at 'shaders/bin/'. Chances are some shader stages are missing because they failed to compile or they haven't been compiled." );
				}

				return false;
			}

			mRenderCommands = Factory.CreateCommandList();
			mWindowMaterial = LoadMaterial( "materials/builtin/window_default" );
			mDebugLineMaterial = LoadMaterial( "materials/builtin/debug_line" );

			return true;
		}
	}
}
