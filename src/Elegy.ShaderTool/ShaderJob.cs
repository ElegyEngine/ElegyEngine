// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Text;

namespace Elegy.ShaderTool
{
	public class ShaderJob
	{
		public ShaderJob( string shaderFile, string shaderOutput, string shaderContents, string arguments )
		{
			Arguments = arguments;
			ShaderFile = shaderFile;
			ShaderContents = shaderContents;
			ShaderOutput = shaderOutput;
		}

		private static string GenerateArguments( string shaderTypeShort, string shaderType, string includeDirectory )
			=>  $"--target-env vulkan1.3 " +
				$"-S {shaderTypeShort} " +
				$"-DSHADER_TYPE=SHADER_{shaderType.ToUpper()} " +
				$"-I\"{includeDirectory}\" " +
				$"-e \"main_{shaderType[0]}s\" " +
				$"--sep \"main\"";

		public static ShaderJob ForVertexShader( string shaderDirectory, string shaderFile, string shaderOutput, string shaderContents )
			=> new( shaderFile,
				shaderOutput,
				shaderContents,
				GenerateArguments( "vert", "vertex", shaderDirectory ) );

		public static ShaderJob ForPixelShader( string shaderDirectory, string shaderFile, string shaderOutput, string shaderContents )
			=> new( shaderFile,
				shaderOutput,
				shaderContents,
				GenerateArguments( "frag", "pixel", shaderDirectory ) );

		public static ShaderJob ForComputeShader( string shaderDirectory, string shaderFile, string shaderOutput, string shaderContents )
			=> new( shaderFile,
				shaderOutput,
				shaderContents,
				GenerateArguments( "comp", "compute", shaderDirectory ) );

		private StringBuilder mErrorLogger = new();

		public string Arguments { get; }
		public string ShaderContents { get; }
		public string ShaderFile { get; }
		public string ShaderOutput { get; }
		public string ErrorMessage => mErrorLogger.ToString();

		public bool Run( bool force )
		{
			// Comparing old and new contents for changes
			if ( File.Exists( ShaderFile ) && !force )
			{
				if ( File.ReadAllText( ShaderFile ) == ShaderContents )
				{
					return true;
				}
			}

			// Write down the .vert or .frag
			File.WriteAllText( ShaderFile, ShaderContents );

			// glslangValidator will handle the actual shader compilation
			Process? process = Process.Start( new ProcessStartInfo()
			{
				Arguments = $"{Arguments} -o \"{ShaderOutput}\" \"{ShaderFile}\"",
				FileName = "glslang",

				CreateNoWindow = false,
				ErrorDialog = false,

				RedirectStandardError = true,
				RedirectStandardOutput = true,

				WorkingDirectory = Directory.GetCurrentDirectory()
			} );

			if ( process is null )
			{
				mErrorLogger.Append( $"Cannot launch glslang on '{Arguments} {ShaderFile}'" );
				return false;
			}

			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			process.ErrorDataReceived += OnProcessError;
			process.OutputDataReceived += OnProcessMessage;

			process.WaitForExit();

			if ( mErrorLogger.Length != 0 )
			{
				mErrorLogger.AppendLine( $"\t^ while compiling '{ShaderFile}' with arguments:" );
				mErrorLogger.AppendLine( $"\t{Arguments}" );
			}

			return mErrorLogger.Length == 0;
		}

		private void OnProcessError( object sender, DataReceivedEventArgs e )
		{
			if ( e.Data is null )
			{
				return;
			}

			mErrorLogger.AppendLine( e.Data );
		}

		private void OnProcessMessage( object sender, DataReceivedEventArgs e )
		{

			if ( e.Data is null )
			{
				return;
			}

			if ( e.Data.ToLower().Contains( "warning" ) )
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine( e.Data );
				Console.ResetColor();
				return;
			}
			
			if ( e.Data.StartsWith( "ERROR" ) )
			{
				OnProcessError( sender, e );
			}
		}
	}
}
