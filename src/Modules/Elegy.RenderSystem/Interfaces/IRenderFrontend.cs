// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.Resources;
using Elegy.AssetSystem.Interfaces.Rendering;
using Elegy.Common.Assets;
using Elegy.Common.Interfaces;
using Elegy.RenderSystem.Interfaces.Rendering;

using IWindow = Silk.NET.Windowing.IWindow;

namespace Elegy.RenderSystem.Interfaces
{
	/// <summary>
	/// Renderer frontend plugin. Implements rendering techniques for a particular graphical style.
	/// </summary>
	public interface IRenderFrontend : IPlugin
	{
		/// <summary>Creates and fills pipelines.</summary>
		bool CreateCorePipelines();

		/// <summary>Creates a render entity.</summary>
		IEntity? CreateEntity( bool animated );
		
		/// <summary>Frees a render entity.</summary>
		bool FreeEntity( IEntity entity );

		/// <summary>Creates a render light.</summary>
		ILight? CreateLight();

		/// <summary>Frees a render light.</summary>
		bool FreeLight( ILight light );

		/// <summary>Creates a render material.</summary>
		IMaterial? CreateMaterial( MaterialDefinition materialDefinition );
		
		/// <summary>Frees a render material.</summary>
		bool FreeMaterial( IMaterial material );

		/// <summary>Creates a render texture.</summary>
		ITexture? CreateTexture( TextureMetadata metadata, Span<byte> data );

		/// <summary>Frees a render texture.</summary>
		bool FreeTexture( ITexture texture );

		/// <summary>Creates a render mesh.</summary>
		IMesh? CreateMesh( Model modelData );

		/// <summary>Frees a render mesh.</summary>
		bool FreeMesh( IMesh mesh );

		/// <summary>Creates a render view from a window.</summary>
		IView? CreateView( IWindow window );

		/// <summary>Creates a render view from a render target texture.</summary>
		IView? CreateView( Texture renderTarget );

		/// <summary>Gets the window's view, if any.</summary>
		IView? GetView( IWindow window );

		/// <summary>Frees a view.</summary>
		bool FreeView( IView view );

		/// <summary>Starts the frame timers, clears stuff etc.</summary>
		void BeginFrame();

		/// <summary>Finishes the frame, timers etc.</summary>
		void EndFrame();

		/// <summary>Executes all draw commands for a view.</summary>
		void RenderView( in IView view );

		/// <summary>Presents the view to its window.</summary>
		void PresentView( in IView view );
	}
}
