// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Text;
using Elegy.Common.Utilities;
using Elegy.RenderBackend.Assets;

using System.Diagnostics;
using System.Text;

namespace Elegy.ShaderTool
{
	public class GlslMaterialParameter
	{
		public int SetId { get; set; } = 0;
		public int BindingId { get; set; } = 0;
		public ShaderDataType Type { get; set; } = ShaderDataType.Texture2D;
		public string TypeGlslName => RenderBackend.Utils.ShaderTypeToGlslString( Type );
		public string ShaderName { get; set; } = string.Empty;
		public string MaterialName { get; set; } = string.Empty;
		public string[] VariantMask { get; set; } = Array.Empty<string>();
		public MaterialParameterLevel Level { get; set; } = MaterialParameterLevel.Builtin;
	}

	public class GlslMaterialParameterBuffer
	{
		public int SetId { get; set; } = 0;
		public int BindingId { get; set; } = 0;
		public string ShaderName { get; set; } = string.Empty;
		public string MaterialName { get; set; } = string.Empty;
		public string[] VariantMask { get; set; } = Array.Empty<string>();
		public string StructContents { get; set; } = string.Empty;
		public MaterialParameterLevel Level { get; set; } = MaterialParameterLevel.Builtin;
	}

	public class GlslInput
	{
		public int LocationId { get; set; } = 0;
		public ShaderDataType Type { get; set; } = ShaderDataType.Vec3;
		public string TypeGlslName => RenderBackend.Utils.ShaderTypeToGlslString( Type );
		public string Name { get; set; } = string.Empty;
		public string[] VariantMask { get; set; } = Array.Empty<string>();
	}

	public class ShaderProcessor
	{
		public ShaderProcessor( string filePath, string contents )
		{
			FilePath = filePath;
			Contents = contents;
		}

		public string FilePath { get; }
		public string Contents { get; }

		public List<ShaderPermutation> Permutations { get; } = new();
		public List<GlslMaterialParameter> MaterialParameters { get; } = new();
		public List<GlslMaterialParameterBuffer> MaterialParameterBuffers { get; } = new();
		public List<GlslInput> VertexInputs { get; } = new();
		public List<GlslInput> PixelInputs { get; } = new();
		public string PixelShaderOutput { get; private set; } = string.Empty;
		public string TemplateName { get; private set; } = string.Empty;

		public List<string> ShaderVariants { get; private set; } = new();
		public string CommonShaderContents { get; private set; } = string.Empty;
		public Dictionary<string, string> VertexShaderRegions { get; private set; } = new();
		public Dictionary<string, string> PixelShaderRegions { get; private set; } = new();
		
		public bool CreatePermutations()
		{
			StringBuilder commonShaderSb = new( 64 );

			// Pass 1: Find out what shader variants there are
			Lexer lexer = new( Contents );
			while ( !lexer.IsEnd() )
			{
				string token = lexer.Next();

				if ( token == "ShaderTemplate" )
				{
					TemplateName = ParseSimpleNameDeclaration( lexer );
				}
				else if ( token == "ShaderVariants" )
				{
					ShaderVariants = ParseShaderVariants( lexer ).ToList();
				}
				else if ( token == "MaterialParameter" )
				{
					MaterialParameters.Add( ParseMaterialParameter( lexer ) );
				}
				else if ( token == "MaterialParameterBuffer" )
				{
					MaterialParameterBuffers.Add( ParseMaterialParameterBuffer( lexer ) );
				}
				else if ( token == "VertexInput" )
				{
					VertexInputs.Add( ParseShaderInput( lexer ) );
				}
				else if ( token == "PixelInput" )
				{
					PixelInputs.Add( ParseShaderInput( lexer ) );
				}
				else if ( token == "PixelOutput" )
				{
					PixelShaderOutput = ParseSimpleNameDeclaration( lexer );
				}
				else if ( token == "VertexShader" )
				{
					ParseShaderRegion( lexer, true );
				}
				else if ( token == "PixelShader" )
				{
					ParseShaderRegion( lexer, false );
				}
				else if ( token.StartsWith( '#' ) )
				{
					commonShaderSb.AppendLine( $"{token} {lexer.PeekUntil( "\n", true )}" );
				}
				else
				{
					commonShaderSb.Append( token );
					if ( "{};".Contains( token ) )
					{
						commonShaderSb.AppendLine();
					}
					else
					{
						commonShaderSb.Append( ' ' );
					}
				}
			}

			CommonShaderContents = commonShaderSb.ToString();

			// Pass 2: For each variant, prepare two shaders: a vertex and pixel shader, with the
			// appropriate defines and material parameter inclusions/exclusions
			Permutations.EnsureCapacity( ShaderVariants.Count * 2 );
			foreach ( var variant in ShaderVariants )
			{
				Permutations.Add( new(
					EmitGlslForPermutation( variant, ShaderKind.Vertex ),
					EmitGlslForPermutation( variant, ShaderKind.Pixel ),
					variant ) );
			}

			return true;
		}

