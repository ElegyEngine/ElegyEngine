// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Text;
using Elegy.RenderBackend.Assets;

namespace Elegy.ShaderTool
{
	public class Program
	{
		internal static string ShaderDirectory { get; set; } = string.Empty;

		private static bool ForceRecompile { get; set; } = false;

		public static void Main( string[] args )
		{
			if ( !ProcessArgs( args ) )
			{
				PrintUsage();
				return;
			}

			ShaderDirectory = Path.TrimEndingDirectorySeparator( ShaderDirectory );
			string[] shaderFiles = Directory.GetFileSystemEntries( ShaderDirectory, "*.glsl", SearchOption.AllDirectories );
			if ( shaderFiles.Length == 0 )
			{
				Error( $"There are no GLSL shaders in the provided {ShaderDirectory}!" );
			}

			List<MaterialParameter> globalParameters = new();

			foreach ( var shader in shaderFiles )
			{
				if ( !shader.Contains( "generated" ) )
				{
					Console.WriteLine( $"Processing shader '{shader}'..." );
					ProcessShader( shader, globalParameters );
				}
			}

			ProcessGlobalMaterialParameters( globalParameters );
		}

		internal static void Error( string errorMessage )
		{
			Console.ForegroundColor = ConsoleColor.Red;
			if ( !errorMessage.StartsWith( "ERROR:" ) )
			{
				Console.Error.WriteLine( $"ERROR: {errorMessage}" );
			}
			else
			{
				Console.Error.WriteLine( errorMessage );
			}
			Console.ResetColor();
		}

		private static void ProcessGlobalMaterialParameters( List<MaterialParameter> parameters )
		{
			List<GlobalParameter> globalParams = parameters.Select( p => new GlobalParameter()
			{
				Name = p.Name,
				Type = p.Type
			} ).ToList();

			JsonHelpers.Write( globalParams, "shaders/globalMaterialParams.json" );
		}

		private static bool AppendGlobalMaterialParameters( ShaderTemplate template, List<MaterialParameter> outGlobalParameters )
		{
			bool okay = true;

			foreach ( var parameter in template.Parameters )
			{
				if ( parameter.Level != MaterialParameterLevel.Global )
				{
					continue;
				}

				MaterialParameter? existingParam = outGlobalParameters.First( item => item.Name == parameter.Name );
				if ( existingParam is null )
				{
					outGlobalParameters.Add( parameter );
					continue;
				}

				// Mixed types are not allowed for obvious reasons
				if ( existingParam.Type != parameter.Type )
				{
					Error( "Differing global param. datatypes:" );
					Error( $"{existingParam.Type} (in memory) vs. {parameter.Type} (in shader template '{template.Name}')" );
					okay = false;
				}

				// Different set IDs are fine, but different binding IDs are not
				// You might have a situation where your global parameter consists of 2 or more resources,
				// at binding slots 0, 1, 2 etc. The order must be the same so it's consistent between
				// shader templates and whatnot, so their resource sets can be reloaded.
				if ( existingParam.ResourceBindingId != parameter.ResourceBindingId )
				{
					Error( "Differing global param. binding IDs:" );
					Error( $"{existingParam.ResourceBindingId} (in memory) vs. {parameter.ResourceBindingId} (in shader template '{template.Name}')" );
					okay = false;
				}
			}

			return okay;
		}

