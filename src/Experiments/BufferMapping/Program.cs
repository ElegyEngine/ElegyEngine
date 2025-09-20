using System.Numerics;
using Elegy.AppTemplate;
using Elegy.AssetSystem.API;
using Elegy.Common.Assets;
using Elegy.Common.Interfaces;
using Elegy.Common.Maths;
using Elegy.Common.Utilities;
using Elegy.InputSystem.API;
using Elegy.PlatformSystem.API;
using Elegy.RenderSystem.API;
using Elegy.RenderSystem.Interfaces;
using Elegy.RenderSystem.Objects;
using Elegy.RenderSystem.Resources;
using Silk.NET.Input;
using Silk.NET.Windowing;
using Veldrid;
using Material = Elegy.AssetSystem.Resources.Material;

namespace BufferMapping;

static class Program
{
	public class BufferMappingApp : IApplication
	{
		private TaggedLogger mLogger = new( "App" );

		private CommandList mRenderCommands;
		private Material? mMaterial;
		private MeshEntity mMeshEntity;
		private RenderTexture mStagingTexture;
		private RenderTexture mTexture;
		private MappedResource mMappedTexture;
		private View? mView;

		private Model mModel = new()
		{
			Name = "__testmodel",
			Meshes =
			[
				new()
				{
					Name = "__testmodel.mesh",
					Positions = [Vector3.Zero, Coords.Right, Coords.Forward + Coords.Right, Coords.Forward],
					Normals = [Coords.Up, Coords.Up, Coords.Up, Coords.Up],
					Uv0 = [Vector2.Zero, Vector2.UnitX, Vector2.One, Vector2.UnitY],
					Indices = [0, 1, 2, 2, 3, 0],
					MaterialName = "materials/builtin/default"
				}
			]
		};

		private RenderMaterial? RenderMaterial => (RenderMaterial?)mMaterial?.RenderMaterial;

		public string Name => "Scratchpad/BufferMapping";
		public bool Initialised => true; // TODO: bookkeep this in the plugin sys

		public bool Start()
		{
			// Some unavoidable boilerplate
			mRenderCommands = Render.Factory.CreateCommandList();
			Render.OnRender += RenderFrame;

			// Set the current render view's position.
			mView = Render.GetCurrentWindowView();
			mView!.Projection = Coords.CreatePerspectiveMatrix( MathF.PI / 2.0f, 16.0f / 9.0f, 0.01f, 100.0f );
			mView!.Transform = Coords.CreateViewMatrixRadians(
				position: Coords.Back * 0.1f + Coords.Right * 0.7f + Coords.Up * 0.4f,
				// 24° to the left, 45° downwards
				angles: Coords.TurnLeft / 15.0f + Coords.TurnDown / 8.0f
			);

			// This is how you load materials
			mMaterial = Assets.LoadMaterial( "materials/builtin/default" );
			if ( mMaterial is null )
			{
				mLogger.Fatal( "Cannot create material, probably missing the shaders!" );
				return false;
			}

			// This is how you create a renderable object.
			// It doesn't automatically render, it just exists.
			mMeshEntity = Render.CreateEntity( false );

			// Setting this will trigger a regeneration of material param pools,
			// in other words the renderer will get acquainted with each parametre
			// on the materials on this mesh.
			mMeshEntity.Mesh = Render.CreateMesh( mModel );

			// Setting this will trigger a GPU buffer update at the start of the frame.
			mMeshEntity.Transform = Coords.CreateWorldMatrixDegrees( Vector3.Zero, Vector3.Zero );

			// This is how you create a texture. We'll use this texture to
			// "hijack" the current rendermaterial's diffuse texture.
			mStagingTexture = Render.CreateTexture(
				new() // Texture info
				{
					Width = 128,
					Height = 128,
					NoSampling = true,
					CpuAccess = true // this lets Vulkan know the texture is meant to be accessed like this
				},
				new byte[128 * 128 * 4] // Pixel data
			);

			mTexture = Render.CreateTexture( 
				new()
				{
					Width = 128,
					Height = 128
				},
				new byte[128 * 128 * 4]
			);

			// First we obtain an index via the material param's name. It's worth noting that
			// it needs the material param name (DiffuseMap), not shader param name (uDiffuseTexture).
			int diffuseMapIndex = mMaterial.RenderMaterial!.GetParameterIndex( "DiffuseMap" );

			// This will trigger a partial regeneration, similar to changing the mesh.
			mMaterial.RenderMaterial!.SetTexture( diffuseMapIndex, mTexture );

			// Now that this is all done, we can map the texture and obtain its memory directly.
			mMappedTexture = Render.Device.Map( mStagingTexture.DeviceTexture, MapMode.Write );
			Span<byte> buffer = mMappedTexture.AsBytes();

			// Quick "bleeding sine" I toyed around with in ShaderToy
			ManipulateImage( buffer, 128, 128, ( x, y ) =>
			{
				Vector4 farColour = new( 0.9f, 0.9f, 0.9f, 1.0f );
				Vector4 closeColour = new( 1.0f, 0.2f, 0.1f, 1.0f );
				float s = MathF.Sin( x / 12.0f );
				float v = MathF.Abs( s - (y / 30.0f) + 1.9f );
				float blend = 1.0f - float.Pow( float.Clamp( v, 0.0f, 1.0f ), 0.16f );

				return closeColour * blend + farColour * (1.0f - blend);
			} );

			// Quickly copy the texture contents over to there
			mRenderCommands.Begin();
			mStagingTexture.BlitTo( mRenderCommands, mTexture );
			mRenderCommands.End();
			Render.Device.SubmitCommands( mRenderCommands );

			return true;
		}