		public ShaderTemplate CreateShaderTemplate()
		{
			ShaderTemplate shaderTemplate = new()
			{
				Name = TemplateName,
				ShaderBinaryBasePath = Path.ChangeExtension( Path.GetRelativePath( Program.ShaderDirectory, FilePath ), null )
			};

			shaderTemplate.Parameters.EnsureCapacity( MaterialParameters.Count + MaterialParameterBuffers.Count );
			foreach ( var param in MaterialParameters )
			{
				shaderTemplate.Parameters.Add( new()
				{
					Name = param.MaterialName,
					ShaderName = param.ShaderName,
					Type = param.Type,
					ResourceSetId = param.SetId,
					Level = param.Level
				} );
			}
			foreach ( var param in MaterialParameterBuffers )
			{
				shaderTemplate.Parameters.Add( new()
				{
					Name = param.MaterialName,
					ShaderName = param.ShaderName,
					Type = ShaderDataType.Buffer,
					ResourceSetId = param.SetId,
					Level = param.Level
				} );
			}

			shaderTemplate.ShaderVariants.EnsureCapacity( Permutations.Count );
			foreach ( var permutation in Permutations )
			{
				shaderTemplate.ShaderVariants.Add( new()
				{
					ShaderDefine = permutation.Variant,
					VertexLayouts = ExtractVertexLayoutData( permutation.Variant ),
					ResourceLayouts = ExtractResourceLayoutData( permutation.Variant )
				} );
			}

			return shaderTemplate;
		}

		private List<VertexLayoutEntry> ExtractVertexLayoutData( string variant )
		{
			List<VertexLayoutEntry> result = new();
			foreach ( var vertexInput in VertexInputs )
			{
				if ( IsVisibleTo( variant, vertexInput.VariantMask ) )
				{
					result.Add( new()
					{
						Name = vertexInput.Name,
						Type = RenderBackend.Utils.ShaderTypeToVertexElementFormat( vertexInput.Name, vertexInput.Type )
					} );
				}
			}

			return result;
		}

		private MaterialParameterLevel DetermineMaterialParameterLevel( string variant, int setId )
		{
			var list = MaterialParameters
				.Where( param => IsVisibleTo( variant, param.VariantMask ) && param.SetId == setId );

			MaterialParameterLevel level = list.First().Level;
			foreach ( var item in list )
			{
				if ( level > item.Level )
				{
					Console.WriteLine( $"WARNING: Mixed material parameter levels: {item.MaterialName} (set {item.SetId} binding {item.BindingId}), choosing the next biggest one" );
					level = item.Level;
				}
			}

			return level;
		}

		private List<ResourceLayoutEntry> ExtractResourceLayoutData( string variant )
		{
			List<ResourceLayoutEntry> result = new();
			List<int> uniqueSets = new();
			foreach ( var param in MaterialParameters )
			{
				if ( IsVisibleTo( variant, param.VariantMask ) )
				{
					if ( !uniqueSets.Contains( param.SetId ) )
					{
						uniqueSets.Add( param.SetId );
					}
				}
			}

			foreach ( var setId in uniqueSets )
			{
				var elements = MaterialParameters
						.Where( param => IsVisibleTo( variant, param.VariantMask ) && param.SetId == setId )
						.Select( param => new ResourceLayoutElementEntry()
						{
							Name = param.MaterialName,
							Binding = param.BindingId,
							Type = param.Type
						} ).ToList();

				result.Add( new()
				{
					Set = setId,
					Elements = elements,
					Level = DetermineMaterialParameterLevel( variant, setId )
				} );
			}

			// Argh not a fan of this copy-pasting
			uniqueSets.Clear();
			foreach ( var param in MaterialParameterBuffers )
			{
				if ( IsVisibleTo( variant, param.VariantMask ) )
				{
					if ( !uniqueSets.Contains( param.SetId ) )
					{
						uniqueSets.Add( param.SetId );
					}
				}
			}

			foreach ( var setId in uniqueSets )
			{
				result.Add( new()
				{
					Set = setId,
					Elements = MaterialParameterBuffers
						.Where( param => IsVisibleTo( variant, param.VariantMask ) )
						.Where( param => param.SetId == setId )
						.Select( param => new ResourceLayoutElementEntry()
						{
							Name = param.MaterialName,
							Binding = param.BindingId,
							Type = ShaderDataType.Buffer
						} ).ToList()
				} );
			}

			result.Sort( ( x, y ) => x.Set.CompareTo( y.Set ) );

			return result;
		}

