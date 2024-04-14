// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.Resources;
using Elegy.AssetSystem.Interfaces.Rendering;
using Elegy.Common.Assets;
using Elegy.RenderSystem.Interfaces;
using Elegy.RenderSystem.Interfaces.Rendering;

using Silk.NET.Windowing;

namespace Elegy.RenderSystem.Dummies
{
	internal class RenderNull : IRenderFrontend
	{
		public string Name => "Dummy renderer";
		public string Error => string.Empty;
		public bool Initialised => true;

		public void BeginFrame() { }

		public IEntity? CreateEntity( bool animated ) => null;
		public ILight? CreateLight() => null;
		public IMaterial? CreateMaterial( MaterialDefinition materialDefinition ) => null;
		public IMesh? CreateMesh( Model modelData ) => null;
		public ITexture? CreateTexture( TextureMetadata metadata, Span<byte> data ) => null;
		public Interfaces.Rendering.IView? CreateView( IWindow window ) => null;
		public Interfaces.Rendering.IView? CreateView( Texture renderTarget ) => null;
		public Interfaces.Rendering.IView? GetView( IWindow window ) => null;

		public void EndFrame() { }

		public bool FreeEntity( IEntity entity ) => true;
		public bool FreeLight( ILight light ) => true;
		public bool FreeMaterial( IMaterial material ) => true;
		public bool FreeMesh( IMesh mesh ) => true;
		public bool FreeTexture( ITexture texture ) => true;
		public bool FreeView( Interfaces.Rendering.IView view ) => true;

		public bool Init() => true;
		public void Shutdown() { }

		public void PresentView( in Interfaces.Rendering.IView view ) { }
		public void RenderView( in Interfaces.Rendering.IView view ) { }
	}
}
