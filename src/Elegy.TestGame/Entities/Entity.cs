// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

using Elegy.Assets;
using Godot;
using System.Diagnostics.CodeAnalysis;

namespace TestGame.Entities
{
	public abstract class Entity
	{
		public Vector3 Position
		{
			get => mRootNode.GlobalPosition;
			set => mRootNode.GlobalPosition = value;
		}

		public virtual void Spawn()
		{

		}

		public virtual void PostSpawn()
		{

		}

		public virtual void KeyValue( Dictionary<string, string> pairs )
		{
			if ( pairs.TryGetValue( "origin", out string? originString ) )
			{
				mRootNode.GlobalPosition = originString.ToVector3().ToGodot();
			}
		}

		public virtual void Destroy()
		{
			mComponents.ForEach( comp => comp.QueueFree() );
			mComponents.Clear();

			mRootNode?.QueueFree();
			mRootNode = null;
		}

		public virtual void Think()
		{

		}

		public virtual void PhysicsUpdate( float delta )
		{

		}

		// This is a very very improper way to do this, but I needed it for a quick way of setting a brush model
		public void AddBrushModel( ElegyMapDocument map, int entityId )
		{
			mComponents.Add( Assets.MapGeometry.CreateBrushModelNode( map, entityId ) );
			if ( mRootNode == null )
			{
				mRootNode = mComponents.Last() as Node3D;
			}
		}

		[NotNull]
		protected Node3D? mRootNode = null;
		protected List<Node> mComponents = new();
	}
}
