// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Maths;
using Elegy.Utilities.Interfaces;
using Silk.NET.Windowing;
using System.Numerics;
using Veldrid;

namespace Elegy.Rendering
{
	// TODO: IView extensions so we can, like,
	// set Projection and Transform nicely

	/// <summary>
	/// A render view. You may think of it as a camera in the 3D world,
	/// which outputs images to a window or a texture image.
	/// </summary>
	public interface IView : ITransformable
	{
		/// <summary>
		/// Layers that this view will see.
		/// </summary>
		int Mask { get; set; }

		/// <summary>
		/// The resolution of this render view.
		/// </summary>
		Vector2I RenderSize { get; set; }

		/// <summary>
		/// The window that this view renders into. It doesn't necessarily have to do that though.
		/// </summary>
		IWindow? Window { get; }

		/// <summary>
		/// The frame buffer for this view.
		/// </summary>
		Framebuffer Framebuffer { get; }

		/// <summary>
		/// Render-to-texture target.
		/// </summary>
		Texture? TargetTexture { get; }

		/// <summary>
		/// The swapchain associated with this view. Used by <see cref="IRenderFrontend.PresentView(in IView)"/>.
		/// </summary>
		Swapchain? TargetSwapchain { get; }

		/// <summary>
		/// Projection matrix.
		/// </summary>
		Matrix4x4 Projection { get; set; }
	}
}
