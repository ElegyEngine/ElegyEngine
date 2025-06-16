using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.OpenXR;
using Silk.NET.OpenXR.Extensions.HTCX;
using XrAction = Silk.NET.OpenXR.Action;

namespace XrResearch
{
	public static partial class Program
	{
		private static ActionSet mActionSet;

		private static XrAction mHandPoseAction;
		private static XrAction mTriggerAction;
		private static XrAction mGrabAction;
		private static XrAction mButtonPrimaryAction;
		private static XrAction mButtonSecondaryAction;
		private static XrAction mJoystickAction;

		private static ulong mLeftHandPath;
		private static ulong mRightHandPath;
		
		private static Space mLeftHandPoseSpace;
		private static Space mRightHandPoseSpace;
		
		private static Posef mLeftHandPose;
		private static Posef mRightHandPose;
		
		private static ActionStatePose mLeftHandPoseState = new() { Type = StructureType.ActionStatePose };
		private static ActionStatePose mRightHandPoseState = new() { Type = StructureType.ActionStatePose };
		private static ActionStateFloat mLeftTriggerState = new() { Type = StructureType.ActionStateFloat };
		private static ActionStateFloat mRightTriggerState = new() { Type = StructureType.ActionStateFloat };
		private static ActionStateFloat mLeftGrabState = new() { Type = StructureType.ActionStateFloat };
		private static ActionStateFloat mRightGrabState = new() { Type = StructureType.ActionStateFloat };
		private static ActionStateBoolean mButtonAState = new() { Type = StructureType.ActionStateBoolean };
		private static ActionStateBoolean mButtonBState = new() { Type = StructureType.ActionStateBoolean };
		private static ActionStateBoolean mButtonXState = new() { Type = StructureType.ActionStateBoolean };
		private static ActionStateBoolean mButtonYState = new() { Type = StructureType.ActionStateBoolean };
		private static ActionStateVector2f mLeftJoystickState = new() { Type = StructureType.ActionStateVector2f };
		private static ActionStateVector2f mRightJoystickState = new() { Type = StructureType.ActionStateVector2f };

		public static ulong StringToXrPath( string text )
		{
			ulong result = 0;
			XrCheck( Xr.StringToPath( mInstance, text, ref result ) );
			return result;
		}

		public static unsafe string XrPathToString( ulong pathId )
		{
			string result = string.Empty;
			IntPtr text = Marshal.AllocHGlobal( (int)XR.MaxPathLength );
			Span<byte> textSpan = new( text.ToPointer(), (int)XR.MaxPathLength );

			uint outputLength = 0;
			if ( XrCheck( Xr.PathToString( mInstance, pathId, ref outputLength, textSpan ) ) )
			{
				result = Encoding.UTF8.GetString( textSpan.Slice( 0, (int)outputLength ) );
			}

			Marshal.FreeHGlobal( text );
			return result;
		}

		public static unsafe void CreateActions()
		{
			Console.WriteLine( "CreateActions" );
			
			ActionSetCreateInfo actionSetCreateInfo = new() { Type = StructureType.ActionSetCreateInfo };
			actionSetCreateInfo.Priority = 0;
			"xr-research-actionset".CStringCopyTo( actionSetCreateInfo.ActionSetName, XR.MaxActionSetNameSize );
			"XrResearch Action Set".CStringCopyTo( actionSetCreateInfo.LocalizedActionSetName, XR.MaxLocalizedActionSetNameSize );

			XrCheck( Xr.CreateActionSet( mInstance, actionSetCreateInfo, ref mActionSet ), "Failed to create action set" );

			void CreateAction( ref XrAction action, string name, ActionType actionType, params string[] subActionPaths )
			{
				ulong[] subActionPathIds = new ulong[subActionPaths.Length];
				for ( int i = 0; i < subActionPaths.Length; i++ )
				{
					subActionPathIds[i] = StringToXrPath( subActionPaths[i] );
				}

				ActionCreateInfo actionCreateInfo = new()
				{
					Type = StructureType.ActionCreateInfo,
					ActionType = actionType,
					CountSubactionPaths = (uint)subActionPaths.Length,
					SubactionPaths = subActionPathIds.Deref()
				};

				name.CStringCopyTo( actionCreateInfo.ActionName, XR.MaxActionNameSize );
				name.CStringCopyTo( actionCreateInfo.LocalizedActionName, XR.MaxLocalizedActionNameSize );

				XrCheck( Xr.CreateAction( mActionSet, actionCreateInfo, ref action ) );
			}

			CreateAction( ref mGrabAction, "grab", ActionType.FloatInput, "/user/hand/left", "/user/hand/right" );
			CreateAction( ref mTriggerAction, "trigger", ActionType.FloatInput, "/user/hand/left", "/user/hand/right" );
			CreateAction( ref mButtonPrimaryAction, "primary", ActionType.BooleanInput, "/user/hand/left", "/user/hand/right" );
			CreateAction( ref mButtonSecondaryAction, "secondary", ActionType.BooleanInput, "/user/hand/left", "/user/hand/right" );
			CreateAction( ref mHandPoseAction, "hand-pose", ActionType.PoseInput, "/user/hand/left", "/user/hand/right" );
			CreateAction( ref mJoystickAction, "joy", ActionType.Vector2fInput, "/user/hand/left", "/user/hand/right" );

			mLeftHandPath = StringToXrPath( "/user/hand/left" );
			mRightHandPath = StringToXrPath( "/user/hand/right" );
		}

