// SPDX-FileCopyrightText: 2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy
{
	public static partial class Materials
	{
		internal static void SetMaterialSystem( MaterialSystemInternal system )
			=> mMaterialSystem = system;

		private static MaterialSystemInternal? mMaterialSystem;
	}
}
