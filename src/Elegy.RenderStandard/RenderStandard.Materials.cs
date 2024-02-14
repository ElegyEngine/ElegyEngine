// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Assets;
using Elegy.Rendering;
using Elegy.Utilities;

namespace Elegy.RenderStandard;

public partial class RenderStandard : IRenderFrontend
{
	public IMaterial CreateMaterial( MaterialDefinition materialDefinition )
	{
		throw new NotImplementedException();
	}


	public bool FreeMaterial( IMaterial material )
	{
		throw new NotImplementedException();
	}
}
