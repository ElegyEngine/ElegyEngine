// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Silk.NET.Windowing;

namespace Elegy.RenderSystem.API
{
	public static partial class Render
	{
		/// <summary>
		/// Renders everything for a given window.
		/// </summary>
		/// <param name="window">The window to render into.</param>
		public static void RenderFrame( IWindow window )
		{
			IView? view = Render.Instance.GetView( window );
			if ( view is null )
			{
				mLogger.Error( "Cannot render frame - there is no renderview for the window!" );
				return;
			}

			RenderFrame( view );
		}

		/// <summary>
		/// Renders everything for a given view.
		/// </summary>
		/// <param name="view">The view to render from. Will render and update an <see cref="IWindow"/>.</param>
		public static void RenderFrame( IView view )
		{
			Render.Instance.BeginFrame();
			Render.Instance.RenderView( view );
			Render.Instance.EndFrame();
			Render.Instance.PresentView( view );
		}
	}
}
