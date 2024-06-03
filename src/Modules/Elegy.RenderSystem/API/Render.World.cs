// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.API;
using Elegy.Common.Assets;
using Elegy.Common.Assets.ElegyMapData;
using Elegy.Common.Extensions;
using Elegy.RenderSystem.Objects;
using Elegy.RenderSystem.Resources;

using Silk.NET.Windowing;
using System.Diagnostics;

namespace Elegy.RenderSystem.API
{
	public static partial class Render
	{
		/// <summary>Creates a render batch.</summary>
		public static Batch CreateBatch()
		{
			throw new NotImplementedException();
		}

		/// <summary>Frees a render batch.</summary>
		public static bool FreeBatch( ref Batch? batch )
		{
			throw new NotImplementedException();
		}

		/// <summary>Creates a billboard.</summary>
		public static Billboard CreateBillboard()
		{
			throw new NotImplementedException();
		}

		/// <summary>Frees a billboard.</summary>
		public static bool FreeBillboard( ref Billboard? billboard )
		{
			throw new NotImplementedException();
		}

		/// <summary>Creates a render entity.</summary>
		public static MeshEntity CreateEntity( bool animated )
			=> mEntitySet.AddAndGet( new( mDevice, animated, Layouts.PerEntity ) );

		/// <summary>Frees a render entity.</summary>
		public static bool FreeEntity( ref MeshEntity? entity )
			=> mEntitySet.RemoveAndThen( entity, ( entity ) =>
			{
				entity.Dispose();
				entity = null;
			} );

		/// <summary>Creates a render light.</summary>
		public static Light CreateLight()
		{
			throw new NotImplementedException();
		}

		/// <summary>Frees a render light.</summary>
		public static bool FreeLight( ref Light? light )
		{
			throw new NotImplementedException();
		}

		/// <summary>Creates a render material.</summary>
		public static RenderMaterial? CreateMaterial( MaterialDefinition materialDefinition )
			=> HasMaterialTemplate( materialDefinition.TemplateName ) switch
			{
				true => mMaterialSet.AddAndGet(
					new( mDevice, materialDefinition, GetMaterialTemplate( materialDefinition.TemplateName ) ) ),
				false => null
			};

		/// <summary>Loads or finds a render material by name.</summary>
		public static RenderMaterial LoadMaterial( string name )
		{
			var material = Assets.LoadMaterial( name );

			Debug.Assert( material is not null );
			Debug.Assert( material.RenderMaterial is not null );

			return (RenderMaterial)material.RenderMaterial;
		}

		/// <summary>Frees a render material.</summary>
		public static bool FreeMaterial( ref RenderMaterial? material )
			=> mMaterialSet.RemoveAndThen( material, material =>
			{
				material.Dispose();
				material = null;
			} );

		/// <summary>Creates a render texture.</summary>
		public static RenderTexture CreateTexture( TextureMetadata metadata, Span<byte> data )
			=> mTextureSet.AddAndGet( new( mDevice, metadata, data ) );

		/// <summary>Frees a render texture.</summary>
		public static bool FreeTexture( ref RenderTexture? texture )
			=> mTextureSet.RemoveAndThen( texture, texture =>
			{
				texture.Dispose();
				texture = null;
			} );

		/// <summary>Creates a render mesh.</summary>
		public static Mesh CreateMesh( Model modelData )
			=> mMeshSet.AddAndGet( new( mDevice, modelData ) );

		/// <summary>Creates/loads a render mesh from a file.</summary>
		public static Mesh LoadMesh( string name )
		{
			var mesh = Assets.LoadModel( name );
			Debug.Assert( mesh is not null );
			return CreateMesh( mesh );
		}

		/// <summary>Frees a render mesh.</summary>
		public static bool FreeMesh( ref Mesh? mesh )
			=> mMeshSet.RemoveAndThen( mesh, mesh =>
			{
				mesh.Clear();
				mesh = null;
			} );

		/// <summary>Creates a render view from a window.</summary>
		public static View CreateView( IWindow window )
			=> mViews.AddAndGet( new( mDevice, window ) );

		/// <summary>Creates a render view from a render target texture.</summary>
		public static View CreateView( AssetSystem.Resources.Texture renderTarget )
			=> mViews.AddAndGet( new( mDevice, renderTarget.RenderTexture ) );

		/// <summary>Gets the window's view, if any.</summary>
		public static View? GetView( IWindow window )
		{
			foreach ( View view in mViews )
			{
				if ( view.Window == window )
				{
					return view;
				}
			}

			return null;
		}

		/// <summary>Frees a view.</summary>
		public static bool FreeView( ref View? view )
			=> mViews.RemoveAndThen( view, ( view ) =>
			{
				view.Dispose();
				view = null;
			} );

		/// <summary>Creates a renderable volume.</summary>
		public static Volume CreateVolume()
		{
			throw new NotImplementedException();
		}

		/// <summary>Frees a renderable volume.</summary>
		public static bool FreeVolume( ref Volume? volume )
		{
			throw new NotImplementedException();
		}
	}
}
