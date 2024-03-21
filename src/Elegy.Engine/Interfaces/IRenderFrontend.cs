// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Engine.Interfaces.Rendering;
using Elegy.Engine.Resources;

using IWindow = Silk.NET.Windowing.IWindow;

namespace Elegy.Engine.Interfaces
{
	/// <summary>
	/// Renderer frontend plugin. Implements rendering techniques for a particular graphical style.
	/// </summary>
	public interface IRenderFrontend : IPlugin
	{
		/// <summary></summary>
		IEntity CreateEntity( bool animated );
		
		/// <summary></summary>
		bool FreeEntity( IEntity entity );

		/// <summary></summary>
		ILight CreateLight();

		/// <summary></summary>
		bool FreeLight( ILight light );

		/// <summary></summary>
		IMaterial CreateMaterial( Material material );
		
		/// <summary></summary>
		bool FreeMaterial( IMaterial material );

		/// <summary></summary>
		IMesh CreateMesh( Model modelData );

		/// <summary></summary>
		bool FreeMesh( IMesh mesh );

		/// <summary></summary>
		IView CreateView( IWindow window );

		/// <summary></summary>
		IView CreateView( Texture renderTarget );

		/// <summary></summary>
		IView? GetView( IWindow window );

		/// <summary></summary>
		bool FreeView( IView view );

		/// <summary></summary>
		void BeginFrame();

		/// <summary></summary>
		void EndFrame();

		/// <summary></summary>
		void RenderView( in IView view );

		/// <summary></summary>
		void PresentView( in IView view );
	}
}
