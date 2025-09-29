using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuUtilities;
using BepuUtilities.Memory;
using Elegy.App;
using Elegy.Common.Assets;
using Elegy.Common.Extensions;
using Elegy.Common.Interfaces;
using Elegy.Common.Maths;
using Elegy.Common.Utilities;
using Elegy.InputSystem.API;
using Elegy.RenderSystem.API;
using Elegy.RenderSystem.Objects;
using Silk.NET.Input;
using Silk.NET.Windowing;
using Veldrid;

namespace PhysBench;

public static class Program
{
	public class PhysBenchApp : IApplication
	{
		private TaggedLogger mLogger = new( "PhysBench" );

		private BufferPool mBufferPool = new();
		private Simulation? mSimulation;
		private BodyDescription[] mBodyDescs;
		private StaticDescription[] mStaticDescs;
		private BodyReference? mBodyReference;
		private StaticReference? mStaticReference;
		private int mCurrentBodyId = -1;
		private int mCurrentStaticId = -1;
		private float mNextSelectionTime;
		private CommandList mCommandList;
		private Vector2 mOldMousePosition;
		private Vector2 mMousePositionDeltaRaw;
		private Vector2 mMousePositionDeltaSmooth;
		private Vector3 mCameraOrbitAngles;
		private float mCameraOrbitDistance = 4.0f;

		public string Name => "PhysBench";
		public bool Initialised => true; // TODO: bookkeep this in the plugin system

		public bool Start()
		{
			mCommandList = Render.Factory.CreateCommandList();

			var narrowphase = new NarrowphaseCallbacks();
			narrowphase.OnCollision += OnCollision;

			for ( int i = 0; i < Environment.ProcessorCount; i++ )
			{
				mContactsThisFrame[i] = new();
			}

			mSimulation = Simulation.Create( mBufferPool,
				narrowphase,
				new PoseIntegratorCallbacks(),
				new SolveDescription( 8, 1 ) );

			var cylinder = new Cylinder( 0.5f, 1.7f );
			var capsule = new Capsule( 0.5f, 1.7f );
			var cylinderIndex = mSimulation.Shapes.Add( cylinder );
			var capsuleIndex = mSimulation.Shapes.Add( capsule );

			mStaticDescs =
			[
				new( new RigidPose( Vector3.Zero, Coords.QuatAboutRight( MathF.PI * 0.15f ) ),
					mSimulation.Shapes.Add( new Box( 2.0f, 2.0f, 0.5f ) ) ),
				new( RigidPose.Identity, cylinderIndex ),
				new( RigidPose.Identity, capsuleIndex ),
				new( RigidPose.Identity, mSimulation.Shapes.Add( new Sphere( 0.5f ) ) ),
				new( RigidPose.Identity, 
					mSimulation.Shapes.Add( new Triangle( Coords.Forward, Coords.Right, Coords.Left ) ) ),
				new( RigidPose.Identity, mSimulation.Shapes.Add(
					new Triangle( Coords.Forward + Coords.Up * 0.15f, Coords.Right + Coords.Up * 0.33f, Coords.Left ) ) )
			];

			mBodyDescs =
			[
				BodyDescription.CreateDynamic( RigidPose.Identity, new BodyInertia { InverseMass = 1.0f }, 
					new CollidableDescription( cylinderIndex, 
						BodyDescription.GetDefaultSpeculativeMargin( cylinder ),
						ContinuousDetection.Passive ),
					BodyDescription.GetDefaultActivity( cylinder ) ),

				BodyDescription.CreateDynamic( RigidPose.Identity, new BodyInertia { InverseMass = 1.0f }, 
					new CollidableDescription( capsuleIndex,
						BodyDescription.GetDefaultSpeculativeMargin( capsule ),
						ContinuousDetection.Passive ),
					BodyDescription.GetDefaultActivity( capsule ) )
			];

			SelectStatic( 4 ); // We are currently focusing on static triangle vs. kinematic cylinder
			SelectBody( 0 );

			View? view = Render.GetCurrentWindowView();
			if ( view is null )
			{
				throw new Exception( "No view found" );
			}

			view.Projection = Coords.CreatePerspectiveMatrix( 45.0f * Coords.Deg2Rad, 16.0f / 9.0f, 0.01f, 20.0f );
			view.Transform = Coords.CreateViewMatrixRadians(
				position: Coords.Up * 6.0f + Coords.Forward * 0.5f,
				angles: Coords.TurnDown * 0.25f
			);

			Input.Mouse.Scroll += ( _, wheel ) => { mCameraOrbitDistance += wheel.X; };

			Render.OnRender += RenderFrame;
			return true;
		}