		private static bool IsVisibleTo( string shaderVariant, string[] variantMask )
			=> variantMask.Contains( shaderVariant );

		private string EmitShaderRegionGlsl( string shaderVariant, ShaderKind shaderKind, bool emitMainFunction = false )
		{
			bool isVertex = shaderKind == ShaderKind.Vertex;

			StringBuilder sb = new();

			string source = isVertex ? VertexShaderRegions[shaderVariant] : PixelShaderRegions[shaderVariant];

			foreach ( var variant in ShaderVariants )
			{
				if ( source.Contains( variant ) )
				{
					source = source.Replace( variant, isVertex ? $"{variant}_vs" : $"{variant}_ps" );
					sb.AppendLine( EmitShaderRegionGlsl( variant, shaderKind ) );
				}
			}

			sb.AppendLine( isVertex ? $"void {shaderVariant}_vs()" : $"vec4 {shaderVariant}_ps()" );
			sb.AppendLine( "{" );
			sb.AppendLine( source );
			sb.AppendLine( "}" );

			sb.AppendLine();

			if ( emitMainFunction )
			{
				if ( isVertex )
				{
					sb.AppendLine( $"void main_vs()" );
					sb.AppendLine( "{" );
					sb.AppendLine( $"	{shaderVariant}_vs();" );
					sb.AppendLine( "}" );
				}
				else
				{
					sb.AppendLine( $"void main_ps()" );
					sb.AppendLine( "{" );
					sb.AppendLine( $"	{PixelShaderOutput} = {shaderVariant}_ps();" );
					sb.AppendLine( "}" );
				}

				sb.AppendLine();
			}

			return sb.ToString();
		}

		private string EmitGlslForPermutation( string shaderVariant, ShaderKind shaderKind )
		{
			StringBuilder sb = new();

			// Step 1: common shader contents
			sb.AppendLine();
			sb.AppendLine( "#version 460" );
			sb.AppendLine();
			sb.AppendLine( "#extension GL_GOOGLE_include_directive : enable" );
			sb.AppendLine();

			// Step 2: parametres, inputs etc.
			if ( shaderKind == ShaderKind.Vertex )
			{
				foreach ( var vertexInput in VertexInputs )
				{
					if ( IsVisibleTo( shaderVariant, vertexInput.VariantMask ) )
					{
						sb.AppendLine(
							$"layout( location = {vertexInput.LocationId} ) in {vertexInput.TypeGlslName} {vertexInput.Name};" );
					}
				}

				// Just to make it more readable in case of troubleshooting etc.
				sb.AppendLine();
			}

			// Pixel shader output
			if ( shaderKind == ShaderKind.Pixel )
			{
				sb.AppendLine(
					$"layout( location = 0 ) out vec4 {PixelShaderOutput};" );
			}

			string vertexToPixelKeyword = shaderKind == ShaderKind.Vertex ? "out" : "in";
			foreach ( var pixelInput in PixelInputs )
			{
				if ( IsVisibleTo( shaderVariant, pixelInput.VariantMask ) )
				{
					sb.AppendLine(
						$"layout( location = {pixelInput.LocationId} ) {vertexToPixelKeyword} {pixelInput.TypeGlslName} {pixelInput.Name};" );
				}
			}
			sb.AppendLine();

			// Step 3: material parametres
			foreach ( var parameter in MaterialParameters )
			{
				if ( IsVisibleTo( shaderVariant, parameter.VariantMask ) )
				{
					sb.AppendLine(
						$"layout( set = {parameter.SetId}, binding = {parameter.BindingId} ) uniform {parameter.TypeGlslName} {parameter.ShaderName};" );
				}
			}
			sb.AppendLine();

			foreach ( var bufferParameter in MaterialParameterBuffers )
			{
				if ( IsVisibleTo( shaderVariant, bufferParameter.VariantMask ) )
				{
					sb.AppendLine( $"struct {bufferParameter.ShaderName}_t" );
					sb.Append( "{" );
					sb.Append( bufferParameter.StructContents );
					sb.AppendLine( "};" );

					sb.AppendLine(
						$"layout( set = {bufferParameter.SetId}, binding = {bufferParameter.BindingId} ) uniform _{bufferParameter.ShaderName}" );
					sb.AppendLine( "{" );
					sb.AppendLine( $"	{bufferParameter.ShaderName}_t {bufferParameter.ShaderName};" );
					sb.AppendLine( "};" );
				}
			}
			sb.AppendLine();

			// Step 4: more common stuff, this time utility functions and whatnot
			sb.AppendLine( CommonShaderContents );
			sb.AppendLine();

			// Step 5: vertex or pixel shader code
			sb.AppendLine( EmitShaderRegionGlsl( shaderVariant, shaderKind, true ) );

			return sb.ToString();
		}

