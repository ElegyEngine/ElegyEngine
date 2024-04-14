// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

global using Console = Elegy.ConsoleSystem.API.Console;

using Veldrid;

using Elegy.ConsoleSystem;
using Elegy.RenderSystem.Interfaces;
using Elegy.AssetSystem.API;

namespace Elegy.RenderStandard;

public partial class RenderStandard : IRenderFrontend
{
	private GraphicsDevice Device => mDevice;
	private ResourceFactory Factory => Device.ResourceFactory;

	private GraphicsDevice mDevice;
	private TaggedLogger mLogger = new( "Renderer" );

	private CommandList mRenderCommands;
	private CommandList mBufferCommands;

	public string Name => "Standard Renderer";

	public string Error { get; set; } = string.Empty;

	public bool Initialised { get; set; } = false;

	public bool Init()
	{
		mLogger.Log( "Init" );
		mStopwatch.Restart();

		// TODO: detect headless mode somehow
		if ( false /*Core.IsHeadless*/ )
		{
			mLogger.Error( "The engine is running in headless mode, meaning graphics cannot work!" );
			return false;
		}

		try
		{
			mDevice = GraphicsDevice.CreateVulkan( new()
			{
#if DEBUG
				Debug = true,
#endif
				ResourceBindingModel = ResourceBindingModel.Improved,
				SyncToVerticalBlank = true,

				SwapchainSrgbFormat = false,
				SwapchainDepthFormat = null,

				//PreferDepthRangeZeroToOne = true,
				//PreferStandardClipSpaceYDirection = true,

				// We are gonna create swapchains manually for IViews
				HasMainSwapchain = false
			},
			new VulkanDeviceOptions()
			{
				// Nothing in here for now, though we may want
				// Vulkan 1.3 dynamic state extensions at some point
			} );

		}
		catch ( Exception ex )
		{
			mLogger.Error( $"Error while creating graphics device\nException message: {ex.Message}" );
			return false;
		}

		mRenderCommands = Factory.CreateCommandList();
		mBufferCommands = Factory.CreateCommandList();

		InitialiseCorePipelines();

		Initialised = true;
		return true;
	}

	public void Shutdown()
	{
		Initialised = false;
	}
}
