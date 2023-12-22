// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

using Elegy.Assets;

namespace TestGame
{
	public class Game : IApplication
	{
		public string Name => "Elegy test game";
		public string Error { get; private set; } = string.Empty;
		public bool Initialised { get; private set; } = false;

		public const string Tag = "Game";

		public Game()
		{

		}

		public bool Init()
		{
			Console.Log( Tag, "Init" );
			Initialised = true;

			Elegy.Assets.ApplicationConfig gameConfig = FileSystem.CurrentConfig;

			Console.Log( Tag,   $"Name:      {gameConfig.Title}" );
			Console.Log( $"       Developer: {gameConfig.Developer}" );
			Console.Log( $"       Publisher: {gameConfig.Publisher}" );
			Console.Log( $"       Version:   {gameConfig.Version}" );

			return true;
		}

		public bool Start()
		{
			Console.Log( Tag, "Start" );

			mMenu.OnNewGame = ( string mapName ) =>
			{
				StartGame( mapName );
				mMenu.Visible = false;
			};

			mMenu.OnLeaveGame = () =>
			{
				LeaveGame();
			};

			mMenu.OnExit = () =>
			{
				LeaveGame();
				mUserWantsToExit = true;
			};

			mMenu.Init();

			return true;
		}

		public void Shutdown()
		{
			Console.Log( Tag, "Shutdown" );
			
			mEntities.Clear();
			mClient = null;
		}

		private void ToggleMenu()
		{
			if ( !mGameIsLoaded )
			{
				mMenu.Visible = true;
				Input.MouseMode = Input.MouseModeEnum.Visible;
				return;
			}

			mMenu.Visible = !mMenu.Visible;
			Input.MouseMode = mMenu.Visible ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
		}

		public bool RunFrame( float delta )
		{
			// Quick little toggle quickly cobbled together,
			// until we have an extension to the input system
			if ( Input.IsKeyPressed( Key.Escape ) )
			{
				if ( !mEscapeWasHeld )
				{
					ToggleMenu();
				}
				mEscapeWasHeld = true;
			}
			else
			{
				mEscapeWasHeld = false;
			}

			if ( mGameIsLoaded )
			{
				mEntities.ForEach( entity => entity.Think() );
				mClient.Update();
				mClient.UpdateController();
			}

			return !mUserWantsToExit;
		}

		public void RunPhysicsFrame( float delta )
		{
			if ( mGameIsLoaded )
			{
				mEntities.ForEach( entity => entity.PhysicsUpdate( delta ) );
			}
		}

		public void HandleInput( InputEvent @event )
		{
			if ( mGameIsLoaded )
			{
				mClient.UserInput( @event );
			}
		}

		private void StartGame( string mapFile )
		{
			if ( mGameIsLoaded )
			{
				LeaveGame();
			}

			Console.Log( Tag, $"Starting 'maps/{mapFile}'..." );
			string? mapPath = FileSystem.PathTo( $"maps/{mapFile}", PathFlags.File );
			if ( mapPath is null )
			{
				Console.Error( "Game.StartGame", $"Cannot load 'maps/{mapFile}', it doesn't exist" );
				return;
			}

			mMap = ElegyMapDocument.LoadFromFile( mapPath );
			if ( mMap is null )
			{
				Console.Error( "Game.StartGame", $"Failed to load 'maps/{mapFile}'" );
				return;
			}

			mEntities = new();
			
			mWorldspawnNode = Assets.MapGeometry.CreateBrushModelNode( mMap, 0 );
			for ( int entityId = 0; entityId < mMap.Entities.Count; entityId++ )
			{
				var mapEntity = mMap.Entities[entityId];
				Entities.Entity? entity = null;

				// TODO: MapEntity attribute that glues the classname to the class
				string className = mapEntity.Attributes["classname"];
				switch ( className )
				{
				case "info_player_start": entity = CreateEntity<Entities.InfoPlayerStart>(); break;
				case "light": entity = CreateEntity<Entities.Light>(); break;
				case "light_environment": entity = CreateEntity<Entities.LightEnvironment>(); break;
				case "func_detail": entity = CreateEntity<Entities.FuncDetail>(); break;
				case "func_breakable": entity = CreateEntity<Entities.FuncBreakable>(); break;
				case "func_rotating": entity = CreateEntity<Entities.FuncRotating>(); break;
				case "func_water": entity = CreateEntity<Entities.FuncWater>(); break;
				case "prop_test": entity = CreateEntity<Entities.PropTest>(); break;
				default: Console.Log( "Game.StartGame", $"{Console.Yellow}Unknown map entity class {Console.White}'{className}'", ConsoleMessageType.Developer ); continue;
				}

				// This is a brush entity
				if ( mapEntity.RenderMeshId != -1 )
				{
					entity.AddBrushModel( mMap, entityId );
				}

				// Actually KeyValue should be called BEFORE Spawn, but oh well
				entity.KeyValue( mapEntity.Attributes );
			}

			mEntities.ForEach( entity => entity.PostSpawn() );

			mClient = new()
			{
				Controller = FindEntity<Entities.InfoPlayerStart>()?.SpawnPlayer( this ) ?? CreateEntity<Entities.Player>()
			};

			Console.Success( Tag, "Map successfully loaded, enjoy" );
			mGameIsLoaded = true;
		}

		void LeaveGame()
		{
			if ( !mGameIsLoaded )
			{
				return;
			}

			Console.Log( Tag, "Leaving the game..." );

			mMap = null;
			mClient = null;
			mEntities.ForEach( entity => entity.Destroy() );
			mEntities.Clear();

			mWorldspawnNode.QueueFree();

			mGameIsLoaded = false;
		}

		#region Move elsewhere

		// TODO: manage entities elsewhere
		// PROPER ENTITY SYSTEM REQUIRED
		public T CreateEntity<T>() where T : Entities.Entity, new()
		{
			T entity = new();
			entity.Spawn();
			mEntities.Add( entity );

			return entity;
		}

		public T? FindEntity<T>() where T : Entities.Entity
		{
			foreach ( var entity in mEntities )
			{
				if ( entity is T )
				{
					return entity as T;
				}
			}

			return null;
		}

		#endregion

		private Client.Client? mClient;
		private Client.MainMenu mMenu = new();
		private List<Entities.Entity> mEntities = new();
		private ElegyMapDocument? mMap;
		private Node3D mWorldspawnNode;

		private bool mGameIsLoaded = false;
		private bool mEscapeWasHeld = false;
		private bool mUserWantsToExit = false;
	}
}
