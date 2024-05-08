// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.RenderBackend;
using Elegy.RenderStandard.Extensions;
using Elegy.RenderSystem.Interfaces;
using Elegy.RenderSystem.Interfaces.Rendering;

using Collections.Pooled;
using Veldrid;

using ModelMesh = Elegy.Common.Assets.MeshData.Mesh;

namespace Elegy.RenderStandard;

public class RenderMesh : IMesh
{
	private RenderStandard mHost;
	private GraphicsDevice mDevice;
	private List<ArrayMesh> mSubmeshes = new();
	private List<RenderMaterial> mMaterials = new();

	public RenderMesh( RenderStandard host, GraphicsDevice device, Model modelData )
	{
		mHost = host;
		mDevice = device;
		Data = modelData;

		foreach ( var modelMesh in modelData.Meshes )
		{
			mSubmeshes.Add( new ArrayMesh( mDevice, modelMesh ) );
			mMaterials.Add( mHost.GetMaterial( modelMesh.MaterialName ) );
		}
	}

	public Model Data { get; set; }

	public IReadOnlyList<ArrayMesh> Submeshes => mSubmeshes;
	public IReadOnlyList<RenderMaterial> Materials => mMaterials;

	public void RegenerateBuffers()
	{
		Clear();

		foreach ( var modelMesh in Data.Meshes )
		{
			mSubmeshes.Add( new ArrayMesh( mDevice, modelMesh ) );
			mMaterials.Add( mHost.GetMaterial( modelMesh.MaterialName ) );
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

public partial class RenderStandard : IRenderFrontend
{
	private PooledSet<RenderMesh> mMeshSet = new( 1024 );

	public IMesh CreateMesh( Model modelData )
		=> mMeshSet.AddAndGet( new RenderMesh( this, mDevice, modelData ) );

	public bool FreeMesh( IMesh mesh )
		=> mMeshSet.RemoveAndThen( (RenderMesh)mesh, ( mesh ) =>
		{
			mesh.Clear();
		} );
}
