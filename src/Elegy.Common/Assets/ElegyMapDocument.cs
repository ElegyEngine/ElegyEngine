// SPDX-FileCopyrightText: 2023 Admer Šuko
// SPDX-License-Identifier: MIT

using Elegy.Assets.ElegyMapData;
using Elegy.Text;

namespace Elegy.Assets
{
	/// <summary>
	/// Standard Elegy level format.
	/// </summary>
	public class ElegyMapDocument
	{
		//// <summary>
		//// Visibility data.
		//// </summary>
		//public List<MapLeaf> VisibilityLeaves { get; set; } = new();

		/// <summary>
		/// IDs of world meshes.
		/// </summary>
		public List<int> WorldMeshIds { get; set; } = new();

		/// <summary>
		/// Map entities.
		/// </summary>
		public List<Entity> Entities { get; set; } = new();

		/// <summary>
		/// Collision meshes for collision detection.
		/// </summary>
		public List<CollisionMesh> CollisionMeshes { get; set; } = new();

		/// <summary>
		/// Occluder meshes for real-time dynamic occlusion culling.
		/// </summary>
		public List<OccluderMesh> OccluderMeshes { get; set; } = new();

		/// <summary>
		/// Visual renderable meshes.
		/// </summary>
		public List<RenderMesh> RenderMeshes { get; set; } = new();

		/// <summary>
		/// Writes the contents of this <see cref="ElegyMapDocument"/> to a file.
		/// </summary>
		public void WriteToFile( string path )
			=> JsonHelpers.Write( this, path );

		/// <summary>
		/// Loads an <see cref="ElegyMapDocument"/> from a file.
		/// </summary>
		/// <exception cref="Exception">File isn't an ELF</exception>
		public static ElegyMapDocument? LoadFromFile( string path )
			=> JsonHelpers.LoadFrom<ElegyMapDocument>( path );
	}
}
