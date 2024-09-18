// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.API;
using Elegy.Common.Assets;
using Elegy.Common.Interfaces;
using Elegy.ConsoleSystem;
using Elegy.FileSystem.API;

using Game.Client;
using Game.Presentation;
using Game.Server;
using Game.Session;
using Game.Shared.Bridges;

using System.Diagnostics;
using System.Net;

namespace Game
{
	public class Game : IApplication
	{
		private TaggedLogger mLogger = new( "Game" );
		private Stopwatch mStopwatch = new();

		private GameClient? mClient; // User input handling, main menu etc.
		private GamePresentation? mPresentation; // Rendering, HUD etc.
		private GameSession? mSession; // Clientside game state
		private GameServer? mServer; // Serverside entities etc.
		private ISessionBridge? mSessionBridge; // Bridge between server and session
		// Clientside and/or serverside game entities. In singleplayer, it is shared
		// between GameSession and GameServer, in multiplayer it is only in GameSession.
		private EntityWorld mEntityWorld = new();

		private bool mUserWantsToExit = false;

		public string Name => "Elegy test game";
		public string Error { get; private set; } = string.Empty;
		public bool Initialised { get; private set; } = false;

		public Game()
		{
		}

		public bool Init()
		{
			mLogger.Log( "Init" );
			Initialised = true;

			ApplicationConfig gameConfig = Files.CurrentConfig;

			mLogger.Log( $" Name:      {gameConfig.Title}" );
			mLogger.Log( $" Developer: {gameConfig.Developer}" );
			mLogger.Log( $" Publisher: {gameConfig.Publisher}" );
			mLogger.Log( $" Version:   {gameConfig.Version}" );

			return true;
		}

		public bool Start()
		{
			mLogger.Log( "Start" );
			mStopwatch.Restart();

			bool headlessMode = Console.Arguments.ContainsKey( "-server" );
			if ( headlessMode )
			{
				return StartDedicatedServer();
			}

			if ( !StartClient() )
			{
				return false;
			}

			// TODO: since we don't have any UI yet, we'll force start an SP server
			if ( !StartSingleplayerServer() || mServer is null )
			{
				return false;
			}

			return StartLevel( "test2" );
		}

		public void Shutdown()
		{
			mLogger.Log( "Shutdown" );

			mSession?.Shutdown();
			mSession = null;

			mPresentation?.Shutdown();
			mPresentation = null;

			mClient?.Shutdown();
			mClient = null;

			mServer?.Shutdown();
			mServer = null;
		}

		public bool RunFrame( float delta )
		{
			// Update input, main menu etc.
			mClient?.Update( delta );

			// Send client state to server
			//mSession?.SendClientState( delta );

			// Entity simulation on the server, sending data to clients etc.
			mServer?.Update( delta );

			// Receive entity state from server, predict & simulate clientside entities
			//mSession?.Update( delta );

			// Update camera, HUD etc. Rendering is handled elsewhere
			//mPresentation?.Update( delta );

			return !mUserWantsToExit;
		}

		#region Server and client initialisation
		private bool StartClient()
		{
			mClient = new();
			if ( !mClient.Init() )
			{
				mLogger.Error( "Failed to initialise client" );
				return false;
			}

			mPresentation = new();
			if ( !mPresentation.Init() )
			{
				mLogger.Error( "Failed to initialise client (presentation layer)" );
				return false;
			}

			mSession = new();
			if ( !mSession.Init() )
			{
				mLogger.Error( "Failed to initialise client (session layer)" );
				return false;
			}

			return true;
		}

		private bool StartSingleplayerServer()
		{
			mServer = new();
			mSessionBridge = new LocalSessionBridge( mServer );

			return true;
		}

		private bool StartMultiplayerServer()
		{
			mServer = new();
			mSessionBridge = new LocalSessionBridge( mServer );
			mLogger.Warning( "Multiplayer not supported yet" );

			return false;
		}

		private bool StartDedicatedServer()
		{
			mServer = new();
			mLogger.Warning( "Multiplayer not supported yet" );
			
			return false;
		}

		private bool JoinServer()
		{
			mSessionBridge = new RemoteSessionBridge( IPAddress.IPv6Loopback );
			mLogger.Warning( "Multiplayer not supported yet" );

			return false;
		}
		#endregion

		private bool StartLevel( string path )
		{
			mLogger.Log( "StartLevel" );
			ElegyMapDocument? level = LoadLevel( $"maps/{path}" );
			if ( level is null )
			{
				mLogger.Warning( "Failed to load level" );
				return false;
			}

			if ( !mServer?.Setup( 16, level ) ?? false )
			{
				mLogger.Warning( "Failed to start server" );
				return false;
			}

			//if ( !mSession?.Setup( mSessionBridge, level ) ?? false )
			//{
			//	mLogger.Warning( "Failed to start session" );
			//	return false;
			//}

			return true;
		}

		private ElegyMapDocument? LoadLevel( string path )
		{
			string pathWithExt = Path.ChangeExtension( path, ".elf" );
			string? fullPath = Files.PathTo( pathWithExt, PathFlags.File );

			if ( fullPath is null )
			{
				mLogger.Error( $"Level '{pathWithExt}' does not exist." );

				// Check for a TrenchBroom .map file, chances are the user did not compile
				// the .elf for whatever reason.
				string originalMapPath = Path.ChangeExtension( path, ".map" );
				fullPath = Files.PathTo( originalMapPath, PathFlags.File );
				if ( fullPath is not null )
				{
					// TODO: some people probably won't be reading the compile log, so what if we
					// implemented intelligent log reading? The formatting is super consistent and
					// it should be easy to find errors
					mLogger.Log( "There's a .map file with the same name though. Did you compile it?" );
					mLogger.Log( "If you did and it's not there, please read the compile log." );
				}

				return null;
			}

			ElegyMapDocument? map = Assets.LoadLevel( fullPath );
			if ( map is null )
			{
				mLogger.Error( "Error while reading the map file" );
				return null;
			}

			return map;
		}
	}
}
