
using System.Reflection;
using System.Runtime.InteropServices;
using Veldrid;

namespace Elegy.RenderBackend
{
	public static class Utils
	{
		public static byte[] LoadShaderBytes( string path )
		{
			return File.ReadAllBytes( path );
		}

		public static VertexElementSemantic GetVertexElementSemantic( PropertyInfo propertyInfo )
		{
			string name = propertyInfo.Name.ToLower();

			if ( name.Contains( "normal" ) || name.Contains( "tangent" ) )
			{
				return VertexElementSemantic.Normal;
			}
			else if ( name.Contains( "uv" ) || name.Contains( "tex" ) )
			{
				return VertexElementSemantic.TextureCoordinate;
			}
			else if ( name.Contains( "col" ) )
			{
				return VertexElementSemantic.Color;
			}

			return VertexElementSemantic.Position;
		}

		public static VertexElementFormat GetVertexElementFormat( PropertyInfo propertyInfo )
		{
			return propertyInfo.PropertyType.Name switch
			{
				"Vector4I" => VertexElementFormat.Int4,
				"Vector3I" => VertexElementFormat.Int3,
				"Vector2I" => VertexElementFormat.Int2,
				"int" => VertexElementFormat.Int1,

				"Vector4" => VertexElementFormat.Float4,
				"Vector3" => VertexElementFormat.Float3,
				"Vector2" => VertexElementFormat.Float2,
				"float" => VertexElementFormat.Float1,

				_ => throw new NotSupportedException(
					$"Unsupported format of vertex element '{propertyInfo.DeclaringType?.Name ?? "unknown"}.{propertyInfo.Name}'" )
			};
		}

		public static VertexLayoutDescription[] GenerateVertexLayoutFor<TVertex>() where TVertex : struct
		{
			Type vertexType = typeof( TVertex );
			var properties = vertexType.GetProperties();
			List<VertexElementDescription> elements = new( properties.Length );

			foreach ( var property in properties )
			{
				elements.Add( new()
				{
					Name = $"v{property.Name}",
					Format = GetVertexElementFormat( property ),
					Semantic = GetVertexElementSemantic( property )
				} );
			}

			return
			[
				new()
				{
					Elements = elements.ToArray(),
					Stride = (uint)Marshal.SizeOf<TVertex>()
				}
			];
		}
	}
}
