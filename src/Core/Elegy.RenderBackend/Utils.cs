// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.RenderBackend.Assets;

using System.Reflection;
using System.Runtime.InteropServices;
using Veldrid;

namespace Elegy.RenderBackend
{
	public static class Utils
	{
		#region Type conversion
		public static string ShaderTypeToGlslString( ShaderDataType type )
			=> type switch
			{
				ShaderDataType.Byte => "bool",
				ShaderDataType.Short => "short",
				ShaderDataType.Int => "int",
				ShaderDataType.Float => "float",
				ShaderDataType.Vec2 => "vec2",
				ShaderDataType.Vec3 => "vec3",
				ShaderDataType.Vec4 => "vec4",
				ShaderDataType.Vec2Byte => "vec2",
				ShaderDataType.Vec3Byte => "vec3",
				ShaderDataType.Vec4Byte => "vec4",
				ShaderDataType.Mat22 => "mat2",
				ShaderDataType.Mat33 => "mat3",
				ShaderDataType.Mat44 => "mat4",
				ShaderDataType.Texture1D => "texture1D",
				ShaderDataType.Texture2D => "texture2D",
				ShaderDataType.Texture3D => "texture3D",
				ShaderDataType.Sampler => "sampler",
				_ => string.Empty
			};

		public static ResourceKind TypeToResourceKind( ShaderDataType type )
			=> type switch
			{
				ShaderDataType.Sampler => ResourceKind.Sampler,
				ShaderDataType.BufferRW => ResourceKind.StructuredBufferReadWrite,
				ShaderDataType.Texture1D => ResourceKind.TextureReadOnly,
				ShaderDataType.Texture2D => ResourceKind.TextureReadOnly,
				ShaderDataType.Texture3D => ResourceKind.TextureReadOnly,
				_ => ResourceKind.UniformBuffer,
			};

		public static VertexElementFormat ShaderTypeToVertexElementFormat( string name, ShaderDataType type )
		{
			string nameLower = name.ToLower();
			if ( nameLower.Contains( "normal" ) || nameLower.Contains( "tangent" ) )
			{
				return type switch
				{
					ShaderDataType.Vec2 => VertexElementFormat.SByte2_Norm,
					ShaderDataType.Vec3 => VertexElementFormat.SByte4_Norm,
					ShaderDataType.Vec4 => VertexElementFormat.SByte4_Norm,
					_ => throw new NotSupportedException( "Normals and tangents can only do vec2/3/4" )
				};
			}

			return type switch
			{
				ShaderDataType.Short => VertexElementFormat.Byte2,
				ShaderDataType.Int => VertexElementFormat.Int1,
				ShaderDataType.Vec2 => VertexElementFormat.Float2,
				ShaderDataType.Vec2Byte => VertexElementFormat.Byte2,
				ShaderDataType.Vec3 => VertexElementFormat.Float3,
				ShaderDataType.Vec4 => VertexElementFormat.Float4,
				ShaderDataType.Vec4Byte => VertexElementFormat.Byte4,
				_ => VertexElementFormat.Int4
			};
		}

		public static BlendAttachmentDescription ExtractBlendDescription( PipelineInfo info )
			=> (info.AlphaTest || info.BlendMode == Blending.Opaque) switch
			{
				// src*1 + dst*0 = src
				true => BlendAttachmentDescription.Disabled,

				false => info.BlendMode switch
				{
					// src*alpha + dst*(1-alpha) = lerp(src, dst, alpha)
					Blending.Transparent => BlendAttachmentDescription.AlphaBlend,

					// src*1 + dst*1 = src + dst
					Blending.Additive => BlendAttachmentDescription.AdditiveBlend,

					// src*0 + dst*src = src*dst
					Blending.Multiply => new()
					{
						BlendEnabled = true,
						SourceColorFactor = BlendFactor.Zero,
						DestinationColorFactor = BlendFactor.SourceColor,
						ColorFunction = BlendFunction.Add,

						SourceAlphaFactor = BlendFactor.SourceAlpha,
						DestinationAlphaFactor = BlendFactor.One,
						AlphaFunction = BlendFunction.Add
					},

					// src*dst + dst*src = 2(src*dst)
					Blending.MiddleGray => new()
					{
						BlendEnabled = true,
						SourceColorFactor = BlendFactor.DestinationColor,
						DestinationColorFactor = BlendFactor.SourceColor,
						ColorFunction = BlendFunction.Add,

						SourceAlphaFactor = BlendFactor.SourceAlpha,
						DestinationAlphaFactor = BlendFactor.One,
						AlphaFunction = BlendFunction.Add
					},

					_ => BlendAttachmentDescription.Disabled
				}
			};

