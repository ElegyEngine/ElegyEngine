using System.Diagnostics;
using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuUtilities;
using BepuUtilities.Collections;
using BepuUtilities.Memory;
using Game.Shared.PhysicsSystem.Interfaces;

namespace Game.Shared.PhysicsSystem.Subsystems
{
	public unsafe class CharacterMovement : IPhysicsSubsystem
	{
		private BufferPool mPool;
		private Simulation mSim;
		private int[] mBodyToCharIndex;
		private List<CharacterController> mChars;
		private Buffer<(int Start, int Count)> mBoundingBoxExpansionJobs;
		private Buffer<ContactCollectionWorkerCache> mContactCollectionWorkerCaches;
		private Buffer<AnalyzeContactsWorkerCache> mAnalyzeContactsWorkerCaches;

		public int CharacterCount => mChars.Count( c => c.BodyHandle.Value != 0 );
		public Span<CharacterController> Characters => mChars.AsSpan();

		public CharacterMovement( int characterCapacity, int handleCapacity = 4096 )
		{
			mBodyToCharIndex = new int[handleCapacity];
			Array.Fill( mBodyToCharIndex, -1 );
			mChars = new( characterCapacity );

			mAnalyzeContactsWorker = AnalyzeContactsWorker;
			mExpandBoundingBoxesWorker = ExpandBoundingBoxesWorker;
		}

		private readonly struct CharacterMovementContactCollector : IContactModifier
		{
			public CharacterMovement Owner { init; get; }

			public bool ConfigureContactManifold<TManifold>( int workerIndex, CollidablePair pair, ref TManifold manifold, ref PairMaterialProperties pairMaterial )
				where TManifold : unmanaged, IContactManifold<TManifold>
			{
				Owner.TryReportContacts( pair, ref manifold, workerIndex, ref pairMaterial );
				return true;
			}
		}

		public void Init( Simulation simulation, Action<IContactFilter> registerFilter, Action<IContactModifier> registerModifier,
			Action<IIntegrator> registerIntegrator )
		{
			mSim = simulation;
			mPool = mSim.BufferPool;

			simulation.Solver.Register<DynamicCharacterMotionConstraint>();
			simulation.Solver.Register<StaticCharacterMotionConstraint>();
			simulation.Timestepper.BeforeCollisionDetection += PrepareForContacts;
			simulation.Timestepper.CollisionsDetected += AnalyzeContacts;

			registerModifier( new CharacterMovementContactCollector { Owner = this } );
		}

		#region Prepare for contacts

		/// <summary>
		/// Essentially, a physics object that supports the character, i.e.
		/// the character's "standing object".
		/// </summary>
		private struct SupportCandidate
		{
			public Vector3 OffsetFromCharacter;
			public float Depth;
			public Vector3 OffsetFromSupport;
			public Vector3 Normal;
			public CollidableReference Support;
		}

		/// <summary>
		/// Per-thread support candidates.
		/// </summary>
		private struct ContactCollectionWorkerCache
		{
			public Buffer<SupportCandidate> SupportCandidates;

			public ContactCollectionWorkerCache( int maximumCharacterCount, BufferPool pool )
			{
				pool.Take( maximumCharacterCount, out SupportCandidates );
				for ( int i = 0; i < maximumCharacterCount; ++i )
				{
					//Initialize the depths to a value that guarantees replacement.
					SupportCandidates[i].Depth = float.MinValue;
				}
			}

