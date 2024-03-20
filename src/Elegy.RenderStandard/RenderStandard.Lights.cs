// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Engine.Interfaces;
using Elegy.Engine.Interfaces.Rendering;

namespace Elegy.RenderStandard;

public partial class RenderStandard : IRenderFrontend
{
	public ILight CreateLight()
	{
		throw new NotImplementedException();
	}

	public bool FreeLight( ILight light )
	{
		throw new NotImplementedException();
	}
}
