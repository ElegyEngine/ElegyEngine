// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy.Extensions
{
	/// <summary>
	/// Godot node extensions.
	/// </summary>
	public static class NodeExtensions
	{
		/// <summary>
		/// Creates a node and attaches it to this node.
		/// </summary>
		public static T CreateChild<T>( this Node parent ) where T : Node, new()
		{
			T child = new T();
			parent.AddChild( child );
			return child;
		}

		/// <summary>
		/// Forward direction vector of this node.
		/// </summary>
		public static Vector3 Forward( this Node3D node )
		{
			return node.GlobalTransform.Forward();
		}

		/// <summary>
		/// Right direction vector of this node.
		/// </summary>
		public static Vector3 Right( this Node3D node )
		{
			return node.GlobalTransform.Right();
		}

		/// <summary>
		/// Up direction vector of this node.
		/// </summary>
		public static Vector3 Up( this Node3D node )
		{
			return node.GlobalTransform.Up();
		}

		/// <summary>
		/// Local forward direction vector of this node.
		/// </summary>
		public static Vector3 ForwardLocal( this Node3D node )
		{
			return node.Transform.Forward();
		}

		/// <summary>
		/// Local right direction vector of this node.
		/// </summary>
		public static Vector3 RightLocal( this Node3D node )
		{
			return node.Transform.Right();
		}

		/// <summary>
		/// Local up direction vector of this node.
		/// </summary>
		public static Vector3 UpLocal( this Node3D node )
		{
			return node.Transform.Up();
		}
	}
}
