// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.RenderSystem.Objects;
using Elegy.RenderSystem.Resources;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Veldrid;

namespace Elegy.RenderSystem.API
{
	public static partial class Render
	{
		/// <summary>Starts the frame timers, clears stuff etc.</summary>
		public static void BeginFrame()
		{
			mCpuTime = GetSeconds();
		}

		/// <summary>Finishes the frame, timers etc.</summary>
		public static void EndFrame()
		{
			mCpuTime = GetSeconds() - mCpuTime;
			mGpuTime = GetSeconds();

			mDevice.WaitForIdle();

			mGpuTime = GetSeconds() - mGpuTime;
		}

		/// <summary>Draws a view to the backbuffer.</summary>
		public static void RenderView( in View view )
		{
			Debug.Assert( view.Window is not null );
			Debug.Assert( view.TargetSwapchain is not null );

			mRenderCommands.Begin();

			view.UpdateBuffers( mDevice );
			SetRenderView( mRenderCommands, view );
			RenderViewIntoBackbuffer( view );

			mRenderCommands.End();
			Device.SubmitCommands( mRenderCommands );
		}

		/// <summary>Presents the view to its window.</summary>
		public static void PresentView( in View view )
		{
			Debug.Assert( view.TargetSwapchain is not null );

			mPresentTime = GetSeconds();
			mDevice.SwapBuffers( view.TargetSwapchain );
			mPresentTime = GetSeconds() - mPresentTime;

			//mLogger.Log( "New frame" );
			//mLogger.Log( $"CPU: {mCpuTime * 1000.0 * 1000.0} us" );
			//mLogger.Log( $"GPU: {mGpuTime * 1000.0 * 1000.0} us" );
			//mLogger.Log( $"SWP: {mPresentTime * 1000.0 * 1000.0} us" );
		}
	}
}