		private ConcurrentDictionary<int, List<TestContact>> mContactsThisFrame = new();
		private void OnCollision( int workerIndex, TestContact data )
		{
			mContactsThisFrame[workerIndex].Add( data );
		}

		public bool RunFrame( float delta )
		{
			if ( Input.Keyboard.IsKeyPressed( Key.Escape ) )
			{
				return false;
			}

			// WASD, Space and Control: move body
			// QE: change body's shape
			// 12345...: change static's shape
			// RMB + drag: orbit around the static shape
			mMousePositionDeltaRaw = Input.Mouse.Position - mOldMousePosition;
			mMousePositionDeltaSmooth = mMousePositionDeltaSmooth.Lerp( mMousePositionDeltaRaw, 0.99f );

			if ( Input.Mouse.IsButtonPressed( MouseButton.Right ) )
			{
				if ( Input.Keyboard.IsKeyPressed( Key.AltLeft ) )
				{
					mCameraOrbitDistance += mMousePositionDeltaSmooth.Y * 0.02f;
				}
				else
				{
					mCameraOrbitAngles += Coords.TurnRight * mMousePositionDeltaSmooth.X * 0.05f;
					mCameraOrbitAngles -= Coords.TurnUp * mMousePositionDeltaSmooth.Y * 0.05f;
				}
			}

			Coords.DirectionsFromDegrees( mCameraOrbitAngles, out var forward, out _ );
			View view = Render.GetCurrentWindowView()!;
			view.Transform = Coords.CreateViewMatrixDegrees(
				position: forward * -mCameraOrbitDistance,
				angles: mCameraOrbitAngles
			);

			mOldMousePosition = Input.Mouse.Position;

			bool bodyMoved = false;
			(Key, Vector3)[] keyMoveMap =
			[
				(Key.W, Coords.Forward),
				(Key.A, Coords.Left),
				(Key.S, Coords.Back),
				(Key.D, Coords.Right),
				(Key.Space, Coords.Up),
				(Key.ControlLeft, Coords.Down)
			];
			foreach ( var (key, move) in keyMoveMap )
			{
				if ( Input.Keyboard.IsKeyPressed( key ) )
				{
					MoveBody( move, delta );
					bodyMoved = true;
				}
			}

			if ( bodyMoved )
			{
				ApplyBodyMovement();
			}
			else
			{
				StopBody();
			}

			mNextSelectionTime -= delta;
			if ( mNextSelectionTime <= 0.0f )
			{
				if ( Input.Keyboard.IsKeyPressed( Key.Q ) )
				{
					SelectBody( mCurrentBodyId - 1 );
					mNextSelectionTime = 0.3f;
				}
				else if ( Input.Keyboard.IsKeyPressed( Key.E ) )
				{
					SelectBody( mCurrentBodyId + 1 );
					mNextSelectionTime = 0.3f;
				}

				for ( int i = 0; i < mStaticDescs.Length; i++ )
				{
					if ( Input.Keyboard.IsKeyPressed( Key.Number1 + i ) )
					{
						SelectStatic( i );
						mNextSelectionTime = 0.3f;
						break;
					}
				}
			}

			mSimulation!.Timestep( delta );

			DebugDraw();

			foreach ( var contacts in mContactsThisFrame.Values )
			{
				Span<TestContact> span = CollectionsMarshal.AsSpan( contacts );
				for ( int i = 0; i < span.Length; i++ )
				{
					DebugDrawContact( span[i] );
				}

				contacts.Clear();
			}

			return true;
		}

