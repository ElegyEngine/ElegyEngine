// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy.Utilities
{
	/// <summary>
	/// Utilities for interacting with Godot nodes.
	/// </summary>
	public static class Nodes
	{
		/// <summary>
		/// Creates a node and attaches it to the root world node.
		/// </summary>
		public static T CreateNode<T>() where T : Node, new()
		{
			T node = Engine.RootNode.CreateChild<T>();
			if ( node is Node3D )
			{
				(node as Node3D).TopLevel = true;
			}

			return node;
		}

		/// <summary>
		/// Creates a CollisionShape3D and creates either a ConcavePolygonShape3D
		/// or ConvexPolygonShape3D depending on the <paramref name="concave"/> parameter.
		/// </summary>
		public static Shape3D CreateCollisionShape( ArrayMesh mesh, bool concave = true )
		{
			if ( !concave )
			{
				Console.Warning( "Nodes.CreateCollisionShape", "'concave = false' is not implemented yet, switching to true" );
			}

			// The collision mesh is a bunch of triangles, organised in triplets of Vector3
			List<Vector3> collisionMesh = new();

			// Since ArrayMesh is kinda difficult to get data from *directly*, we use MeshDataTool
			for ( int surfaceId = 0; surfaceId < mesh._Surfaces.Count; surfaceId++ )
			{
				MeshDataTool tool = new();
				tool.CreateFromSurface( mesh, surfaceId );

				for ( int faceId = 0; faceId < tool.GetFaceCount(); faceId++ )
				{
					for ( int vertexNum = 0; vertexNum < 3; vertexNum++ )
					{
						int vertexId = tool.GetFaceVertex( faceId, vertexNum );
						collisionMesh.Add( tool.GetVertex( vertexId ) );
					}
				}
			}

			ConcavePolygonShape3D meshShape = new();
			meshShape.Data = collisionMesh.ToArray();

			return meshShape;
		}
	}
}
