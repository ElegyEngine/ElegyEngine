// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Engine.Interfaces;
using Elegy.Engine.Interfaces.Rendering;

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
