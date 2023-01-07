
namespace Elegy.Internal
{
	internal sealed class PluginSystem
	{
		public PluginSystem( string configPath )
		{

		}

		public bool Init()
		{
			Elegy.Console.Log( "[PluginSystem] Init" );

			if ( LoadLibrary( "game" ) == null )
			{
				return false;
			}

			List<string> failedPlugins = new();
			mPluginLibraries.ForEach( library =>
			{
				IPlugin? plugin = library.InstantiatePlugin();
				if ( plugin == null )
				{
					failedPlugins.Add( $"'{library.Metadata.Name}' at '{library.MetadataPath}' - couldn't instantiate" );
					return;
				}

				if ( !plugin.Init() )
				{
					failedPlugins.Add( $"'{library.Metadata.Name}' at '{library.MetadataPath}' - failed to initialise ({plugin.Error})" );
					return;
				}

				if ( plugin is IApplication )
				{
					mApplicationPlugins.Add( library.MetadataPath, plugin as IApplication );
				}
				else
				{
					mGenericPlugins.Add( library.MetadataPath, plugin );
				}
			} );

			if ( failedPlugins.Count > 0 )
			{
				Elegy.Console.Error( "Plugins failed to load:" );
				for ( int i = 0; i < failedPlugins.Count; i++ )
				{
					Elegy.Console.Log( $" * {failedPlugins[i]}" );
				}
				Elegy.Console.Log( "Resolve these plugins' errors and try again." );
				return false;
			}

			failedPlugins.Clear();
			foreach ( var app in mApplicationPlugins )
			{
				if ( !app.Value.Start() )
				{
					failedPlugins.Add( $"'{app.Value.Name}' - failed to start ({app.Value.Error})" );
				}
			}

			if ( failedPlugins.Count > 0 )
			{
				Elegy.Console.Error( "Applications failed to load:" );
				for ( int i = 0; i < failedPlugins.Count; i++ )
				{
					Elegy.Console.Log( $" * {failedPlugins[i]}" );
				}
				Elegy.Console.Log( "Resolve these application errors and try again." );
				return false;
			}

			return true;
		}

		public void Shutdown()
		{
			// First shut down any app/game app
			foreach ( var plugin in mGenericPlugins )
			{
				plugin.Value.Shutdown();
			}
			foreach ( var app in mApplicationPlugins )
			{
				app.Value.Shutdown();
			}
		}

		public IPlugin? GetPlugin( string path )
		{
			return mGenericPlugins.GetValueOrDefault( path );
		}

		public IApplication? GetApplication( string path )
		{
			return mApplicationPlugins.GetValueOrDefault( path );
		}

		public bool UnloadGenericPlugin( IPlugin plugin )
		{
			int i = 0;
			foreach ( var pluginPair in mGenericPlugins )
			{
				if ( pluginPair.Value == plugin )
				{
					mGenericPlugins.Remove( mGenericPlugins.Keys.ElementAt( i ) );
					return true;
				}

				i++;
			}

			return false;
		}

		public bool UnloadApplication( IApplication app )
		{
			int i = 0;
			foreach ( var plugin in mApplicationPlugins )
			{
				if ( plugin.Value == app )
				{
					mApplicationPlugins.Remove( mApplicationPlugins.Keys.ElementAt( i ) );
					return true;
				}

				i++;
			}

			return false;
		}

		private PluginLibrary? LoadLibrary( string path )
		{
			for ( int i = 0; i < mPluginLibraries.Count; i++ )
			{
				if ( mPluginLibraries[i].MetadataPath == path )
				{
					return mPluginLibraries[i];
				}	
			}

			Assembly assembly;
			try
			{
				assembly = mLoadContext.LoadFromAssemblyPath( $"{Directory.GetCurrentDirectory()}/{path}/Game.dll" );
			}
			catch ( Exception ex )
			{
				Elegy.Console.Error( $"[PluginSystem] Failed to load '{path}/Game.dll'" );
				Elegy.Console.Error( $"[PluginSystem] Exception: {ex.Message.TrimEnd( '\r' )}" );

				return null;
			}

			Elegy.Console.Log( $"[PluginSystem] Found '{path}/Game.dll'" );

			PluginLibraryMetadata metadata = new( "temp", "temp", "temp", DateTime.Now, "IGame" );
			PluginLibrary library = new( assembly, metadata, path );

			if ( !library.LoadedSuccessfully )
			{
				Elegy.Console.Error( $"[PluginSystem] Didn't load '{path}/Game.dll' successfully" );
				return null;
			}

			mPluginLibraries.Add( library );
			return library;
		}

		public IReadOnlyCollection<IApplication> ApplicationPlugins => mApplicationPlugins.Values;
		public IReadOnlyCollection<IPlugin> GenericPlugins => mGenericPlugins.Values;

		private Dictionary<string, IApplication> mApplicationPlugins = new();
		private Dictionary<string, IPlugin> mGenericPlugins = new();
		private List<PluginLibrary> mPluginLibraries = new();

		private PluginLoadContext mLoadContext = new();
	}
}