		#region Debug drawing

		private void DebugDraw()
		{
			if ( mBodyReference is null || mStaticReference is null )
			{
				return;
			}

			Vector4 bodyColour = new( 1.0f, 0.5f, 0.0f, 1.0f );
			Vector4 staticColour = new( 1.0f, 0.9f, 0.5f, 1.0f );
			Vector4 collisionColour = new( 0.2f, 0.9f, 0.2f, 1.0f );

			DebugDrawShape( mBodyReference.Value.Pose, mBodyReference.Value.Collidable.Shape, bodyColour );
			DebugDrawShape( mStaticReference.Value.Pose, mStaticReference.Value.Shape, staticColour );
		}

		private void DebugDrawContact( TestContact contact )
		{
			var colour = new Vector4( 0.2f, 0.9f, 0.2f, 1.0f );
			var centre = contact.PositionAndDepth.ToVector3();

			Render.DebugLine( centre, centre + contact.Normal, colour );
		}

		private void DebugDrawShape( RigidPose pose, TypedIndex shape, Vector4 colour )
		{
			switch ( shape.Type )
			{
				case Box.Id: DebugDrawBox( pose, shape, colour ); break;
				case Capsule.Id: DebugDrawCapsule( pose, shape, colour ); break;
				case ConvexHull.Id: DebugDrawConvexHull( pose, shape, colour ); break;
				case Cylinder.Id: DebugDrawCylinder( pose, shape, colour ); break;
				case Sphere.Id: DebugDrawSphere( pose, shape, colour ); break;
				case Triangle.Id: DebugDrawTriangle( pose, shape, colour ); break;
			}
		}

		private void DebugDrawBox( RigidPose pose, TypedIndex shape, Vector4 colour )
		{
			var box = mSimulation!.Shapes.GetShape<Box>( shape.Index );
			var centre = pose.Position;
			var right = Vector3.Transform( new( box.HalfWidth, 0.0f, 0.0f ), pose.Orientation );
			var forward = Vector3.Transform( new( 0.0f, box.HalfHeight, 0.0f ), pose.Orientation );
			var up = Vector3.Transform( new( 0.0f, 0.0f, box.HalfLength ), pose.Orientation );

			// Top square
			Render.DebugLine( centre + forward + right + up, centre + forward - right + up, colour );
			Render.DebugLine( centre - forward + right + up, centre - forward - right + up, colour );
			Render.DebugLine( centre + forward + right + up, centre - forward + right + up, colour );
			Render.DebugLine( centre + forward - right + up, centre - forward - right + up, colour );
			// Bottom square
			Render.DebugLine( centre + forward + right - up, centre + forward - right - up, colour );
			Render.DebugLine( centre - forward + right - up, centre - forward - right - up, colour );
			Render.DebugLine( centre + forward + right - up, centre - forward + right - up, colour );
			Render.DebugLine( centre + forward - right - up, centre - forward - right - up, colour );
			// 4 corner pillars
			Render.DebugLine( centre + forward + right + up, centre + forward + right - up, colour );
			Render.DebugLine( centre + forward - right + up, centre + forward - right - up, colour );
			Render.DebugLine( centre - forward - right + up, centre - forward - right - up, colour );
			Render.DebugLine( centre - forward + right + up, centre - forward + right - up, colour );
		}

		private void DebugDrawCapsule( RigidPose pose, TypedIndex shape, Vector4 colour )
		{
			var capsule = mSimulation!.Shapes.GetShape<Capsule>( shape.Index );
			var centre = pose.Position;
			var halfHeight = capsule.HalfLength;
			var radius = capsule.Radius;
			QuaternionEx.TransformUnitY( pose.Orientation, out Vector3 forward );
			QuaternionEx.TransformUnitZ( pose.Orientation, out Vector3 up );
			var topSphere = centre + up * halfHeight;
			var bottomSphere = centre - up * halfHeight;

			DebugDrawCylinder( centre, forward, up, halfHeight, radius, colour );
			DebugDrawSphere( topSphere, forward, up, radius, colour, keepSide: 1 );
			DebugDrawSphere( bottomSphere, forward, up, radius, colour, keepSide: -1 );
		}