		private static bool ProcessShader( string path, List<MaterialParameter> outGlobalParameters )
		{
			ShaderProcessor processor = new( path, File.ReadAllText( path ) );
			if ( !processor.CreatePermutations() )
			{
				Error( "Failed to process the shader files" );
				return false;
			}

			ShaderTemplate shaderTemplate = processor.CreateShaderTemplate();

			// The compilation may fail, but the shader template will prevail
			JsonHelpers.Write( shaderTemplate, Path.ChangeExtension( path, ".stemplate" ) );

			// Collect global material params here
			AppendGlobalMaterialParameters( shaderTemplate, outGlobalParameters );

			string pathNoExtension = Path.ChangeExtension( path, null );
			foreach ( var permutation in processor.Permutations )
			{
				// engine/shaders/Shader.VARIANT
				string shaderFullName = $"{pathNoExtension}.{permutation.Variant}".Replace( '\\', '/' );
				// engine/shaders/generated/Shader.VARIANT
				string generatedShaderPath = shaderFullName.Replace( "shaders/", "shaders/generated/" );
				// engine/shaders/bin/Shader.VARIANT
				string generatedShaderBinaryPath = shaderFullName.Replace( "shaders/", "shaders/bin/" );

				// File.WriteAllText is gonna complain if the folders ain't there
				CreateDirectoriesAdHoc( generatedShaderPath );
				CreateDirectoriesAdHoc( generatedShaderBinaryPath );

				ShaderJob vertexJob = ShaderJob.ForVertexShader( ShaderDirectory, $"{generatedShaderPath}.vs.glsl", $"{generatedShaderBinaryPath}.vs.spv", permutation.VertexContents );
				ShaderJob pixelJob = ShaderJob.ForPixelShader( ShaderDirectory, $"{generatedShaderPath}.ps.glsl", $"{generatedShaderBinaryPath}.ps.spv", permutation.PixelContents );

				if ( !vertexJob.Run( ForceRecompile ) )
				{
					Error( vertexJob.ErrorMessage );
				}
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine( $"	Successfully compiled {vertexJob.ShaderOutput}" );
				Console.ResetColor();

				if ( !pixelJob.Run( ForceRecompile ) )
				{
					Error( pixelJob.ErrorMessage );
				}
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine( $"	Successfully compiled {pixelJob.ShaderOutput}" );
				Console.ResetColor();
			}

			return true;
		}

		private static void CreateDirectoriesAdHoc( string path )
		{
			string pathDirectoryOnly = path[0..path.LastIndexOf( '/' )];
			Directory.CreateDirectory( pathDirectoryOnly );
		}

		#region Boilerplate
		private static void DebugFreeze( float seconds )
		{
			Console.WriteLine( "DEVELOPER: You have entered the 'debug freeze'. The purpose of this is to freeze" );
			Console.WriteLine( $"           the app so you can attach a debugger for about {(int)seconds} seconds." );
			Console.WriteLine( "           Now go ahead. Attach the debugger while you still have the time." );

			int quarterSecondCounter = (int)seconds * 4;
			while ( quarterSecondCounter >= 0 )
			{
				Thread.Sleep( 250 );

				if ( System.Diagnostics.Debugger.IsAttached )
				{
					Console.WriteLine();
					Console.WriteLine( "Alrighty, you've attached it! Let's go now." );
					Console.WriteLine();
					Thread.Sleep( 500 );
					return;
				}
			}

			Console.WriteLine( "Welp, no debugger was attached, moving on!" );
			Console.WriteLine();
		}

		private static bool ProcessArgs( string[] args )
		{
			if ( args.Length == 0 )
			{
				Console.WriteLine( "No arguments were provided." );
				return false;
			}

			if ( args.Length == 1 && args[0] == "-help" )
			{
				Console.WriteLine( "Ah, a lost soul like myself. Let me guide you..." );
				return false;
			}

			foreach ( var arg in args )
			{
				if ( arg == "-debugfreeze" )
				{
					DebugFreeze( 20.0f );
				}
				else if ( arg == "-force" )
				{
					ForceRecompile = true;
				}
				else if ( Directory.Exists( arg ) )
				{
					ShaderDirectory = arg;
				}
				else
				{
					Error( $"Unknown parameter {arg}" );
					return false;
				}
			}

			if ( string.IsNullOrEmpty( ShaderDirectory ) )
			{
				return false;
			}

			return true;
		}

		private static void PrintUsage()
		{
			Console.WriteLine( "In order to compile shaders, you need to call me like so:" );
			Console.WriteLine( "> Elegy.ShaderTool \"path/to/shaders\"" );
			Console.WriteLine();
			Console.WriteLine( "Now, here's a list of all parameters:" );
			Console.WriteLine();
			Console.WriteLine( "| NAME          | DESCRIPTION                                                          |" );
			Console.WriteLine();
			Console.WriteLine( " -debugfreeze:    Freezes the program for 20 seconds, so you can attach a debugger." );
			Console.WriteLine();
			Console.WriteLine( "This compiler is in a very early state, so this is all we have at the moment." );
		}
		#endregion
	}
}
