// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Elegy.Bootstrapper
{
	internal record ApplicationBootloaderData()
	{
		private bool mUseAssetSystem = false;
		private bool mUseAudioSystem = false;
		private bool mUseConsoleSystem = false;
		private bool mUseFileSystem = false;
		private bool mUseInputSystem = false;
		private bool mUsePluginSystem = false;
		private bool mUseRenderSystem = false;

		public bool WithAssetSystem
		{
			get => mUseAssetSystem;
			set
			{
				if ( value )
				{
					mUseAssetSystem = true;
					mUsePluginSystem = true;
					mUseFileSystem = true;
					mUseConsoleSystem = true;
				}
				else
				{
					mUseAssetSystem = false;
				}
			}
		}

		public bool WithAudioSystem
		{
			get => mUseAudioSystem;
			set
			{
				if ( value )
				{
					mUseConsoleSystem = true;
					mUseFileSystem = true;
					mUsePluginSystem = true;
					mUseAssetSystem = true;
					mUseAudioSystem = true;
				}
				else
				{
					mUseAudioSystem = false;
				}
			}
		}

		public bool WithConsoleSystem
		{
			get => mUseConsoleSystem;
			set => mUseConsoleSystem = value;
		}

		public bool WithFileSystem
		{
			get => mUseFileSystem;
			set
			{
				if ( value )
				{
					mUseFileSystem = true;
					mUseConsoleSystem = true;
				}
				else
				{
					mUseFileSystem = false;
				}
			}
		}

		public bool WithInputSystem
		{
			get => mUseInputSystem;
			set => mUseInputSystem = value;
		}

		public bool WithPluginSystem
		{
			get => mUsePluginSystem;
			set
			{
				if ( value )
				{
					mUsePluginSystem = true;
					mUseFileSystem = true;
					mUseConsoleSystem = true;
				}
				else
				{
					mUsePluginSystem = false;
				}
			}
		}

		public bool WithRenderSystem
		{
			get => mUseRenderSystem;
			set
			{
				if ( value )
				{
					mUseConsoleSystem = true;
					mUseFileSystem = true;
					mUsePluginSystem = true;
					mUseAssetSystem = true;
					mUseRenderSystem = true;
				}
				else
				{
					mUseRenderSystem = false;
				}
			}
		}
	}

	/// <summary>
	/// 
	/// </summary>
	[Generator]
	public class BootstrapGenerator : IIncrementalGenerator
	{
		private static string GenerateUsings( ApplicationBootloaderData data )
		{
			StringBuilder sb = new();

			if ( data.WithConsoleSystem )	{ sb.AppendLine( "using Elegy.ConsoleSystem.API;" ); }
			if ( data.WithFileSystem )		{ sb.AppendLine( "using Elegy.FileSystem.API;" ); }
			if ( data.WithPluginSystem )	{ sb.AppendLine( "using Elegy.PluginSystem.API;" ); }
			if ( data.WithAssetSystem )		{ sb.AppendLine( "using Elegy.AssetSystem.API;" ); }

			return sb.ToString();
		}

		private static string GenerateInits( ApplicationBootloaderData data )
		{
			StringBuilder sb = new();

			if ( data.WithConsoleSystem )	{ sb.AppendLine( "		Validate( \"console system\",      Elegy.ConsoleSystem.API.Console.Init );" ); }
			if ( data.WithFileSystem )		{ sb.AppendLine( "		Validate( \"virtual file system\", Elegy.FileSystem.API.Files.Init );" ); }
			if ( data.WithPluginSystem )	{ sb.AppendLine( "		Validate( \"plugin system\",       Elegy.PluginSystem.API.Plugins.Init );" ); }
			if ( data.WithAssetSystem )		{ sb.AppendLine( "		Validate( \"asset system\",        Elegy.AssetSystem.API.Assets.Init );" ); }

			return sb.ToString();
		}

		private static string GeneratePostInits( ApplicationBootloaderData data )
		{
			StringBuilder sb = new();

			if ( data.WithAssetSystem ) { sb.AppendLine( "		Validate( \"asset system\", Elegy.AssetSystem.API.Assets.Init );" ); }

			return sb.ToString();
		}

		private static string GeneratePreShutdowns( ApplicationBootloaderData data )
		{
			StringBuilder sb = new();

			if ( data.WithAssetSystem ) { sb.AppendLine( "		Elegy.AssetSystem.API.Assets.Shutdown();" ); }

			return sb.ToString();
		}

		private static string GenerateShutdowns( ApplicationBootloaderData data )
		{
			StringBuilder sb = new();

			if ( data.WithAssetSystem )		{ sb.AppendLine( "		Elegy.AssetSystem.API.Assets.Shutdown();" ); }
			if ( data.WithPluginSystem )	{ sb.AppendLine( "		Elegy.PluginSystem.API.Plugins.Shutdown();" ); }
			if ( data.WithFileSystem )		{ sb.AppendLine( "		Elegy.FileSystem.API.Files.Shutdown();" ); }
			if ( data.WithConsoleSystem )	{ sb.AppendLine( "		Elegy.ConsoleSystem.API.Console.Shutdown();" ); }

			return sb.ToString();
		}

		public void Initialize( IncrementalGeneratorInitializationContext context )
		{
			var data = context.SyntaxProvider.ForAttributeWithMetadataName(
				fullyQualifiedMetadataName: "Elegy.Bootstrapper.ElegyMainAttribute",
				predicate: ( syntaxNode, cancellationToken ) => syntaxNode is MethodDeclarationSyntax,
				transform: ( syntaxContext, cancellationToken ) =>
				{
					ApplicationBootloaderData data = new();

					var elegyMainMethod = syntaxContext.TargetSymbol as IMethodSymbol;
					foreach ( var attribute in elegyMainMethod.GetAttributes() )
					{
						switch ( attribute.AttributeClass.Name )
						{
							case "WithAssetSystemAttribute": data.WithAssetSystem = true; break;
							case "WithAudioSystemAttribute": data.WithAudioSystem = true; break;
							case "WithConsoleSystemAttribute": data.WithConsoleSystem = true; break;
							case "WithFileSystemAttribute": data.WithFileSystem = true; break;
							case "WithInputSystemAttribute": data.WithInputSystem = true; break;
							case "WithPluginSystemAttribute": data.WithPluginSystem = true; break;
							case "WithRenderSystemAttribute": data.WithRenderSystem = true; break;

							case "WithAllGameSystemsAttribute":
								// These 3 should cascade downwards fine enough
								data.WithRenderSystem = true;
								data.WithAudioSystem = true;
								data.WithInputSystem = true;
								break;

							case "WithMinimalToolSystemsAttribute":
								// Tools typically work with assets, and this pulls
								// the plugin system, file system etc. with it
								data.WithAssetSystem = true;
								break;

							default: break;
						}
					}

					return data;
				} );

			context.RegisterSourceOutput( data, ( productionContext, sourceData ) =>
				{
					productionContext.AddSource( "ElegyAppFramework.generated.cs", SourceText.From(
						$$"""
						{{GenerateUsings( sourceData )}}

						using Elegy.Common.Assets;

						namespace Elegy.Engine;

						public static partial class Engine
						{
							partial void Init_Generated( in LaunchConfig config )
							{
								{{GenerateInits( sourceData )}}
							}
						
							partial void PostInit_Generated( in LaunchConfig config )
							{
								{{GeneratePostInits( sourceData )}}
							}
						
							partial void PreShutdown_Generated()
							{
								{{GeneratePreShutdowns( sourceData )}}
							}

							partial void Init_Shutdown()
							{
								{{GenerateShutdowns( sourceData )}}
							}
						}
						""" ) );
				} );
		}
	}
}
