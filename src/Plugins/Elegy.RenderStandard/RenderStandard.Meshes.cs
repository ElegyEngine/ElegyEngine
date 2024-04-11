// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.AssetSystem.Interfaces;
using Elegy.AssetSystem.Interfaces.Rendering;
using Elegy.RenderSystem.Interfaces;
using Elegy.RenderSystem.Interfaces.Rendering;

namespace Elegy.RenderStandard;

public partial class RenderStandard : IRenderFrontend
{
	public IMesh CreateMesh( Model modelData )
	{
		throw new NotImplementedException();
	}

	public bool FreeMesh( IMesh mesh )
	{
		throw new NotImplementedException();
	}
}