		public static BlendStateDescription ExtractBlendState( MaterialTemplate materialTemplate )
			=> new()
			{
				// TODO: might wanna expose this to material params,
				// would be super useful for veggies!!!
				AlphaToCoverageEnabled = false,
				AttachmentStates = [ ExtractBlendDescription( materialTemplate.PipelineInfo ) ]
			};

		public static VertexLayoutDescription[] ExtractVertexLayouts( ShaderVariantEntry variant )
		{
			VertexLayoutDescription[] vertexLayouts = new VertexLayoutDescription[variant.VertexLayouts.Count];
			for ( int i = 0; i < variant.VertexLayouts.Count; i++ )
			{
				VertexLayoutEntry entry = variant.VertexLayouts[i];

				vertexLayouts[i] = new()
				{
					Elements =
					[new VertexElementDescription()
					{
						Name = entry.Name,
						Semantic = GetVertexElementSemantic( entry.Name ),
						Format = entry.Type
					}],
					Stride = GetStrideForVertexFormat( entry.Type )
				};
			}

			return vertexLayouts;
		}

		public static RasterizerStateDescription ExtractRasterizerState( MaterialTemplate materialTemplate )
			=> new()
			{
				FrontFace = FrontFace.CounterClockwise,
				ScissorTestEnabled = false,
				CullMode = materialTemplate.PipelineInfo.FaceCulling,
				FillMode = PolygonFillMode.Solid,
				// TODO: depth testing in the material template errr maybe?
				DepthClipEnabled = materialTemplate.PipelineInfo.BlendMode == Blending.Opaque
			};

		public static DepthStencilStateDescription ExtractDepthStencilState( MaterialTemplate materialTemplate )
			=> materialTemplate.PipelineInfo.BlendMode switch
			{
				Blending.Opaque => DepthStencilStateDescription.DepthOnlyLessEqual,
				_ => DepthStencilStateDescription.DepthOnlyLessEqualRead
			};
		#endregion

		#region Size stuff
		public static uint GetStrideForVertexFormat( VertexElementFormat format )
			=> format switch
			{
				VertexElementFormat.Float1		=> sizeof( float ),
				VertexElementFormat.Float2		=> sizeof( float ) * 2U,
				VertexElementFormat.Float3		=> sizeof( float ) * 3U,
				VertexElementFormat.Float4		=> sizeof( float ) * 4U,
				VertexElementFormat.Byte2		=> sizeof( byte ) * 2U,
				VertexElementFormat.Byte2_Norm	=> sizeof( byte ) * 2U,
				VertexElementFormat.Byte4		=> sizeof( byte ) * 4U,
				VertexElementFormat.Byte4_Norm	=> sizeof( byte ) * 4U,
				VertexElementFormat.SByte2		=> sizeof( sbyte ) * 2U,
				VertexElementFormat.SByte2_Norm => sizeof( sbyte ) * 2U,
				VertexElementFormat.SByte4		=> sizeof( sbyte ) * 4U,
				VertexElementFormat.SByte4_Norm => sizeof( sbyte ) * 4U,
				VertexElementFormat.UShort2		=> sizeof( ushort ) * 2U,
				VertexElementFormat.UShort2_Norm=> sizeof( ushort ) * 2U,
				VertexElementFormat.UShort4		=> sizeof( ushort ) * 4U,
				VertexElementFormat.UShort4_Norm=> sizeof( ushort ) * 4U,
				VertexElementFormat.Short2		=> sizeof( short ) * 2U,
				VertexElementFormat.Short2_Norm => sizeof( short ) * 2U,
				VertexElementFormat.Short4		=> sizeof( short ) * 4U,
				VertexElementFormat.Short4_Norm => sizeof( short ) * 4U,
				VertexElementFormat.Int1		=> sizeof( int ),
				VertexElementFormat.Int2		=> sizeof( int ) * 2U,
				VertexElementFormat.Int3		=> sizeof( int ) * 3U,
				VertexElementFormat.Int4		=> sizeof( int ) * 4U,
				VertexElementFormat.UInt1		=> sizeof( uint ),
				VertexElementFormat.UInt2		=> sizeof( uint ) * 2U,
				VertexElementFormat.UInt3		=> sizeof( uint ) * 3U,
				VertexElementFormat.UInt4		=> sizeof( uint ) * 4U,
				VertexElementFormat.Half1		=> 2U,
				VertexElementFormat.Half2		=> 2U * 2U,
				VertexElementFormat.Half4		=> 2U * 4U,
				_ => 0U
			};