		public static unsafe bool SuggestActionBindings()
		{
			Console.WriteLine( "SuggestActionBindings" );
			
			ActionSuggestedBinding Bind( XrAction action, string path )
			{
				return new()
				{
					Action = action,
					Binding = StringToXrPath( path )
				};
			}

			bool Suggest( string profilePath, params ActionSuggestedBinding[] suggestions )
			{
				InteractionProfileSuggestedBinding suggestedBinding = new()
				{
					Type = StructureType.InteractionProfileSuggestedBinding,
					InteractionProfile = StringToXrPath( profilePath ),
					CountSuggestedBindings = (uint)suggestions.Length,
					SuggestedBindings = suggestions.Deref()
				};

				if ( !XrCheck( Xr.SuggestInteractionProfileBinding( mInstance, suggestedBinding ) ) )
				{
					Console.WriteLine( $"Failed to suggest bindings for profile '{profilePath}'" );
					return false;
				}

				return true;
			}

			bool allOkay = Suggest( "/interaction_profiles/khr/simple_controller",
				Bind( mHandPoseAction, "/user/hand/left/input/grip/pose" ),
				Bind( mHandPoseAction, "/user/hand/right/input/grip/pose" ),
				Bind( mButtonPrimaryAction, "/user/hand/left/input/select/click" ),
				Bind( mButtonPrimaryAction, "/user/hand/right/input/select/click" )
			);

			allOkay |= Suggest( "/interaction_profiles/oculus/touch_controller",
				Bind( mHandPoseAction, "/user/hand/left/input/grip/pose" ),
				Bind( mHandPoseAction, "/user/hand/right/input/grip/pose" ),
				Bind( mTriggerAction, "/user/hand/left/input/trigger/value" ),
				Bind( mTriggerAction, "/user/hand/right/input/trigger/value" ),
				Bind( mGrabAction, "/user/hand/left/input/squeeze/value" ),
				Bind( mGrabAction, "/user/hand/right/input/squeeze/value" ),
				Bind( mButtonPrimaryAction, "/user/hand/left/input/x/click" ),
				Bind( mButtonPrimaryAction, "/user/hand/right/input/a/click" ),
				Bind( mButtonSecondaryAction, "/user/hand/left/input/y/click" ),
				Bind( mButtonSecondaryAction, "/user/hand/right/input/b/click" ),
				Bind( mJoystickAction, "/user/hand/left/input/thumbstick" ),
				Bind( mJoystickAction, "/user/hand/right/input/thumbstick" )
			);

			return allOkay;
		}

		public static unsafe void CreateActionPoses()
		{
			Console.WriteLine( "CreateActionPoses" );
			
			Space CreateSpace( XrAction action, string? subactionPath = null )
			{
				ActionSpaceCreateInfo spaceCreateInfo = new()
				{
					Type = StructureType.ActionSpaceCreateInfo,
					Action = action,
					SubactionPath = subactionPath is null ? XR.NullPath : StringToXrPath( subactionPath ),
					PoseInActionSpace = new() { Position = new( 0, 0, 0 ), Orientation = new( 0, 0, 0, 1 ) }
				};

				Space result = new();
				XrCheck( Xr.CreateActionSpace( mSession, spaceCreateInfo, ref result ) );
				return result;
			}

			mLeftHandPoseSpace = CreateSpace( mHandPoseAction, "/user/hand/left" );
			mRightHandPoseSpace = CreateSpace( mHandPoseAction, "/user/hand/right" );
		}

		public static unsafe void AttachActionSets( params ActionSet[] actionSets )
		{
			Console.WriteLine( "AttachActionSets" );
			
			SessionActionSetsAttachInfo attachInfo = new()
			{
				Type = StructureType.SessionActionSetsAttachInfo,
				CountActionSets = (uint)actionSets.Length,
				ActionSets = actionSets.Deref()
			};

			XrCheck( Xr.AttachSessionActionSets( mSession, attachInfo ) );
		}

