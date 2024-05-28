// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Maths;
using Elegy.RenderBackend.Assets;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace Elegy.RenderSystem.Resources
{
	public class GlobalMaterialParameterBundle
	{
		public GlobalMaterialParameterBundle( GraphicsDevice device, List<GlobalParameter> parameters )
		{

		}

		public ResourceSet Set { get; private set; }

		public List<MaterialParameter> Parameters { get; private set; } = new();
	}
}
