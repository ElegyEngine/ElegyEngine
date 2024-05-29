// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.RenderBackend.Assets;
using Veldrid;

using Utils = Elegy.RenderSystem.Resources.MaterialParameterUtils;

namespace Elegy.RenderSystem.Resources
{
	public class MaterialParameterSet : IDisposable
	{
		public MaterialParameterSet( GraphicsDevice device, MaterialParameterLevel level, ResourceLayout layout, List<MaterialParameter> parametres )
		{
			mDevice = device;
			ParameterLevel = level;
			ResourceLayout = layout;
			Parameters = parametres;

			RegenerateSet();
		}

		private GraphicsDevice mDevice;
		public MaterialParameterLevel ParameterLevel { get; private set; }
		public List<MaterialParameter> Parameters { get; private set; } = new();

		public ResourceLayout ResourceLayout { get; private set; }
		public ResourceSet ResourceSet { get; private set; }

		/// <summary>
		/// Regenerates the resource set.
		/// </summary>
		public void RegenerateSet()
		{
			ResourceSet = Utils.GenerateResourceSet( mDevice, ResourceLayout, Parameters );
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			ResourceSet.Dispose();
		}
	}
}
