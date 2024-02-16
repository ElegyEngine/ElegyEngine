// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
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
