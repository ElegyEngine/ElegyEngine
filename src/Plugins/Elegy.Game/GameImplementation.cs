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
using Game.Session.Bridges;
using Game.Shared;
using System.Diagnostics;
using System.Net;
using Elegy.RenderSystem.API;
using Elegy.RenderSystem.Objects;
using Game.Shared.Physics;

namespace Game
{
	public class GameImplementation : IApplication
	{
		private TaggedLogger mLogger = new( "Game" );
		private Stopwatch mStopwatch = new();

		private GameClient? mClient;             // User input handling, main menu etc.
		private GamePresentation? mPresentation; // Rendering, HUD etc.
		private GameSession? mSession;           // Clientside game state
		private GameServer? mServer;             // Serverside entities etc. null when connecting to a server

		private bool mUserWantsToExit = false;

		public string Name => "Elegy test game";
		public string Error { get; private set; } = string.Empty;
		public bool Initialised { get; private set; }

		public GameImplementation()
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

			string mapName = Console.Arguments.GetValueOrDefault( "+map", "test2" );
			int maxPlayers = Console.Arguments.GetInt( "+maxplayers", 16 );

			EntityWorld.Init();
			PhysicsWorld.Init();

			bool headlessMode = Console.Arguments.ContainsKey( "-headless" );
			if ( headlessMode )
			{
				return StartServer( mapName, maxPlayers );
			}

			if ( !StartClient() )
			{
				return false;
			}

			// TODO: since we don't have any UI yet, we'll force start an SP server
			return StartServerAndJoin( mapName );
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

			PhysicsWorld.Shutdown();
			EntityWorld.Shutdown();
		}

		public bool RunFrame( float delta )
		{
			// Update input, main menu etc.
			mClient?.Update( delta );

			// Send client state to server
			mSession?.SendClientState();

			// Entity simulation on the server, sending data to clients etc.
			mServer?.Update( delta );

			// Receive entity state from server, predict & simulate clientside entities
			mSession?.Update( delta );

			// Update camera, HUD etc. Rendering is handled elsewhere
			mPresentation?.Update( delta );

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

			EntityWorld.OnSpawned += static entity => entity.Dispatch( new Entity.ClientSpawnEvent( entity ) );
			EntityWorld.OnPreDestroyed += static entity => entity.Dispatch( new Entity.ClientDespawnEvent( entity ) );

			return true;
		}

		/// <summary>
		/// Starts a game instance and joins it.
		/// </summary>
		private bool StartServerAndJoin( string mapName, int maxPlayers = 1 )
		{
			if ( !StartServer( mapName, maxPlayers ) )
			{
				return false;
			}

			// In singleplayer, update every frame so
			// there's no need for clientside prediction
			if ( maxPlayers == 1 )
			{
				mServer.ServerUpdateRate = 1000;
			}

			return StartJoiningServer( IPAddress.IPv6Loopback );
		}

		/// <summary>
		/// Joins a game instance.
		/// </summary>
		private bool StartJoiningServer( IPAddress address )
		{
			if ( mClient is null )
			{
				mLogger.Error( "Tried joining server without a client" );
				return false;
			}

			mSession = new( mClient );

			// In singleplayer, don't waste time on sending & receiving your own packets.
			// But also take into account that a headless server may run on the same machine!
			var sessionBridgeFactory = IServerBridge () =>
			{
				if ( IPAddress.IsLoopback( address ) && mServer is not null )
				{
					return new LocalSessionBridge( mServer, mSession );
				}

				return new RemoteSessionBridge( mSession, address );
			};

			mSession.Bridge = sessionBridgeFactory();
			mSession.Bridge.SendJoinRequest( address );

			// The actual connection will happen over the course of a few seconds,
			// the session bridge will take care of it all.
			return true;
		}

		#endregion

		/// <summary>
		/// Starts a game instance.
		/// </summary>
		private bool StartServer( string path, int maxPlayers = 1 )
		{
			mLogger.Log( $"StartServer [map: '{path}', clients: {maxPlayers}]" );

			ElegyMapDocument? level = LoadLevel( $"maps/{path}" );
			if ( level is null )
			{
				mLogger.Warning( "Failed to load level" );
				return false;
			}

			mServer = new( maxPlayers );

			EntityWorld.OnSpawned += static entity => entity.Dispatch( new Entity.SpawnEvent( entity ) );
			EntityWorld.OnPreDestroyed += static entity => entity.Dispatch( new Entity.DespawnEvent( entity ) );

			if ( !mServer.Setup( level ) )
			{
				mLogger.Warning( "Failed to start server" );
				return false;
			}

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
