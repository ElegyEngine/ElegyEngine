// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

using Elegy.Assets.ModelData;

namespace Elegy.Assets
{
	// TODO: Okay, hear me out. What if we have a Mesh asset and a Skeleton asset,
	// so that we can easily have shared Animation assets?

	/// <summary>
	/// Raw data for Elegy models. Models can represent NPCs, weapons,
	/// terrain, 3D UIs, props and other things. Elegy's model format is
	/// generally concerned with monolithic models for individual entities,
	/// as opposed to models that host entire scenes and submodels.
	/// </summary>
	public class Model
	{
		public string Name { get; set; } = string.Empty;

		public List<ModelData.Mesh> Meshes { get; set; } = new();

		public List<BoneJoint> Joints { get; set; } = new();
	}
}
