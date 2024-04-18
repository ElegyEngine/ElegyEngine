// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

global using Console = Elegy.ConsoleSystem.API.Console;

using Elegy.ConsoleSystem;
using Elegy.RenderSystem.Interfaces;
using Elegy.RenderSystem.API;

using Veldrid;

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

		GraphicsDevice? device = Render.CreateGraphicsDevice();
		if ( device is null )
		{
			return false;
		}

		mDevice = device;

		mRenderCommands = Factory.CreateCommandList();
		mBufferCommands = Factory.CreateCommandList();

		InitialiseCorePipelines();

		Initialised = true;
		return true;
	}

	public void Shutdown()
	{
		mLogger.Log( "Shutdown" );
		Initialised = false;
	}
}
