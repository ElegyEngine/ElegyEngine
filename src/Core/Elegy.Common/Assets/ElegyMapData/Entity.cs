// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Common.Assets.ElegyMapData
{
	/// <summary></summary>
	public class Entity
	{
		/// <summary></summary>
		public int RenderMeshId { get; set; }

		/// <summary></summary>
		public int CollisionMeshId { get; set; }

		/// <summary>
		/// Brush entities can have occluder meshes associated
		/// with them. E.g. doors may block visibility.
		/// </summary>
		public int OccluderMeshId { get; set; }
		
		/// <summary></summary>
		public Dictionary<string, string> Attributes { get; set; } = new();
	}
}