		private void DebugDrawConvexHull( RigidPose pose, TypedIndex shape, Vector4 colour )
		{
			throw new NotImplementedException();
		}

		private void DebugDrawCircle( Vector3 position, Vector3 poleAxis, Vector3 equatorAxis, float radius, Vector4 colour,
			int sidesPerQuarter = 2, int keepY = 0 )
		{
			// Create a quarter of a circle, scaled by radius
			Span<Vector2> quarterPoints = stackalloc Vector2[sidesPerQuarter];
			quarterPoints[0] = new( radius, 0.0f );
			for ( int i = 1; i < sidesPerQuarter; i++ )
			{
				// Good old polar coordinates
				float t = (float)i / sidesPerQuarter;
				(float y, float x) = MathF.SinCos( t * MathF.PI * 0.5f );
				quarterPoints[i] = new( x * radius, y * radius );
			}

			// Then construct a full circle by repeating that quarter in different quadrants,
			// simultaneously transforming it into (potentially rotated) 3D space
			Span<Vector3> points = stackalloc Vector3[sidesPerQuarter * 4];
			for ( int i = 0; i < sidesPerQuarter * 4; i++ )
			{
				int quadrantId = i / sidesPerQuarter;
				Vector2 p = quarterPoints[i % sidesPerQuarter];
				switch ( quadrantId )
				{
					case 1: p = new( -p.Y, p.X ); break; // (1,2) into (-2,1)
					case 2: p = new( -p.X, -p.Y ); break; // (1,2) into (-1,-2)
					case 3: p = new( p.Y, -p.X ); break; // (1,2) into (2,-1)
				}

				points[i] = p.X * equatorAxis + p.Y * poleAxis;
			}

			int startId = keepY >= 0 ? 0 : points.Length / 2;
			int endId = keepY > 0 ? points.Length / 2 : points.Length;

			for ( int i = startId; i < endId; i++ )
			{
				int j = (i + 1) % points.Length;
				Render.DebugLine( position + points[i], position + points[j], colour );
			}
		}

		private void DebugDrawCylinder( Vector3 position, Vector3 forward, Vector3 up, float halfHeight, float radius,
			Vector4 colour )
		{
			var right = Vector3.Cross( forward, up );
			var capTop = position + up * halfHeight;
			var capBottom = position - up * halfHeight;

			// Caps
			DebugDrawCircle( capTop, forward, right, radius, colour, 3 );
			DebugDrawCircle( capBottom, forward, right, radius, colour, 3 );

			// Premultiply the axes
			up *= halfHeight;
			forward *= radius;
			right *= radius;

			// 4 pillars, front, back, right, left
			Render.DebugLine( position + forward + up, position + forward - up, colour );
			Render.DebugLine( position - forward + up, position - forward - up, colour );
			Render.DebugLine( position + right + up, position + right - up, colour );
			Render.DebugLine( position - right + up, position - right - up, colour );
		}

		private void DebugDrawCylinder( RigidPose pose, TypedIndex shape, Vector4 colour )
		{
			var cylinder = mSimulation!.Shapes.GetShape<Cylinder>( shape.Index );
			QuaternionEx.TransformUnitY( pose.Orientation, out Vector3 forward );
			QuaternionEx.TransformUnitZ( pose.Orientation, out Vector3 up );
			DebugDrawCylinder( pose.Position, forward, up, cylinder.HalfLength, cylinder.Radius, colour );
		}

		private void DebugDrawSphere( Vector3 position, Vector3 forward, Vector3 up, float radius, Vector4 colour,
			int keepSide = 0 )
		{
			var right = Vector3.Cross( forward, up );
			DebugDrawCircle( position, up, forward, radius, colour, 4, keepSide );
			DebugDrawCircle( position, up, right, radius, colour, 4, keepSide );
		}

