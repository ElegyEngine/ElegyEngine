// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Elegy.Bootstrap.Generator
{
	internal record ApplicationBootloaderData()
	{
		public string ClassName { get; set; } = string.Empty;
		public string Namespace { get; set; } = string.Empty;

		private bool mUseAssetSystem = false;
		private bool mUseAudioSystem = false;
		private bool mUseConsoleSystem = false;
		private bool mUseFileSystem = false;
		private bool mUseInputSystem = false;
		private bool mUsePlatformSystem = false;
		private bool mUsePluginSystem = false;
		private bool mUseRenderSystem = false;

		public bool WithAssetSystem
		{
			get => mUseAssetSystem;
			set
			{
				if ( value )
				{
					WithPluginSystem = true;
				}

				mUseAssetSystem = value;
			}
		}

		public bool WithAudioSystem
		{
			get => mUseAudioSystem;
			set
			{
				if ( value )
				{
					WithAssetSystem = true;
				}

				mUseAudioSystem = false;
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
					mUseConsoleSystem = true;
				}

				mUseFileSystem = value;
			}
		}

		public bool WithPlatformSystem
		{
			get => mUsePlatformSystem;
			set
			{
				if ( value )
				{
					WithPluginSystem = true;
				}

				mUsePlatformSystem = value;
			}
		}

		public bool WithInputSystem
		{
			get => mUseInputSystem;
			set
			{
				if ( value )
				{
					WithPlatformSystem = true;
				}

				mUseInputSystem = value;
			}
		}

		public bool WithPluginSystem
		{
			get => mUsePluginSystem;
			set
			{
				if ( value )
				{
					WithFileSystem = true;
				}

				mUsePluginSystem = value;
			}
		}

		public bool WithRenderSystem
		{
			get => mUseRenderSystem;
			set
			{
				if ( value )
				{
					WithAssetSystem = true;
				}
			
				mUseRenderSystem = value;
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

			var validateInit = ( string message, string system, string name ) =>
			{
				sb.AppendLine( $"		if ( !Elegy.{system}.API.{name}.Init( config ) )" );
				sb.AppendLine( "		{" );
				sb.AppendLine( $"			mErrorMessage_Generated = \"{message} failure\";" );
				sb.AppendLine( "			return false;" );
				sb.AppendLine( "		}" );
			};

			if ( data.WithConsoleSystem )	validateInit( "console system", "ConsoleSystem", "Console" );
			if ( data.WithFileSystem )		validateInit( "virtual filesystem", "FileSystem", "Files" );
			if ( data.WithPluginSystem )	validateInit( "plugin system", "PluginSystem", "Plugins" );
			if ( data.WithAssetSystem )		validateInit( "asset system", "AssetSystem", "Assets" );
			if ( data.WithPlatformSystem )	validateInit( "platform system", "PlatformSystem", "Platform" );
			if ( data.WithInputSystem )		validateInit( "input system", "InputSystem", "Input" );
			if ( data.WithRenderSystem )	validateInit( "render system", "RenderSystem", "Render" );

			return sb.ToString();
		}

		private static string GeneratePostInits( ApplicationBootloaderData data )
		{
			StringBuilder sb = new();

			var validatePostInit = ( string message, string system, string name ) =>
			{
				sb.AppendLine( $"		if ( !Elegy.{system}.API.{name}.PostInit() )" );
				sb.AppendLine( "		{" );
				sb.AppendLine( $"			mErrorMessage_Generated = \"{message} failure\";" );
				sb.AppendLine( "			return false;" );
				sb.AppendLine( "		}" );
			};

			if ( data.WithPluginSystem ) validatePostInit( "plugin system", "PluginSystem", "Plugins" );
			if ( data.WithRenderSystem ) validatePostInit( "render system", "RenderSystem", "Render" );
			// Normally the asset system would go before the renderer here, but when the renderer is
			// post-initialising, it doesn't need anything from the asset system. Rather it's the opposite.
			if ( data.WithAssetSystem ) validatePostInit( "asset system", "AssetSystem", "Assets" );

			return sb.ToString();
		}

		private static string GenerateShutdowns( ApplicationBootloaderData data )
		{
			StringBuilder sb = new();

			if ( data.WithRenderSystem )	{ sb.AppendLine( "		Elegy.RenderSystem.API.Render.Shutdown();" ); }
			if ( data.WithInputSystem )		{ sb.AppendLine( "		Elegy.InputSystem.API.Input.Shutdown();" ); }
			if ( data.WithPlatformSystem )	{ sb.AppendLine( "		Elegy.PlatformSystem.API.Platform.Shutdown();" ); }
			if ( data.WithAssetSystem )		{ sb.AppendLine( "		Elegy.AssetSystem.API.Assets.Shutdown();" ); }
			if ( data.WithPluginSystem )	{ sb.AppendLine( "		Elegy.PluginSystem.API.Plugins.Shutdown();" ); }
			if ( data.WithFileSystem )		{ sb.AppendLine( "		Elegy.FileSystem.API.Files.Shutdown();" ); }
			if ( data.WithConsoleSystem )	{ sb.AppendLine( "		Elegy.ConsoleSystem.API.Console.Shutdown();" ); }

			return sb.ToString();
		}

		private string GetNamespace( INamedTypeSymbol symbol )
		{
			List<string> result = new( 8 );

			INamespaceSymbol namespaceSymbol = symbol.ContainingNamespace;
			while ( namespaceSymbol is not null )
			{
				result.Add( namespaceSymbol.Name );
				namespaceSymbol = namespaceSymbol.ContainingNamespace;
			}

			StringBuilder sb = new( result.Count );
			for ( int i = result.Count - 1; i >= 0; i-- )
			{
				sb.Append( result[i] );
				if ( i != 0 && i != result.Count - 1 )
				{
					sb.Append( '.' );
				}
			}

			return sb.ToString();
		}

		public void Initialize( IncrementalGeneratorInitializationContext context )
		{
			var data = context.SyntaxProvider.ForAttributeWithMetadataName(
				fullyQualifiedMetadataName: "Elegy.Framework.Bootstrap.ElegyBootstrapAttribute",
				predicate: ( syntaxNode, cancellationToken ) => syntaxNode is ClassDeclarationSyntax,
				transform: ( syntaxContext, cancellationToken ) =>
				{
					ApplicationBootloaderData data = new();

					Debug.Assert( syntaxContext.TargetSymbol is INamedTypeSymbol );
					var classSymbol = syntaxContext.TargetSymbol as INamedTypeSymbol;

					data.ClassName = classSymbol.Name;
					data.Namespace = GetNamespace( classSymbol );

					foreach ( var attribute in classSymbol.GetAttributes() )
					{
						switch ( attribute.AttributeClass.Name )
						{
							case "WithAssetSystemAttribute": data.WithAssetSystem = true; break;
							case "WithAudioSystemAttribute": data.WithAudioSystem = true; break;
							case "WithConsoleSystemAttribute": data.WithConsoleSystem = true; break;
							case "WithFileSystemAttribute": data.WithFileSystem = true; break;
							case "WithPlatformSystemAttribute": data.WithInputSystem = true; break;
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
					productionContext.AddSource( $"Generated/{sourceData.ClassName}.generated.cs", SourceText.From(
						$$"""
						{{GenerateUsings( sourceData )}}

						using Elegy.Common.Assets;

						namespace {{sourceData.Namespace}};

						public static partial class {{sourceData.ClassName}}
						{
							static string mErrorMessage_Generated = string.Empty;

							internal static bool Init_Generated( LaunchConfig config )
							{
						{{GenerateInits( sourceData )}}

								return true;
							}
						
							internal static bool PostInit_Generated()
							{
						{{GeneratePostInits( sourceData )}}

								return true;
							}

							internal static void Shutdown_Generated()
							{
						{{GenerateShutdowns( sourceData )}}
							}

							internal static string ErrorMessage_Generated()
							{
								return mErrorMessage_Generated;
							}
						}
						""", encoding: Encoding.ASCII ) );
				} );
		}
	}
}