		private void ManipulateImage( Span<byte> buffer, int width, int height, Func<int, int, Vector4> transform )
		{
			for ( int y = 0; y < height; y++ )
			{
				for ( int x = 0; x < width; x++ )
				{
					Vector4 colour = transform( x, y );
					buffer[y*width*4 + x*4 + 3] = (byte)(colour.X * 255.0f);
					buffer[y*width*4 + x*4 + 2] = (byte)(colour.Y * 255.0f);
					buffer[y*width*4 + x*4 + 1] = (byte)(colour.Z * 255.0f);
					buffer[y*width*4 + x*4 + 0] = (byte)(colour.W * 255.0f);
				}
			}
		}

		public void Shutdown()
		{
			Render.OnRender -= RenderFrame;
		}

		private void RenderFrame()
		{
			View? GetView()
			{
				var window = Platform.GetCurrentWindow();
				if ( window is null )
				{
					return null;
				}

				View? currentView = Render.GetView( window );
				if ( currentView is null )
				{
					return null;
				}

				if ( Render.RenderStyle is null )
				{
					return null;
				}

				return currentView;
			}

			if ( GetView() is not { } view || RenderMaterial is null )
			{
				return;
			}

			// Corrupt the image, one pixel at a time...
			for ( int i = 0; i < 128; i++ )
			{
				int randomIndex = Random.Shared.Next( 0, 128 * 128 * 4 );
				mMappedTexture.AsBytes()[randomIndex] += 8;
			}

			// Quickly copy the texture contents over to there
			mRenderCommands.Begin();
			mStagingTexture.BlitTo( mRenderCommands, mTexture );
			mRenderCommands.End();
			Render.Device.SubmitCommands( mRenderCommands );

			// Update all buffers at the start of the frame pretty please
			Render.UpdateBuffers();

			// Begin rendering
			mRenderCommands.Begin();
			Render.SetRenderView( mRenderCommands, view );

			// Opaque pass
			RenderSurface surface = new()
			{
				Mesh = mMeshEntity.Mesh.Submeshes[0],
				ParameterPool = mMeshEntity.Mesh.Materials[0].ParameterPool,
				PerEntitySet = mMeshEntity.PerEntitySet
			};
			Render.RenderStyle!.RenderSurfaces( mRenderCommands, view, [surface], RenderMaterial, [] );

			mRenderCommands.End();
			Render.Device.SubmitCommands( mRenderCommands );
		}

		public bool RunFrame( float delta )
		{
			if ( Input.Keyboard.IsKeyPressed( Key.Escape ) )
			{
				return false;
			}

			return true;
		}
	}

	// NOTE: Set the working directory to testgame/
	static void Main( string[] args )
	{
		Console.Title = "Scratchpad/BufferMapping";
		Window.PrioritizeSdl();

		Application.Start<BufferMappingApp>(
			config:
			new()
			{
				Args = args,
				Engine = EngineConfig.Game( "game" ),
				WithMainWindow = true,
				ToolMode = true
			},
			windowPlatform:
			Window.GetWindowPlatform( viewOnly: false )
			?? throw new Exception( "SDL2 not found" )
		);
	}
}