		private void DebugDrawSphere( RigidPose pose, TypedIndex shape, Vector4 colour, int keepSide = 0 )
		{
			// keepSide is used to help rendering capsules:
			// -1 -> keep bottom hemisphere
			// 0 -> keep both
			// 1 -> keep top hemisphere
			var sphere = mSimulation!.Shapes.GetShape<Sphere>( shape.Index );
			QuaternionEx.TransformUnitY( pose.Orientation, out Vector3 forward );
			QuaternionEx.TransformUnitZ( pose.Orientation, out Vector3 up );
			DebugDrawSphere( pose.Position, up, forward, sphere.Radius, colour, keepSide );
		}

		private void DebugDrawTriangle( RigidPose pose, TypedIndex shape, Vector4 colour )
		{
			var tri = mSimulation!.Shapes.GetShape<Triangle>( shape.Index );
			var centre = pose.Position;
			var p1 = Vector3.Transform( tri.A, pose.Orientation );
			var p2 = Vector3.Transform( tri.B, pose.Orientation );
			var p3 = Vector3.Transform( tri.C, pose.Orientation );

			Render.DebugLine( centre + p1, centre + p2, colour );
			Render.DebugLine( centre + p2, centre + p3, colour );
			Render.DebugLine( centre + p3, centre + p1, colour );
		}

		#endregion

		private void RenderFrame()
		{
			View? view = Render.GetCurrentWindowView();
			if ( view is null )
			{
				return;
			}

			// Update debug draw buffers etc.
			Render.UpdateBuffers();

			// We just kinda clear the screen, that's all
			mCommandList.Begin();
			Render.SetRenderView( mCommandList, view );
			mCommandList.End();

			Render.Device.SubmitCommands( mCommandList );
		}

		public void Shutdown()
		{
			mSimulation?.Dispose();
			mBufferPool.Clear();
		}

		#region Selection and movement

		private void SelectStatic( int newStatic )
		{
			if ( mCurrentStaticId == newStatic )
			{
				return;
			}

			mCurrentStaticId = newStatic % mStaticDescs.Length;

			if ( mStaticReference is null )
			{
				StaticHandle handle = mSimulation!.Statics.Add( mStaticDescs[mCurrentStaticId] );
				mStaticReference = mSimulation!.Statics[handle];
			}
			else
			{
				mStaticReference.Value.ApplyDescription( mStaticDescs[mCurrentStaticId] );
			}
		}

		private void SelectBody( int newBody )
		{
			if ( mCurrentBodyId == newBody )
			{
				return;
			}

			mCurrentBodyId = Math.Abs( newBody % mBodyDescs.Length );

			if ( mBodyReference is null )
			{
				BodyHandle handle = mSimulation!.Bodies.Add( mBodyDescs[mCurrentBodyId] );
				mBodyReference = mSimulation!.Bodies[handle];
			}
			else
			{
				mBodyReference.Value.ApplyDescription( mBodyDescs[mCurrentBodyId] );
			}
		}

		private void StopBody()
		{
			mBodyReference.Value.Velocity.Linear *= 0.8f;
		}

		private void MoveBody( Vector3 move, float delta )
		{
			//mBodyReference.Value.Pose.Position += move * delta * 0.25f;
			mBodyReference.Value.Velocity.Linear += move * delta * 30.0f;
		}

		private void ApplyBodyMovement()
		{
			mSimulation.Awakener.AwakenBody( mBodyReference.Value.Handle );
			mSimulation.Bodies.UpdateBounds( mBodyReference.Value.Handle );
		}

		#endregion
	}

	// NOTE: Set the working directory to testgame/
	public static void Main( string[] args )
	{
		Console.Title = "Scratchpad/PhysBench";
		Window.PrioritizeSdl();

		AppTemplate.Start<PhysBenchApp>(
			config: new()
			{
				Args = args,
				Engine = EngineConfig.Game( "game" ),
				WithMainWindow = true,
				ToolMode = true
			},
			windowPlatform: Window.GetWindowPlatform( viewOnly: false )
			                ?? throw new Exception( "SDL2 not found" )
		);
	}
}