			public void Dispose( BufferPool pool )
			{
				pool.Return( ref SupportCandidates );
			}
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private bool TryReportContacts<TManifold>( CollidableReference characterCollidable, CollidableReference supportCollidable, CollidablePair pair,
			ref TManifold manifold, int workerIndex ) where TManifold : struct, IContactManifold<TManifold>
		{
			if ( characterCollidable.Mobility == CollidableMobility.Dynamic && characterCollidable.BodyHandle.Value < mBodyToCharIndex.Length )
			{
				BodyHandle characterBodyHandle = characterCollidable.BodyHandle;
				int characterIndex = mBodyToCharIndex[characterBodyHandle.Value];
				if ( characterIndex >= 0 )
				{
					//This is actually a character.
					ref var character = ref Characters[characterIndex];
					//Our job here is to process the manifold into a support representation.
					//That means a single point, normal, and importance heuristic.
					//Note that we cannot safely pick from the candidates in this function -
					//it is likely executed from a multithreaded context, so all we can do is
					//output the pair's result into a worker-exclusive buffer.

					//Contacts with sufficiently negative depth will not be considered support candidates.
					//Contacts with intermediate depth (above minimum threshold,
					//but still below negative epsilon) may be candidates if the character previously had support.
					//Contacts with depth above negative epsilon always pass the depth test.

					//Maximum depth is used to heuristically choose which contact represents the support.
					//Note that this could be changed to subtly modify the behavior- for example, dotting the movement direction with the support normal and such.
					//A more careful choice of heuristic could make the character more responsive when trying to 'step' up obstacles.

					//Note that the body may be inactive during this callback even though it will be activated by new constraints after the narrow phase flushes.
					//Have to take into account the current potentially inactive location.
					ref var bodyLocation = ref mSim.Bodies.HandleToLocation[character.BodyHandle.Value];
					ref var set = ref mSim.Bodies.Sets[bodyLocation.SetIndex];
					ref var pose = ref set.DynamicsState[bodyLocation.Index].Motion.Pose;
					QuaternionEx.Transform( character.LocalUp, pose.Orientation, out var up );
					//Note that this branch is compiled out - the generic constraints force type specialization.
					if ( manifold.Convex )
					{
						ref var convexManifold = ref Unsafe.As<TManifold, ConvexContactManifold>( ref manifold );
						var normalUpDot = Vector3.Dot( convexManifold.Normal, up );
						//The narrow phase generates contacts with normals pointing from B to A by convention.
						//If the character is collidable B, then we need to negate the comparison.
						if ( (pair.B.Packed == characterCollidable.Packed ? -normalUpDot : normalUpDot) > character.CosMaximumSlope )
						{
							//This manifold has a slope that is potentially supportive.
							//Can the maximum depth contact be used as a support?
							var maximumDepth = convexManifold.Contact0.Depth;
							var maximumDepthIndex = 0;
							for ( int i = 1; i < convexManifold.Count; ++i )
							{
								ref var candidateDepth = ref Unsafe.Add( ref convexManifold.Contact0, i ).Depth;
								if ( candidateDepth > maximumDepth )
								{
									maximumDepth = candidateDepth;
									maximumDepthIndex = i;
								}
							}

							if ( maximumDepth >= character.MinimumSupportDepth ||
							     (character.Supported && maximumDepth > character.MinimumSupportContinuationDepth) )
							{
								ref var supportCandidate = ref mContactCollectionWorkerCaches[workerIndex].SupportCandidates[characterIndex];
								if ( supportCandidate.Depth < maximumDepth )
								{
									//This support candidate should be replaced.
									supportCandidate.Depth = maximumDepth;
									ref var deepestContact = ref Unsafe.Add( ref convexManifold.Contact0, maximumDepthIndex );
									var offsetFromB = deepestContact.Offset - convexManifold.OffsetB;
									if ( pair.B.Packed == characterCollidable.Packed )
									{
										supportCandidate.Normal = -convexManifold.Normal;
										supportCandidate.OffsetFromCharacter = offsetFromB;
										supportCandidate.OffsetFromSupport = deepestContact.Offset;
									}
									else
									{
										supportCandidate.Normal = convexManifold.Normal;
										supportCandidate.OffsetFromCharacter = deepestContact.Offset;
										supportCandidate.OffsetFromSupport = offsetFromB;
									}

									supportCandidate.Support = supportCollidable;
								}
							}
						}
					}
					else
					{
						ref var nonconvexManifold = ref Unsafe.As<TManifold, NonconvexContactManifold>( ref manifold );
						//The narrow phase generates contacts with normals pointing from B to A by convention.
						//If the character is collidable B, then we need to negate the comparison.
						//This manifold has a slope that is potentially supportive.
						//Can the maximum depth contact be used as a support?
						var maximumDepth = float.MinValue;
						var maximumDepthIndex = -1;
						for ( int i = 0; i < nonconvexManifold.Count; ++i )
						{
							ref var candidate = ref Unsafe.Add( ref nonconvexManifold.Contact0, i );
							if ( candidate.Depth > maximumDepth )
							{
								//All the nonconvex candidates can have different normals, so we have to perform the (calibrated) normal test on every single one.
								var upDot = Vector3.Dot( candidate.Normal, up );
								if ( (pair.B.Packed == characterCollidable.Packed ? -upDot : upDot) > character.CosMaximumSlope )
								{
									maximumDepth = candidate.Depth;
									maximumDepthIndex = i;
								}
							}
						}

						if ( maximumDepth >= character.MinimumSupportDepth ||
						     (character.Supported && maximumDepth > character.MinimumSupportContinuationDepth) )
						{
							ref var supportCandidate = ref mContactCollectionWorkerCaches[workerIndex].SupportCandidates[characterIndex];
							if ( supportCandidate.Depth < maximumDepth )
							{
								//This support candidate should be replaced.
								ref var deepestContact = ref Unsafe.Add( ref nonconvexManifold.Contact0, maximumDepthIndex );
								supportCandidate.Depth = maximumDepth;
								var offsetFromB = deepestContact.Offset - nonconvexManifold.OffsetB;
								if ( pair.B.Packed == characterCollidable.Packed )
								{
									supportCandidate.Normal = -deepestContact.Normal;
									supportCandidate.OffsetFromCharacter = offsetFromB;
									supportCandidate.OffsetFromSupport = deepestContact.Offset;
								}
								else
								{
									supportCandidate.Normal = deepestContact.Normal;
									supportCandidate.OffsetFromCharacter = deepestContact.Offset;
									supportCandidate.OffsetFromSupport = offsetFromB;
								}

								supportCandidate.Support = supportCollidable;
							}
						}
					}

					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Reports contacts about a collision to the character system.
		/// If the pair does not involve a character or there are no contacts, does nothing and returns false.
		/// </summary>
		/// <param name="pair">Pair of objects associated with the contact manifold.</param>
		/// <param name="manifold">Contact manifold between the colliding objects.</param>
		/// <param name="workerIndex">Index of the currently executing worker thread.</param>
		/// <param name="materialProperties">Material properties for this pair. Will be modified if the pair involves a character.</param>
		/// <returns>True if the pair involved a character pair and has contacts, false otherwise.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool TryReportContacts<TManifold>( in CollidablePair pair, ref TManifold manifold, int workerIndex,
			ref PairMaterialProperties materialProperties )
			where TManifold : struct, IContactManifold<TManifold>
		{
			if ( mChars.Count == 0 )
			{
				return true;
			}

			Debug.Assert(
				mContactCollectionWorkerCaches.Allocated && workerIndex < mContactCollectionWorkerCaches.Length &&
				mContactCollectionWorkerCaches[workerIndex].SupportCandidates.Allocated,
				"Worker caches weren't properly allocated; did you forget to call PrepareForContacts before collision detection?" );
			if ( manifold.Count == 0 )
				return false;
			//It's possible for neither, one, or both collidables to be a character. Check each one, treating the other as a potential support.
			var aIsCharacter = TryReportContacts( pair.A, pair.B, pair, ref manifold, workerIndex );
			var bIsCharacter = TryReportContacts( pair.B, pair.A, pair, ref manifold, workerIndex );
			if ( aIsCharacter || bIsCharacter )
			{
				//The character's motion over the surface should be controlled entirely by the horizontal motion constraint.
				//Note - you could use the friction coefficient to change the horizontal motion constraint's maximum force
				//to simulate different environments if you want.
				//That would just require caching a bit more information for the AnalyzeContacts function to use.
				materialProperties.FrictionCoefficient = 0;
				return true;
			}

			return false;
		}

		private void ExpandBoundingBoxes( int start, int count )
		{
			var end = start + count;
			for ( int i = start; i < end; ++i )
			{
				ref var character = ref Characters[i];
				var characterBody = mSim.Bodies[character.BodyHandle];
				if ( characterBody.Awake )
				{
					mSim.BroadPhase.GetActiveBoundsPointers( characterBody.Collidable.BroadPhaseIndex, out var min, out var max );
					QuaternionEx.Transform( character.LocalUp, characterBody.Pose.Orientation, out var characterUp );
					var supportExpansion = character.MinimumSupportContinuationDepth * characterUp;
					*min += Vector3.Min( Vector3.Zero, supportExpansion );
					*max += Vector3.Max( Vector3.Zero, supportExpansion );
				}
			}
		}

		private int mBoundingBoxExpansionJobIndex;
		private Action<int> mExpandBoundingBoxesWorker;

		private void ExpandBoundingBoxesWorker( int workerIndex )
		{
			while ( true )
			{
				var jobIndex = Interlocked.Increment( ref mBoundingBoxExpansionJobIndex );
				if ( jobIndex < mBoundingBoxExpansionJobs.Length )
				{
					ref var job = ref mBoundingBoxExpansionJobs[jobIndex];
					ExpandBoundingBoxes( job.Start, job.Count );
				}
				else
				{
					break;
				}
			}
		}

		/// <summary>
		/// Preallocates space for support data collected during the narrow phase. Should be called before the narrow phase executes.
		/// Also expands bounding boxes for all the characters.
		/// </summary>
		private void PrepareForContacts( float dt, IThreadDispatcher? threadDispatcher = null )
		{
			if ( mChars.Count == 0 )
			{
				return;
			}

			Debug.Assert( !mContactCollectionWorkerCaches.Allocated,
				"Worker caches were already allocated; did you forget to call AnalyzeContacts after collision detection to flush the previous frame's results?" );
			var threadCount = threadDispatcher?.ThreadCount ?? 1;
			mPool.Take( threadCount, out mContactCollectionWorkerCaches );
			for ( int i = 0; i < mContactCollectionWorkerCaches.Length; ++i )
			{
				mContactCollectionWorkerCaches[i] = new ContactCollectionWorkerCache( mChars.Count, mPool );
			}

			//While the character will retain support with contacts with depths above the MinimumSupportContinuationDepth if there was support in the previous frame,
			//it's possible for the contacts to be lost because the bounding box isn't expanded by MinimumSupportContinuationDepth and the broad phase doesn't see the support collidable.
			//Here, we expand the bounding boxes to compensate.
			if ( threadCount == 1 || mChars.Count < 256 )
			{
				ExpandBoundingBoxes( 0, mChars.Count );
			}
			else
			{
				var jobCount = Math.Min( mChars.Count, threadCount );
				var charactersPerJob = mChars.Count / jobCount;
				var baseCharacterCount = charactersPerJob * jobCount;
				var remainder = mChars.Count - baseCharacterCount;
				mPool.Take( jobCount, out mBoundingBoxExpansionJobs );
				var previousEnd = 0;
				for ( int jobIndex = 0; jobIndex < jobCount; ++jobIndex )
				{
					var charactersForJob = jobIndex < remainder ? charactersPerJob + 1 : charactersPerJob;
					ref var job = ref mBoundingBoxExpansionJobs[jobIndex];
					job.Start = previousEnd;
					job.Count = charactersForJob;
					previousEnd += job.Count;
				}

				mBoundingBoxExpansionJobIndex = -1;
				threadDispatcher.DispatchWorkers( mExpandBoundingBoxesWorker, mBoundingBoxExpansionJobs.Length );
				mPool.Return( ref mBoundingBoxExpansionJobs );
			}
		}

		#endregion

		#region Contact analysis

		private struct PendingDynamicConstraint
		{
			public int CharacterIndex;
			public DynamicCharacterMotionConstraint Description;
		}

		private struct PendingStaticConstraint
		{
			public int CharacterIndex;
			public StaticCharacterMotionConstraint Description;
		}

		private struct Jump
		{
			//Note that not every jump will contain a support body, so this can waste memory.
			//That's not really a concern - jumps are very rare (relatively speaking), so all we're wasting is capacity, not bandwidth.
			public int CharacterBodyIndex;
			public Vector3 CharacterVelocityChange;
			public int SupportBodyIndex;
			public Vector3 SupportImpulseOffset;
		}

		private struct AnalyzeContactsWorkerCache
		{
			//The solver does not permit multithreaded removals and additions. We handle all of them in a sequential postpass.
			public QuickList<ConstraintHandle> ConstraintHandlesToRemove;
			public QuickList<PendingDynamicConstraint> DynamicConstraintsToAdd;
			public QuickList<PendingStaticConstraint> StaticConstraintsToAdd;
			public QuickList<Jump> Jumps;

			public AnalyzeContactsWorkerCache( int maximumCharacterCount, BufferPool pool )
			{
				ConstraintHandlesToRemove = new QuickList<ConstraintHandle>( maximumCharacterCount, pool );
				DynamicConstraintsToAdd = new QuickList<PendingDynamicConstraint>( maximumCharacterCount, pool );
				StaticConstraintsToAdd = new QuickList<PendingStaticConstraint>( maximumCharacterCount, pool );
				Jumps = new QuickList<Jump>( maximumCharacterCount, pool );
			}

			public void Dispose( BufferPool pool )
			{
				ConstraintHandlesToRemove.Dispose( pool );
				DynamicConstraintsToAdd.Dispose( pool );
				StaticConstraintsToAdd.Dispose( pool );
				Jumps.Dispose( pool );
			}
		}

		private void AnalyzeContactsForCharacterRegion( int start, int exclusiveEnd, int workerIndex )
		{
			ref var analyzeContactsWorkerCache = ref mAnalyzeContactsWorkerCaches[workerIndex];
			for ( int characterIndex = start; characterIndex < exclusiveEnd; ++characterIndex )
			{
				//Note that this iterates over both active and inactive characters rather than segmenting inactive characters into their own collection.
				//This demands branching, but the expectation is that the vast majority of characters will be active, so there is less value in copying them into stasis.                
				ref var character = ref Characters[characterIndex];
				ref var bodyLocation = ref mSim.Bodies.HandleToLocation[character.BodyHandle.Value];
				if ( bodyLocation.SetIndex == 0 )
				{
					var supportCandidate = mContactCollectionWorkerCaches[0].SupportCandidates[characterIndex];
					for ( int j = 1; j < mContactCollectionWorkerCaches.Length; ++j )
					{
						ref var workerCandidate = ref mContactCollectionWorkerCaches[j].SupportCandidates[characterIndex];
						if ( workerCandidate.Depth > supportCandidate.Depth )
						{
							supportCandidate = workerCandidate;
						}
					}

					//We need to protect against one possible corner case: if the body supporting the character was removed, the associated motion constraint was also removed.
					//Arbitrarily un-support the character if we detect this.      
					if ( character.Supported )
					{
						//If the constraint no longer exists at all, 
						if ( !mSim.Solver.ConstraintExists( character.MotionConstraintHandle ) ||
						     //or if the constraint does exist but is now used by a different constraint type,
						     (mSim.Solver.HandleToConstraint[character.MotionConstraintHandle.Value].TypeId !=
						      DynamicCharacterMotionTypeProcessor.BatchTypeId &&
						      mSim.Solver.HandleToConstraint[character.MotionConstraintHandle.Value].TypeId !=
						      StaticCharacterMotionTypeProcessor.BatchTypeId) )
						{
							//then the character isn't actually supported anymore.
							character.Supported = false;
						}
						//Note that it's sufficient to only check that the type matches the dynamic motion constraint type id because no other systems ever create dynamic character motion constraints.
						//Other systems may result in the constraint's removal, but no other system will ever *create* it.
						//Further, during this analysis loop, we do not create any constraints. We only set up pending additions to be processed after the multithreaded analysis completes.
					}

					//The body is active. We may need to remove the associated constraint from the solver. Remove if any of the following hold:
					//1) The character was previously supported but is no longer.
					//2) The character was previously supported by a body, and is now supported by a different body.
					//3) The character was previously supported by a static, and is now supported by a body.
					//4) The character was previously supported by a body, and is now supported by a static.
					var shouldRemove = character.Supported && (character.TryJump || supportCandidate.Depth == float.MinValue ||
					                                           character.Support.Packed != supportCandidate.Support.Packed);
					if ( shouldRemove )
					{
						//Mark the constraint for removal.
						analyzeContactsWorkerCache.ConstraintHandlesToRemove.AllocateUnsafely() = character.MotionConstraintHandle;
					}

					//If the character is jumping, don't create a constraint.
					if ( supportCandidate.Depth > float.MinValue && character.TryJump )
					{
						QuaternionEx.Transform( character.LocalUp, mSim.Bodies.ActiveSet.DynamicsState[bodyLocation.Index].Motion.Pose.Orientation,
							out var characterUp );
						//Note that we assume that character orientations are constant. This isn't necessarily the case in all uses, but it's a decent approximation.
						var characterUpVelocity = Vector3.Dot( mSim.Bodies.ActiveSet.DynamicsState[bodyLocation.Index].Motion.Velocity.Linear,
							characterUp );
						//We don't want the character to be able to 'superboost' by simply adding jump speed on top of horizontal motion.
						//Instead, jumping targets a velocity change necessary to reach character.JumpVelocity along the up axis.
						if ( character.Support.Mobility != CollidableMobility.Static )
						{
							ref var supportingBodyLocation = ref mSim.Bodies.HandleToLocation[character.Support.BodyHandle.Value];
							Debug.Assert( supportingBodyLocation.SetIndex == 0, "If the character is active, any support should be too." );
							ref var supportVelocity = ref mSim.Bodies.ActiveSet.DynamicsState[supportingBodyLocation.Index].Motion.Velocity;
							var wxr = Vector3.Cross( supportVelocity.Angular, supportCandidate.OffsetFromSupport );
							var supportContactVelocity = supportVelocity.Linear + wxr;
							var supportUpVelocity = Vector3.Dot( supportContactVelocity, characterUp );

							//If the support is dynamic, apply an opposing impulse. Note that velocity changes cannot safely be applied during multithreaded execution;
							//characters could share support bodies, and a character might be a support of another character.
							//That's really not concerning from a performance perspective- characters don't jump many times per frame.
							ref var jump = ref analyzeContactsWorkerCache.Jumps.AllocateUnsafely();
							jump.CharacterBodyIndex = bodyLocation.Index;
							jump.CharacterVelocityChange = characterUp * MathF.Max( 0, character.JumpVelocity - (characterUpVelocity - supportUpVelocity) );
							if ( character.Support.Mobility == CollidableMobility.Dynamic )
							{
								jump.SupportBodyIndex = supportingBodyLocation.Index;
								jump.SupportImpulseOffset = supportCandidate.OffsetFromSupport;
							}
							else
							{
								//No point in applying impulses to kinematics.
								jump.SupportBodyIndex = -1;
							}
						}
						else
						{
							//Static bodies have no velocity, so we don't have to consider the support.
							ref var jump = ref analyzeContactsWorkerCache.Jumps.AllocateUnsafely();
							jump.CharacterBodyIndex = bodyLocation.Index;
							jump.CharacterVelocityChange = characterUp * MathF.Max( 0, character.JumpVelocity - characterUpVelocity );
							jump.SupportBodyIndex = -1;
						}

						character.Supported = false;
					}
					else if ( supportCandidate.Depth > float.MinValue )
					{
						//If a support currently exists and there is still an old constraint, then update it.
						//If a support currently exists and there is not an old constraint, add the new constraint.

						//Project the view direction down onto the surface as represented by the contact normal.
						Matrix3x3 surfaceBasis;
						surfaceBasis.Y = supportCandidate.Normal;
						//Note negation: we're using a right handed basis where -Z is forward, +Z is backward.
						QuaternionEx.Transform( character.LocalUp, mSim.Bodies.ActiveSet.DynamicsState[bodyLocation.Index].Motion.Pose.Orientation,
							out var up );
						var rayDistance = Vector3.Dot( character.ViewDirection, surfaceBasis.Y );
						var rayVelocity = Vector3.Dot( up, surfaceBasis.Y );
						Debug.Assert( rayVelocity > 0,
							"The calibrated support normal and the character's up direction should have a positive dot product if the maximum slope is working properly. Is the maximum slope >= pi/2?" );
						surfaceBasis.Z = up * (rayDistance / rayVelocity) - character.ViewDirection;
						var zLengthSquared = surfaceBasis.Z.LengthSquared();
						if ( zLengthSquared > 1e-12f )
						{
							surfaceBasis.Z /= MathF.Sqrt( zLengthSquared );
						}
						else
						{
							QuaternionEx.GetQuaternionBetweenNormalizedVectors( Vector3.UnitY, surfaceBasis.Y, out var rotation );
							QuaternionEx.TransformUnitZ( rotation, out surfaceBasis.Z );
						}

						surfaceBasis.X = Vector3.Cross( surfaceBasis.Y, surfaceBasis.Z );
						QuaternionEx.CreateFromRotationMatrix( surfaceBasis, out var surfaceBasisQuaternion );
						if ( supportCandidate.Support.Mobility != CollidableMobility.Static )
						{
							//The character is supported by a body.
							var motionConstraint = new DynamicCharacterMotionConstraint
							{
								MaximumHorizontalForce = character.MaximumHorizontalForce,
								MaximumVerticalForce = character.MaximumVerticalForce,
								OffsetFromCharacterToSupportPoint = supportCandidate.OffsetFromCharacter,
								OffsetFromSupportToSupportPoint = supportCandidate.OffsetFromSupport,
								SurfaceBasis = surfaceBasisQuaternion,
								TargetVelocity = character.TargetVelocity,
								Depth = supportCandidate.Depth
							};
							if ( character.Supported && !shouldRemove )
							{
								//Already exists, update it.
								mSim.Solver.ApplyDescriptionWithoutWaking( character.MotionConstraintHandle, motionConstraint );
							}
							else
							{
								//Doesn't exist, mark it for addition.
								ref var pendingConstraint = ref analyzeContactsWorkerCache.DynamicConstraintsToAdd.AllocateUnsafely();
								pendingConstraint.Description = motionConstraint;
								pendingConstraint.CharacterIndex = characterIndex;
							}
						}
						else
						{
							//The character is supported by a static.
							var motionConstraint = new StaticCharacterMotionConstraint
							{
								MaximumHorizontalForce = character.MaximumHorizontalForce,
								MaximumVerticalForce = character.MaximumVerticalForce,
								OffsetFromCharacterToSupportPoint = supportCandidate.OffsetFromCharacter,
								SurfaceBasis = surfaceBasisQuaternion,
								TargetVelocity = character.TargetVelocity,
								Depth = supportCandidate.Depth
							};
							if ( character.Supported && !shouldRemove )
							{
								//Already exists, update it.
								mSim.Solver.ApplyDescriptionWithoutWaking( character.MotionConstraintHandle, motionConstraint );
							}
							else
							{
								//Doesn't exist, mark it for addition.
								ref var pendingConstraint = ref analyzeContactsWorkerCache.StaticConstraintsToAdd.AllocateUnsafely();
								pendingConstraint.Description = motionConstraint;
								pendingConstraint.CharacterIndex = characterIndex;
							}
						}

						character.Supported = true;
						character.Support = supportCandidate.Support;
					}
					else
					{
						character.Supported = false;
					}
				}

				//The TryJump flag is always reset even if the attempt failed.
				character.TryJump = false;
			}
		}

		private struct AnalyzeContactsJob
		{
			public int Start;
			public int ExclusiveEnd;
		}

		private int mAnalysisJobIndex;
		private int mAnalysisJobCount;
		private Buffer<AnalyzeContactsJob> mJobs;
		private readonly Action<int> mAnalyzeContactsWorker;

		private void AnalyzeContactsWorker( int workerIndex )
		{
			int jobIndex;
			while ( (jobIndex = Interlocked.Increment( ref mAnalysisJobIndex )) < mAnalysisJobCount )
			{
				ref var job = ref mJobs[jobIndex];
				AnalyzeContactsForCharacterRegion( job.Start, job.ExclusiveEnd, workerIndex );
			}
		}

		/// <summary>
		/// Updates all character support states and motion constraints based on the current character goals and all the contacts collected since the
		/// last call to AnalyzeContacts. Attach to a simulation callback where the most recent contact is available and before the solver executes.
		/// </summary>
		private void AnalyzeContacts( float dt, IThreadDispatcher? threadDispatcher )
		{
			if ( mChars.Count == 0 )
			{
				return;
			}

			Debug.Assert( mContactCollectionWorkerCaches.Allocated,
				"Worker caches weren't properly allocated; did you forget to call PrepareForContacts before collision detection?" );

			if ( threadDispatcher == null )
			{
				mPool.Take( 1, out mAnalyzeContactsWorkerCaches );
				mAnalyzeContactsWorkerCaches[0] = new AnalyzeContactsWorkerCache( mChars.Count, mPool );
				AnalyzeContactsForCharacterRegion( 0, mChars.Count, 0 );
			}
			else
			{
				mAnalysisJobCount = Math.Min( mChars.Count, threadDispatcher.ThreadCount * 4 );
				if ( mAnalysisJobCount > 0 )
				{
					mPool.Take( threadDispatcher.ThreadCount, out mAnalyzeContactsWorkerCaches );
					mPool.Take( mAnalysisJobCount, out mJobs );
					for ( int i = 0; i < threadDispatcher.ThreadCount; ++i )
					{
						mAnalyzeContactsWorkerCaches[i] = new AnalyzeContactsWorkerCache( mChars.Count, mPool );
					}

					var baseCount = mChars.Count / mAnalysisJobCount;
					var remainder = mChars.Count - baseCount * mAnalysisJobCount;
					var previousEnd = 0;
					for ( int i = 0; i < mAnalysisJobCount; ++i )
					{
						ref var job = ref mJobs[i];
						job.Start = previousEnd;
						job.ExclusiveEnd = job.Start + (i < remainder ? baseCount + 1 : baseCount);
						previousEnd = job.ExclusiveEnd;
					}

					mAnalysisJobIndex = -1;
					threadDispatcher.DispatchWorkers( mAnalyzeContactsWorker, mAnalysisJobCount );
					mPool.Return( ref mJobs );
				}
			}

			//We're done with all the contact collection worker caches.
			for ( int i = 0; i < mContactCollectionWorkerCaches.Length; ++i )
			{
				mContactCollectionWorkerCaches[i].Dispose( mPool );
			}

			mPool.Return( ref mContactCollectionWorkerCaches );

			if ( mAnalyzeContactsWorkerCaches.Allocated )
			{
				//Flush all the worker caches. Note that we perform all removals before moving onto any additions to avoid unnecessary constraint batches
				//caused by the new and old constraint affecting the same bodies.
				for ( int threadIndex = 0; threadIndex < mAnalyzeContactsWorkerCaches.Length; ++threadIndex )
				{
					ref var cache = ref mAnalyzeContactsWorkerCaches[threadIndex];
					for ( int i = 0; i < cache.ConstraintHandlesToRemove.Count; ++i )
					{
						mSim.Solver.Remove( cache.ConstraintHandlesToRemove[i] );
					}
				}

				for ( int threadIndex = 0; threadIndex < mAnalyzeContactsWorkerCaches.Length; ++threadIndex )
				{
					ref var workerCache = ref mAnalyzeContactsWorkerCaches[threadIndex];
					for ( int i = 0; i < workerCache.StaticConstraintsToAdd.Count; ++i )
					{
						ref var pendingConstraint = ref workerCache.StaticConstraintsToAdd[i];
						ref var character = ref Characters[pendingConstraint.CharacterIndex];
						Debug.Assert( character.Support.Mobility == CollidableMobility.Static );
						character.MotionConstraintHandle = mSim.Solver.Add( character.BodyHandle, pendingConstraint.Description );
					}

					for ( int i = 0; i < workerCache.DynamicConstraintsToAdd.Count; ++i )
					{
						ref var pendingConstraint = ref workerCache.DynamicConstraintsToAdd[i];
						ref var character = ref Characters[pendingConstraint.CharacterIndex];
						Debug.Assert( character.Support.Mobility != CollidableMobility.Static );
						character.MotionConstraintHandle
							= mSim.Solver.Add( character.BodyHandle, character.Support.BodyHandle, pendingConstraint.Description );
					}

					ref var activeSet = ref mSim.Bodies.ActiveSet;
					for ( int i = 0; i < workerCache.Jumps.Count; ++i )
					{
						ref var jump = ref workerCache.Jumps[i];
						activeSet.DynamicsState[jump.CharacterBodyIndex].Motion.Velocity.Linear += jump.CharacterVelocityChange;
						if ( jump.SupportBodyIndex >= 0 )
						{
							BodyReference.ApplyImpulse( mSim.Bodies.ActiveSet, jump.SupportBodyIndex,
								jump.CharacterVelocityChange / -activeSet.DynamicsState[jump.CharacterBodyIndex].Inertia.Local.InverseMass,
								jump.SupportImpulseOffset );
						}
					}

					workerCache.Dispose( mPool );
				}

				mPool.Return( ref mAnalyzeContactsWorkerCaches );
			}
		}

		#endregion

		#region Character allocation etc

		public int GetCharacterIndex( BodyHandle body )
		{
			Debug.Assert( body.Value >= 0 && body.Value < mBodyToCharIndex.Length,
				"Body handle out of range" );

			return mBodyToCharIndex[body.Value];
		}

		public ref CharacterController GetCharacter( int index )
		{
			return ref Characters[index];
		}

		public ref CharacterController GetCharacter( BodyHandle body )
		{
			Debug.Assert( body.Value >= 0 && body.Value < mBodyToCharIndex.Length,
				"Body handle out of range" );

			int handle = mBodyToCharIndex[body.Value];
			return ref Characters[handle];
		}

		public ref CharacterController CreateCharacter( BodyHandle body )
		{
			Debug.Assert( body.Value >= 0, "Body handle out of range" );

			// Resize if needed
			if ( body.Value >= mBodyToCharIndex.Length )
			{
				// TODO: move this to an extension
				int newSize = mBodyToCharIndex.Length * 2;
				while ( newSize <= body.Value )
				{
					newSize *= 2;
				}

				Array.Resize( ref mBodyToCharIndex, newSize );
			}

			Debug.Assert( mBodyToCharIndex[body.Value] is -1, "Body handle already associated with character" );

			int index = mChars.Count;
			mBodyToCharIndex[body.Value] = index;
			mChars.Add( default );
			// Important: set a body handle for the character
			Characters[index].BodyHandle = body;
			return ref Characters[index];
		}

		public void RemoveCharacter( int index )
		{
			if ( index < 0 || index >= mChars.Count )
			{
				return;
			}

			ref int bodyHandle = ref Characters[index].BodyHandle.Value;
			mBodyToCharIndex[bodyHandle] = -1;
			mChars.RemoveAt( index );
		}

		public void RemoveCharacter( BodyHandle body )
		{
			if ( body.Value < 0 || body.Value >= mBodyToCharIndex.Length )
			{
				return;
			}

			RemoveCharacter( mBodyToCharIndex[body.Value] );
		}

		#endregion

		public void Dispose()
		{
			mSim.Timestepper.BeforeCollisionDetection -= PrepareForContacts;
			mSim.Timestepper.CollisionsDetected -= AnalyzeContacts;
			mChars = [];
			mBodyToCharIndex = [];
		}
	}
}
