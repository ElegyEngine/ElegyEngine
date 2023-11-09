// SPDX-FileCopyrightText: 2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy.Assets.ElegyMapData
{
	public class Entity
	{
		public int RenderMeshId { get; set; }

		public int CollisionMeshId { get; set; }

		/// <summary>
		/// Brush entities can have occluder meshes associated
		/// with them. E.g. doors may block visibility.
		/// </summary>
		public int OccluderMeshId { get; set; }

		public Dictionary<string, string> Attributes { get; set; } = new();
	}
}
