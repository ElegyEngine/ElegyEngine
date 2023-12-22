// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace TestGame.Entities
{
	public class PropTest : Entity
	{
		public override void Spawn()
		{
			base.Spawn();

			mRootNode = Nodes.CreateNode<Node3D>();
		}

		public override void KeyValue( Dictionary<string, string> pairs )
		{
			base.KeyValue( pairs );

			try
			{
				if ( pairs.TryGetValue( "model", out string? modelPath ) )
				{
					mMesh = mRootNode.CreateChild<MeshInstance3D>();
					mMesh.Mesh = Assets.GLTFModel.Load( modelPath );
					// TODO: just like how we have a MaterialSystem now, it'd be good to have:
					//mMesh = Models.Load( modelPath );
				}
				else
				{
					TestAnimatedMesh();
				}
			}
			catch( Exception ex )
			{
				Console.Error( $"Exception \"handled\": {ex.Message}" );
				Console.Error( $"Stack trace: {ex.StackTrace}" );
			}
		}

		public override void Destroy()
		{
			// The animation player is created as a global node,
			// so it's gotta be cleaned up manually
			mAnimationPlayer.QueueFree();

			base.Destroy();
		}

		private ArrayMesh CreateTestMesh()
		{
			SurfaceTool st = new();

			st.Begin( Mesh.PrimitiveType.Triangles );

			st.SetBones( new int[] { 0, 1, 0, 0 } );
			st.SetWeights( new float[] { 1.0f, 0.0f, 0.0f, 0.0f } );
			st.SetUV( new( 0.0f, 0.0f ) );
			st.SetNormal( new( 0.0f, 1.0f, 0.0f ) );
			st.AddVertex( new( 0.0f, 0.0f, 0.0f ) );

			st.SetBones( new int[] { 0, 1, 0, 0 } );
			st.SetWeights( new float[] { 1.0f, 0.0f, 0.0f, 0.0f } );
			st.SetUV( new( 0.0f, 1.0f ) );
			st.SetNormal( new( 0.0f, 1.0f, 0.0f ) );
			st.AddVertex( new( 0.0f, 0.0f, -1.0f ) );

			st.SetBones( new int[] { 0, 1, 0, 0 } );
			st.SetWeights( new float[] { 0.0f, 1.0f, 0.0f, 0.0f } );
			st.SetUV( new( 1.0f, 0.0f ) );
			st.SetNormal( new( 0.0f, 1.0f, 0.0f ) );
			st.AddVertex( new( 1.0f, 0.0f, 0.0f ) );

			st.GenerateTangents();
			st.SetMaterial( new StandardMaterial3D()
			{
				AlbedoColor = Color.Color8( 255, 128, 192 ),
				MetallicSpecular = 0.5f,
				Roughness = 0.5f
			} );

			return st.Commit();
		}

		private AnimationLibrary TestAnimatedLibrary()
		{
			AnimationLibrary animationLibrary = new();

			Animation idleAnimation = new();
			int positionTrack = idleAnimation.AddTrack( Animation.TrackType.Position3D );
			idleAnimation.TrackSetPath( positionTrack, $"Skeleton3D:tail_bone" );
			idleAnimation.PositionTrackInsertKey( positionTrack, 0.0, Vector3.Zero );
			idleAnimation.PositionTrackInsertKey( positionTrack, 1.0, Vector3.Up );
			idleAnimation.PositionTrackInsertKey( positionTrack, 2.0, Vector3.Zero );
			idleAnimation.Length = 2.0f;
			idleAnimation.LoopMode = Animation.LoopModeEnum.Linear;

			animationLibrary.AddAnimation( "idle", idleAnimation );

			return animationLibrary;
		}

		private void TestAnimatedMesh()
		{
			mSkeleton = mRootNode.CreateChild<Skeleton3D>();
			mSkeleton.Name = "Skeleton3D";
			mSkeleton.GlobalPosition = mRootNode.GlobalPosition;
			// Bone in the centre of the model
			mSkeleton.AddBone( "root_bone" );
			mSkeleton.SetBoneRest( 0, new Transform3D( Basis.Identity, Vector3.Zero ) );
			// Bone that is positioned roughly on 1,0,0
			mSkeleton.AddBone( "tail_bone" );
			mSkeleton.SetBoneRest( 1, new Transform3D( Basis.Identity, Vector3.Left ) );

			mMesh = mSkeleton.CreateChild<MeshInstance3D>();
			mMesh.Mesh = CreateTestMesh();
			mMesh.Skeleton = mSkeleton.GetPath();

			mAnimationPlayer = Nodes.CreateNode<AnimationPlayer>();
			mAnimationPlayer.RootNode = mRootNode.GetPath();
			mAnimationPlayer.AddAnimationLibrary( "", TestAnimatedLibrary() );
			mAnimationPlayer.Play( "idle" );
		}

		private AnimationPlayer mAnimationPlayer;
		private Skeleton3D mSkeleton;
		private MeshInstance3D mMesh;
	}
}
