
namespace Elegy
{
	public class PluginLibrary
	{
		internal PluginLibrary( Assembly assembly, PluginLibraryMetadata metadata, string metadataPath )
		{
			Assembly = assembly;
			Metadata = metadata;
			MetadataPath = metadataPath;

			InitialisePluginLibrary();
		}

		private void InitialisePluginLibrary()
		{
			var types = Assembly.GetTypes();
			for ( int i = 0; i < types.Length; i++ )
			{
				if ( types[i].GetInterface( Metadata.ImplementedInterface ) != null )
				{
					Factory = () => Activator.CreateInstance( types[i] ) as IPlugin;
					LoadedSuccessfully = true;
					break;
				}
			}
		}

		public T? InstantiatePlugin<T>() where T : class, IPlugin
		{
			return InstantiatePlugin() as T;
		}

		public IPlugin? InstantiatePlugin()
		{
			return Factory?.Invoke() ?? null;
		}

		public delegate IPlugin? PluginFactory();

		public Assembly Assembly { get; }
		public PluginLibraryMetadata Metadata { get; }
		public string MetadataPath { get; }
		public PluginFactory? Factory { get; private set; } = null;
		public bool LoadedSuccessfully { get; private set; } = false;
	}
}