		public static uint StrideOf<T>( BufferUsage usage )
			where T : unmanaged
		{
			if ( usage.HasFlag( BufferUsage.StructuredBufferReadOnly | BufferUsage.StructuredBufferReadWrite ) )
			{
				return (uint)Marshal.SizeOf<T>();
			}

			return 0U;
		}

		public static uint NearestSize16<T>( int numElements )
			where T : unmanaged
		{
			uint bytes = (uint)(numElements * Marshal.SizeOf<T>());

			return bytes + (bytes % 16);
		}
		#endregion

		public static byte[] LoadShaderBytes( string path )
		{
			return File.ReadAllBytes( path );
		}

		public static string PathToShaderVariant( ShaderTemplate template, ShaderVariantEntry entry )
		{
			return $"shaders/bin/{template.ShaderBinaryBasePath}.{entry.ShaderDefine}";
		}

		#region Extraction
		public static VertexElementSemantic GetVertexElementSemantic( string name )
		{
			string lowerName = name.ToLower();

			if ( lowerName.Contains( "normal" ) || lowerName.Contains( "tangent" ) )
			{
				return VertexElementSemantic.Normal;
			}
			else if ( lowerName.Contains( "uv" ) || lowerName.Contains( "tex" ) )
			{
				return VertexElementSemantic.TextureCoordinate;
			}
			else if ( lowerName.Contains( "col" ) )
			{
				return VertexElementSemantic.Color;
			}

			return VertexElementSemantic.Position;
		}

		public static VertexSemantic GetVertexSemantic( string name )
		{
			string lowerName = name.ToLower();

			if ( lowerName.Contains( "normal" ) )
			{
				return VertexSemantic.Normal;
			}
			else if ( lowerName.Contains( "tangent" ) )
			{
				return VertexSemantic.Tangent;
			}
			else if ( lowerName.Contains( "uv" ) || lowerName.Contains( "tex" ) )
			{
				return VertexSemantic.Uv;
			}
			else if ( lowerName.Contains( "col" ) )
			{
				return VertexSemantic.Colour;
			}
			else if ( lowerName.Contains( "weight" ) )
			{
				return VertexSemantic.BoneWeight;
			}
			else if ( lowerName.Contains( "index" ) || lowerName.Contains( "indices" ) )
			{
				return VertexSemantic.BoneIndex;
			}

			return VertexSemantic.Position;
		}

		public static VertexElementSemantic GetVertexElementSemantic( PropertyInfo propertyInfo )
		{
			return GetVertexElementSemantic( propertyInfo.Name );
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

		public static VertexLayoutDescription GenerateStandardVertexLayoutElement( VertexSemantic semantic )
			=> new()
			{
				Stride = semantic switch
				{
					VertexSemantic.Position => 3 * sizeof( float ),
					VertexSemantic.Normal => 4 * sizeof( byte ),
					VertexSemantic.Tangent => 4 * sizeof( byte ),
					VertexSemantic.Uv => 2 * sizeof( float ),
					VertexSemantic.Colour => 4 * sizeof( byte ),
					VertexSemantic.BoneWeight => 4 * sizeof( byte ),
					VertexSemantic.BoneIndex => 4 * sizeof( byte ),
					_ => throw new ArgumentException()
				},

				Elements =
				[
					new VertexElementDescription()
					{
						Name = semantic.ToString(),
						Offset = 0,
						Format = semantic switch
						{
							VertexSemantic.Position => VertexElementFormat.Float3,
							VertexSemantic.Normal => VertexElementFormat.Byte4,
							VertexSemantic.Tangent => VertexElementFormat.Byte4,
							VertexSemantic.Uv => VertexElementFormat.Float2,
							VertexSemantic.Colour => VertexElementFormat.Byte4,
							VertexSemantic.BoneWeight => VertexElementFormat.Byte4,
							VertexSemantic.BoneIndex => VertexElementFormat.Byte4,
							_ => throw new ArgumentException()
						}
					}
				]
			};

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
		#endregion
	}
}
