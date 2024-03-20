﻿// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets.MeshData;

namespace Elegy.Common.Assets
{
	/// <summary>
	/// Raw data for Elegy model skeletons.
	/// </summary>
	public class Skeleton
	{
		public string Name { get; set; } = string.Empty;

		public List<BoneJoint> Joints { get; set; } = new();
	}
}
