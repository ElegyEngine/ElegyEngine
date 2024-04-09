﻿// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Common.Interfaces;
using Elegy.ConsoleSystem;
using Elegy.ConsoleSystem.Frontends;
using Elegy.PluginSystem.API;

namespace Elegy.Engine
{
	/// <summary>
	/// The engine. Launches and updates all subsystems.
	/// </summary>
	public static partial class Engine
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

		private static bool mHasShutdown = false;
		private static EngineConfig mEngineConfig;

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
		/// Is the engine running in headless/dedicated server mode?
		/// </summary>
		public static bool IsHeadless { get; private set; } = false;

		/// <summary>
		/// Time the engine started.
		/// </summary>
		public static DateTime StartupTime { get; private set; }

		/// <summary>
		/// Autogenerated startup of subsystems. Different apps/tools/games will use different subsystems.
		/// </summary>
		static partial void Init_Generated( in LaunchConfig config );

		/// <summary>
		/// Same as <see cref="Init_Generated"/> but after the primary subsystems were initialised.
		/// </summary>
		static partial void PostInit_Generated( in LaunchConfig config );

		/// <summary>
		/// Refer to <see cref="PostInit_Generated"/>.
		/// </summary>
		static partial void PreShutdown_Generated();

		/// <summary>
		/// Refer to <see cref="Init_Generated"/>.
		/// </summary>
		static partial void Shutdown_Generated();

		/// <summary>
		/// Initialises the engine's systems.
		/// </summary>
		public static bool Init( in LaunchConfig config )
		{
			StartupTime = DateTime.Now;
			mHasShutdown = false;

			if ( !LoadOrCreateEngineConfig( config ) )
			{
				return Shutdown( "Configuration failure" );
			}

			// Initialise all other subsystems
			Init_Generated( config );

			Console.Log( $"Initialising Elegy Engine ({VersionString})" );
			if ( MajorVersion < 1 )
			{
				mLogger.Warning( "This is an early in-development build of the engine. DO NOT use in production!" );
			}
			mLogger.Verbose( $"Working directory: '{Directory.GetCurrentDirectory()}'" );

			IsHeadless = Console.Arguments.GetBool( "headless" );

			if ( mEngineConfig.ConfigName != null )
			{
				mLogger.Developer( $"Engine config: '{mEngineConfig.ConfigName}'" );
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
			PostInit_Generated( config );

			mLogger.Log( "Successfully initialised all systems" );
			return true;
		}

		private static bool LoadOrCreateEngineConfig( in LaunchConfig config )
		{
			string path = config.EngineConfigName ?? "engineConfig.json";

			if ( config.EngineConfigName is null && config.Engine is not null )
			{
				mEngineConfig = config.Engine.Value;
				return true;
			}

			if ( !File.Exists( path ) )
			{
				mLogger.Log( $"'{path}' does not exist, creating a default one..." );

				mEngineConfig = new();
				Common.Text.JsonHelpers.Write( mEngineConfig, path );
				return true;
			}

			if ( !Common.Text.JsonHelpers.LoadFrom( ref mEngineConfig, path ) )
			{
				mLogger.Error( $"'{path}' somehow failed to load" );
				return false;
			}

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

			PreShutdown_Generated();
			Shutdown_Generated();

			ShutdownReason = why;

			mHasShutdown = true;
			return false;
		}
	}
}
