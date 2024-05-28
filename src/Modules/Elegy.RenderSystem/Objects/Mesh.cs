// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.RenderSystem.API;
using Elegy.RenderBackend;
using Veldrid;
using Elegy.RenderSystem.Resources;

namespace Elegy.RenderSystem.Objects
{
	public class Mesh
	{
		private GraphicsDevice mDevice;
		private Model mData;
		private List<ArrayMesh> mSubmeshes = new();
		private List<RenderMaterial> mMaterials = new();

		internal Mesh( GraphicsDevice device, Model modelData )
		{
			mDevice = device;
			mData = modelData;

			RegenerateBuffers();
		}

		public Model Data
		{
			get => mData;
			set
			{
				mData = value;
				RegenerateBuffers();
			}
		}

		public IReadOnlyList<ArrayMesh> Submeshes => mSubmeshes;
		public IReadOnlyList<RenderMaterial> Materials => mMaterials;

		public void RegenerateBuffers()
		{
			Clear();

			foreach ( var modelMesh in Data.Meshes )
			{
				mSubmeshes.Add( new ArrayMesh( mDevice, modelMesh ) );
				mMaterials.Add( Render.LoadMaterial( modelMesh.MaterialName ) );
			}
		}

		public void Clear()
		{
			foreach ( var submesh in mSubmeshes )
			{
				submesh.Dispose();
			}

			mSubmeshes.Clear();
			mMaterials.Clear();
		}
	}
}
