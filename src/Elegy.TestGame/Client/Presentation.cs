// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace TestGame.Client
{
	// Presentation layer. Spawns a camera and displays it to a viewport
	public class Presentation
	{
		public Presentation()
		{
			// One little camera attached to the world
			mCamera = Nodes.CreateNode<Camera3D>();
			mCamera.Current = true;
		}

		public void Update()
		{
			if ( null != mNodeToTrack )
			{
				Position = mNodeToTrack.Position;
				Angles = mNodeToTrack.Rotation;
			}
		}

		public void Track( Node3D node )
		{
			mNodeToTrack = node;
		}

		public void Unlink()
		{
			mNodeToTrack = null;
		}

		public Vector3 Position { get => mCamera.Position; set => mCamera.Position = value; }
		public Vector3 Angles { get => mCamera.Rotation; set => mCamera.Rotation = value; }

		private Node3D? mNodeToTrack = null;
		private Camera3D mCamera;
	}
}
