// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Common.Interfaces;
using Elegy.ConsoleSystem;
using Elegy.ConsoleSystem.Frontends;
using Elegy.PluginSystem.API;

namespace Elegy.Framework
{
	/// <summary>
	/// The engine framework. Launches and shuts down subsystems.
	/// </summary>
	public static partial class EngineSystem
	{
		/// <summary>
		/// Elegy Engine major version, used for version checking against plugins.
		/// </summary>
		public const int MajorVersion = 0;
		/// <summary>
		/// Elegy Engine minor version, used for version checking against plugins.
		/// </summary>
		public const int MinorVersion = 1;
		/// <summary>
		/// Plugins built before this minor version will not work.
		/// </summary>
		public const int OldestSupportedMinor = 0;

		private static TaggedLogger mLogger = new( "Engine" );

		private static Action? mSystemShutdownFunc = null;
		private static bool mHasShutdown = false;
		private static LaunchConfig mLaunchConfig;

		private static EngineConfig EngineConfig => mLaunchConfig.Engine;

		/// <summary>
		/// Version string to be displayed when the engine starts up.
		/// </summary>
		public static readonly string VersionString = $"{MajorVersion}.{MinorVersion}";

		/// <summary>
		/// The reason of shutdown. Might be an error.
		/// </summary>
		public static string? ShutdownReason { get; private set; } = null;

		/// <summary>
		/// Is the engine running?
		/// </summary>
		public static bool IsRunning => !mHasShutdown;

		/// <summary>
		/// Time the engine started.
		/// </summary>
		public static DateTime StartupTime { get; private set; }

		/// <summary>
		/// Initialises the engine's systems.
		/// </summary>
		public static bool Init( LaunchConfig config,
			Func<LaunchConfig, bool> systemInitFunc,
			Func<bool> systemPostInitFunc,
			Action systemShutdownFunc,
			Func<string?> systemErrorFunc )
		{
			StartupTime = DateTime.Now;
			mHasShutdown = false;

			mSystemShutdownFunc = systemShutdownFunc;

			if ( !LoadOrCreateEngineConfig( config ) )
			{
				return Shutdown( "Configuration failure" );
			}

			// Some apps will use all available engine subsystems,
			// others might only need a couple, so prepare them right here.
			if ( !systemInitFunc( config ) )
			{
				return Shutdown( systemErrorFunc() );
			}

			mLogger.Verbose( $"Working directory: '{Directory.GetCurrentDirectory()}'" );

			if ( EngineConfig.ConfigName != null )
			{
				mLogger.Developer( $"Engine configuration: '{EngineConfig.ConfigName}'" );
			}

			foreach ( IPlugin plugin in Plugins.GenericPlugins )
			{
				if ( plugin is IConsoleFrontend )
				{
					Console.AddFrontend( plugin as IConsoleFrontend );
				}
			}

			// Now that the primary things are (hopefully) initialised, we can initialise
			// secondary subsystems that depend on these ones.
			if ( !systemPostInitFunc() )
			{
				return Shutdown( systemErrorFunc() );
			}

			mLogger.Success( $"Initialised Elegy Engine ({VersionString})" );
			mLogger.WarningIf( MajorVersion < 1, "This is an early in-development build of the engine. DO NOT use in production!" );

			return true;
		}

		private static bool LoadOrCreateEngineConfig( in LaunchConfig config )
		{
			mLaunchConfig = config;
			if ( config.EngineConfigName is null )
			{
				return true;
			}

			if ( !File.Exists( config.EngineConfigName ) )
			{
				mLogger.Log( $"'{config.EngineConfigName}' does not exist, creating a default one..." );

				mLaunchConfig.Engine = new();
				Common.Text.JsonHelpers.Write( EngineConfig, config.EngineConfigName );
				return true;
			}

			EngineConfig engineConfig = new();
			if ( !Common.Text.JsonHelpers.LoadFrom( ref engineConfig, config.EngineConfigName ) )
			{
				mLogger.Error( $"'{config.EngineConfigName}' somehow failed to load" );
				return false;
			}

			mLaunchConfig.Engine = engineConfig;

			return true;
		}

		/// <summary>
		/// Shuts down the engine.
		/// </summary>
		/// <param name="why">The reason of shutdown. If left empty, it is a normal shutdown, else an error.</param>
		public static bool Shutdown( string why = "" )
		{
			if ( mHasShutdown )
			{
				return true;
			}

			if ( why == "" )
			{
				mLogger.Log( "Shutting down normally..." );
			}
			else
			{
				mLogger.Error( $"Shutting down, reason: {why}" );
			}

			mSystemShutdownFunc?.Invoke();
			ShutdownReason = why;
			mHasShutdown = true;
			return false;
		}
	}
}
