// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.RenderSystem.Objects;

namespace Elegy.RenderSystem.API
{
	public static partial class Render
	{
		/// <summary>Starts the frame timers, clears stuff etc.</summary>
		public static void BeginFrame()
		{
			throw new NotImplementedException();
		}

		/// <summary>Finishes the frame, timers etc.</summary>
		public static void EndFrame()
		{
			throw new NotImplementedException();
		}

		/// <summary>Executes all draw commands for a view.</summary>
		public static void RenderView( in View view )
		{
			throw new NotImplementedException();
		}

		/// <summary>Presents the view to its window.</summary>
		public static void PresentView( in View view )
		{
			throw new NotImplementedException();
		}
	}
}
