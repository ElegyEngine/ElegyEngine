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

public class RenderSubmesh
{
	public RenderSubmesh( RenderMaterial material, DeviceBuffer buffer )
	{

	}

	public RenderMaterial Material;
	public DeviceBuffer MeshBuffer;
}

public class RenderMesh : IMesh
{
	private GraphicsDevice mDevice;
	private List<RenderSubmesh> mSubmeshes = new();

	public RenderMesh( GraphicsDevice device, Model modelData )
	{
		mDevice = device;
		Data = modelData;

		foreach ( var modelMesh in modelData.Meshes )
		{
			
		}
	}

	public Model Data { get; set; }

	public void RegenerateBuffers()
	{
		Clear();


	}

	public void Clear()
	{
		foreach ( var submesh in mSubmeshes )
		{
			submesh.MeshBuffer.Dispose();
		}

		mSubmeshes.Clear();
	}
}

public partial class RenderStandard : IRenderFrontend
{
	private PooledSet<RenderMesh> mMeshSet = new( 1024 );

	public IMesh CreateMesh( Model modelData )
		=> mMeshSet.AddAndGet( new RenderMesh( mDevice, modelData ) );

	public bool FreeMesh( IMesh mesh )
		=> mMeshSet.RemoveAndThen( (RenderMesh)mesh, ( mesh ) =>
		{
			mesh.Clear();
		} );
}