		public static void RecordCurrentBindings()
		{
			void Record( ulong path )
			{
				InteractionProfileState interactionProfileState = new() { Type = StructureType.InteractionProfileState };

				XrCheck( Xr.GetCurrentInteractionProfile( mSession, path, ref interactionProfileState ) );
				if ( interactionProfileState.InteractionProfile is not XR.NullPath )
				{
					Console.WriteLine(
						$"'{XrPathToString( path )}' active profile -> {XrPathToString( interactionProfileState.InteractionProfile )}" );
				}
				else
				{
					Console.WriteLine( $"'{XrPathToString( path )}' active profile -> none" );
				}
			}

			Record( mLeftHandPath );
			Record( mRightHandPath );
		}

		public static unsafe void PollSystemEvents()
		{
		}

		public static unsafe void PollEvents()
		{
			EventDataBuffer eventData = new() { Type = StructureType.EventDataBuffer };

			bool XrPollEvents()
			{
				eventData.Type = StructureType.EventDataBuffer;
				return Xr.PollEvent( mInstance, ref eventData ) == Result.Success;
			}

			while ( XrPollEvents() )
			{
				switch ( eventData.Type )
				{
					case StructureType.EventDataEventsLost:
					{
						EventDataEventsLost* eventsLostData = (EventDataEventsLost*)Unsafe.AsPointer( ref eventData );
						Console.WriteLine( $"OpenXR: Events lost - {eventsLostData->LostEventCount}" );
						break;
					}

					case StructureType.EventDataInstanceLossPending:
					{
						EventDataInstanceLossPending* lossPendingData =
							(EventDataInstanceLossPending*)Unsafe.AsPointer( ref eventData );
						Console.WriteLine( $"OpenXR: Instance loss pending at: {lossPendingData->LossTime}" );
						mIsRunning = false;
						mSessionIsRunning = false;
						break;
					}

					case StructureType.EventDataInteractionProfileChanged:
					{
						EventDataInteractionProfileChanged* profileChangedData =
							(EventDataInteractionProfileChanged*)Unsafe.AsPointer( ref eventData );
						Console.WriteLine( $"OpenXR: Interaction profile changed -> session {profileChangedData->Session.Handle}" );
						if ( profileChangedData->Session.Handle != mSession.Handle )
						{
							Console.WriteLine( "OpenXR: Interaction profile changed for unknown session!" );
						}

						RecordCurrentBindings();

						break;
					}

					case StructureType.EventDataReferenceSpaceChangePending:
					{
						EventDataReferenceSpaceChangePending* spaceChangeData =
							(EventDataReferenceSpaceChangePending*)Unsafe.AsPointer( ref eventData );
						Console.WriteLine( $"OpenXR: Ref. space change pending -> session {spaceChangeData->Session.Handle}" );
						if ( spaceChangeData->Session.Handle != mSession.Handle )
						{
							Console.WriteLine( "OpenXR: Ref. space changing for unknown session!" );
						}

						break;
					}

					case StructureType.EventDataSessionStateChanged:
					{
						EventDataSessionStateChanged* sessionStateChangedData =
							(EventDataSessionStateChanged*)Unsafe.AsPointer( ref eventData );
						if ( sessionStateChangedData->Session.Handle != mSession.Handle )
						{
							Console.WriteLine( $"OpenXR: Session state changed for unknown session!" );
							break;
						}

						mSessionState = sessionStateChangedData->State;
						switch ( mSessionState )
						{
							case SessionState.Ready:
							{
								SessionBeginInfo beginInfo = new()
								{
									Type = StructureType.SessionBeginInfo,
									PrimaryViewConfigurationType = ViewConfigurationType.PrimaryStereo
								};

								XrCheck( Xr.BeginSession( mSession, beginInfo ), "Failed to begin session" );
								mSessionIsRunning = true;
								break;
							}

							case SessionState.Stopping:
							{
								XrCheck( Xr.EndSession( mSession ), "Failed to end session" );
								mSessionIsRunning = false;
								break;
							}

							case SessionState.LossPending:
							case SessionState.Exiting:
							{
								mSessionIsRunning = false;
								mIsRunning = false;
								break;
							}
						}

						Console.WriteLine( $"OpenXR: New session state -> {mSessionState}" );
						break;
					}
				}
			}
		}

