// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Engine.API
{
	public static partial class Materials
	{
		internal static void SetMaterialSystem( MaterialSystemInternal? system )
			=> mMaterialSystem = system;

		private static MaterialSystemInternal? mMaterialSystem;
	}
}