		#region Parsing stuff
		private bool ValidateName( string name )
		{
			return !Lexer.DelimitersFull.Contains( name );
		}
		private void Expect( Lexer lexer, string token )
		{
			if ( !lexer.Expect( token, true ) )
			{
				throw lexer.ParsingException( $"Expected '{token}'" );
			}
		}

		private string ParseSimpleNameDeclaration( Lexer lexer )
		{
			Expect( lexer, "(" );

			string token = lexer.Next();
			if ( !ValidateName( token ) )
			{
				throw lexer.ParsingException( $"'{token}' isn't valid shader variant" );
			}

			Expect( lexer, ")" );

			lexer.Expect( ";", true );

			return token;
		}

		private string[] ParseShaderVariants( Lexer lexer )
		{
			Expect( lexer, "(" );

			List<string> variants = new();
			foreach ( var token in lexer.TokenListBeforeNewline() )
			{
				if ( ValidateName( token ) )
				{
					variants.Add( token );
				}
			}

			return variants.ToArray();
		}

		private MaterialParameterLevel ParseMaterialParameterLevel( string token )
		{
			return token.ToLower() switch
			{
				"builtin" => MaterialParameterLevel.Builtin,
				"data" => MaterialParameterLevel.Data,
				"global" => MaterialParameterLevel.Global,
				"instance" => MaterialParameterLevel.Instance,
				_ => throw new Exception( $"Unknown value '{token}'" )
			};
		}

		private ShaderDataType ParseShaderDataType( string token )
		{
			return token.ToLower() switch
			{
				"byte" => ShaderDataType.Byte,
				"short" => ShaderDataType.Short,
				"int" => ShaderDataType.Int,
				"float" => ShaderDataType.Float,
				"vec2" => ShaderDataType.Vec2,
				"vec3" => ShaderDataType.Vec3,
				"vec4" => ShaderDataType.Vec4,
				"mat2" => ShaderDataType.Mat22,
				"mat3" => ShaderDataType.Mat33,
				"mat4" => ShaderDataType.Mat44,
				"buffer" => ShaderDataType.Buffer,
				"bufferrw" => ShaderDataType.BufferRW,
				"texture1d" => ShaderDataType.Texture1D,
				"texture2d" => ShaderDataType.Texture2D,
				"texture3d" => ShaderDataType.Texture3D,
				"sampler" => ShaderDataType.Sampler,
				_ => throw new Exception( $"Unknown value '{token}'" )
			};
		}

		private string[] ParseVariantMask( Lexer lexer )
		{
			List<string> result = new();

			var isValidName = ( string name ) =>
			{
				return ShaderVariants.Contains( name );
			};

			var modifyVariantMask = ( string name, bool remove ) =>
			{
				Debug.Assert( isValidName( name ) );
				if ( remove )
				{
					result.Remove( name );
				}
				else
				{
					result.Add( name );
				}
			};

			bool exclusionMode = false;
			while ( !lexer.Expect( ")" ) && !lexer.Expect( "," ) )
			{
				string token = lexer.Next();
				if ( token == "ALL" )
				{
					foreach ( var variant in ShaderVariants )
					{
						modifyVariantMask( variant, exclusionMode );
					}
				}
				else if ( token.ToLower() == "except" )
				{
					exclusionMode = true;
				}
				else if ( token.ToLower() == "with" )
				{
					exclusionMode = false;
				}
				else if ( isValidName( token ) )
				{
					modifyVariantMask( token, exclusionMode );
				}
				else
				{
					lexer.ParsingException( $"Unknown token '{token}', expected a shader variant, ')', 'except' or 'with'" );
				}
			}

			return result.ToArray();
		}