		public static unsafe void PollActions( long predictedTime )
		{
			ActiveActionSet activeActionSet = new()
			{
				ActionSet = mActionSet,
				SubactionPath = XR.NullPath
			};

			ActionsSyncInfo actionsSyncInfo = new()
			{
				Type = StructureType.ActionsSyncInfo,
				CountActiveActionSets = 1,
				ActiveActionSets = &activeActionSet
			};

			if ( !XrCheck( Xr.SyncAction( mSession, actionsSyncInfo ), "Failed to sync actions", true ) )
			{
				return;
			}

			void GetPoseState( XrAction action, Space space, ref Posef pose, ref ActionStatePose state, ulong subactionPath = XR.NullPath )
			{
				ActionStateGetInfo getInfo = new()
				{
					Type = StructureType.ActionStateGetInfo,
					Action = action,
					SubactionPath = subactionPath,
				};

				XrCheck( Xr.GetActionStatePose( mSession, getInfo, ref state ) );
				if ( state.IsActive != 0 )
				{
					SpaceLocation spaceLocation = new() { Type = StructureType.SpaceLocation };
					Result result = Xr.LocateSpace( space, mReferenceSpace, predictedTime, ref spaceLocation );

					if ( result is Result.Success
						 && spaceLocation.LocationFlags.HasFlag( SpaceLocationFlags.OrientationValidBit )
						 && spaceLocation.LocationFlags.HasFlag( SpaceLocationFlags.PositionValidBit ) )
					{
						pose = spaceLocation.Pose;
					}
					else
					{
						state.IsActive = 0;
					}
				}
			}

			void GetFloatState( XrAction action, ref ActionStateFloat state, ulong subactionPath = XR.NullPath )
			{
				ActionStateGetInfo getInfo = new()
				{
					Type = StructureType.ActionStateGetInfo,
					Action = action,
					SubactionPath = subactionPath,
				};

				XrCheck( Xr.GetActionStateFloat( mSession, getInfo, ref state ) );
			}

			void GetBooleanState( XrAction action, ref ActionStateBoolean state, ulong subactionPath = XR.NullPath )
			{
				ActionStateGetInfo getInfo = new()
				{
					Type = StructureType.ActionStateGetInfo,
					Action = action,
					SubactionPath = subactionPath,
				};

				XrCheck( Xr.GetActionStateBoolean( mSession, getInfo, ref state ) );
			}

			void GetVectorState( XrAction action, ref ActionStateVector2f state, ulong subactionPath = XR.NullPath )
			{
				ActionStateGetInfo getInfo = new()
				{
					Type = StructureType.ActionStateGetInfo,
					Action = action,
					SubactionPath = subactionPath,
				};

				XrCheck( Xr.GetActionStateVector2( mSession, getInfo, ref state ) );
			}

			GetPoseState( mHandPoseAction, mLeftHandPoseSpace, ref mLeftHandPose, ref mLeftHandPoseState, mLeftHandPath );
			GetPoseState( mHandPoseAction, mRightHandPoseSpace, ref mRightHandPose, ref mRightHandPoseState, mRightHandPath );

			GetFloatState( mTriggerAction, ref mLeftTriggerState, mLeftHandPath );
			GetFloatState( mTriggerAction, ref mRightTriggerState, mRightHandPath );
			GetFloatState( mGrabAction, ref mLeftGrabState, mLeftHandPath );
			GetFloatState( mGrabAction, ref mRightGrabState, mRightHandPath );

			GetBooleanState( mButtonPrimaryAction, ref mButtonXState, mLeftHandPath );
			GetBooleanState( mButtonPrimaryAction, ref mButtonAState, mRightHandPath );
			GetBooleanState( mButtonSecondaryAction, ref mButtonYState, mLeftHandPath );
			GetBooleanState( mButtonSecondaryAction, ref mButtonBState, mRightHandPath );
		}

		public static void InitInput()
		{
			Console.WriteLine( "InitInput" );
			
			// Actions = essentially the VR equivalent of keybinds
			CreateActions();
			SuggestActionBindings();
		}

		public static void PostInitInput()
		{
			Console.WriteLine( "PostInitInput" );
			
			CreateActionPoses();

			// Once the session is created, we can attach
			// an action set or two
			AttachActionSets( mActionSet );
		}

		public static void AddInputExtensions()
		{
			mInstanceExtensions.Add( HtcxViveTrackerInteraction.ExtensionName );
		}

		public static void ShutdownInput()
		{
			Console.WriteLine( "ShutdownInput" );
			
			XrCheck( Xr.DestroySpace( mLeftHandPoseSpace ) );
			XrCheck( Xr.DestroySpace( mRightHandPoseSpace ) );
			XrCheck( Xr.DestroyAction( mGrabAction ) );
			XrCheck( Xr.DestroyAction( mTriggerAction ) );
			XrCheck( Xr.DestroyAction( mButtonPrimaryAction ) );
			XrCheck( Xr.DestroyAction( mButtonSecondaryAction ) );
			XrCheck( Xr.DestroyAction( mHandPoseAction ) );
			XrCheck( Xr.DestroyActionSet( mActionSet ) );
		}

		public static void LoopInput()
		{
			PollSystemEvents();
			PollEvents();
		}
	}
}
