
// TEMPORARY until we get a render backend!

namespace Elegy
{
	public class Material
	{
		public Material()
		{

		}

		public string ResourceName { get; set; } = string.Empty;

		// public RenderMaterial => ...
	}

	public class Texture2D
	{
		public static Texture2D? CreateFromImageFile( string path )
		{
			return null;
		}
	}
}