		private GlslMaterialParameter ParseMaterialParameter( Lexer lexer )
		{
			Expect( lexer, "(" );
			int setId = Parse.Int( lexer.Next() );
			Expect( lexer, "," );
			int bindingId = Parse.Int( lexer.Next() );
			Expect( lexer, "," );
			ShaderDataType shaderDataType = ParseShaderDataType( lexer.Next() );
			Expect( lexer, "," );
			string shaderName = lexer.Next();
			Expect( lexer, "," );
			string materialName = lexer.Next();
			Expect( lexer, "," );
			string[] variantMask = ParseVariantMask( lexer );
			Expect( lexer, "," );
			MaterialParameterLevel materialParameterLevel = ParseMaterialParameterLevel( lexer.Next() );
			Expect( lexer, ")" );
			lexer.Expect( ";", true );
			return new()
			{
				SetId = setId,
				BindingId = bindingId,
				Type = shaderDataType,
				ShaderName = shaderName,
				MaterialName = materialName,
				VariantMask = variantMask,
				Level = materialParameterLevel
			};
		}

		private GlslMaterialParameterBuffer ParseMaterialParameterBuffer( Lexer lexer )
		{
			Expect( lexer, "(" );
			int setId = Parse.Int( lexer.Next() );
			Expect( lexer, "," );
			int bindingId = Parse.Int( lexer.Next() );
			Expect( lexer, "," );
			string shaderName = lexer.Next();
			Expect( lexer, "," );
			string materialName = lexer.Next();
			Expect( lexer, "," );
			string[] variantMask = ParseVariantMask( lexer );
			Expect( lexer, "," );
			MaterialParameterLevel materialParameterLevel = ParseMaterialParameterLevel( lexer.Next() );
			Expect( lexer, "," );
			string structContents = lexer.PeekUntil( ")", skipPeeked: true, skipWhatToo: false );
			Expect( lexer, ")" );
			lexer.Expect( ";", true );
			return new()
			{
				SetId = setId,
				BindingId = bindingId,
				ShaderName = shaderName,
				MaterialName = materialName,
				VariantMask = variantMask,
				StructContents = structContents,
				Level = materialParameterLevel
			};
		}

		private GlslInput ParseShaderInput( Lexer lexer )
		{
			Expect( lexer, "(" );
			int setId = Parse.Int( lexer.Next() );
			Expect( lexer, "," );
			ShaderDataType shaderDataType = ParseShaderDataType( lexer.Next() );
			Expect( lexer, "," );
			string shaderName = lexer.Next();
			Expect( lexer, "," );
			string[] variantMask = ParseVariantMask( lexer );
			Expect( lexer, ")" );
			lexer.Expect( ";", true );
			return new()
			{
				LocationId = setId,
				Type = shaderDataType,
				Name = shaderName,
				VariantMask = variantMask
			};
		}

		private void ParseShaderRegion( Lexer lexer, bool isVertex )
		{
			Expect( lexer, "(" );

			string shaderVariantName = lexer.Next();

			Expect( lexer, "," );

			// It may be better to use a stack, but this is faster and probably won't break
			StringBuilder sb = new( 64 );
			int numOpenParentheses = 1;
			int numClosedParentheses = 0;

			while ( !lexer.IsEnd() )
			{
				string token = lexer.Next();
				if ( token == "(" )
				{
					numOpenParentheses++;
				}
				else if ( token == ")" )
				{
					numClosedParentheses++;
				}

				if ( numOpenParentheses == numClosedParentheses )
				{
					break;
				}

				sb.Append( token );
				if ( "{};".Contains( token ) )
				{
					sb.AppendLine();
				}
				else
				{
					sb.Append( ' ' );
				}
			}

			if ( numOpenParentheses != numClosedParentheses )
			{
				throw lexer.ParsingException( "You are missing a ')' somewhere" );
			}

			if ( isVertex )
			{
				VertexShaderRegions[shaderVariantName] = sb.ToString();
			}
			else
			{
				PixelShaderRegions[shaderVariantName] = sb.ToString();
			}
		}
		#endregion
	}
}
