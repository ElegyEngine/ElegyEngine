// SPDX-FileCopyrightText: 2024 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy
{
	public static partial class Core
	{
		internal static void SetCore( CoreInternal core )
		{
			mCoreSystem = core;
		}

		private static CoreInternal mCoreSystem;
	}
}
