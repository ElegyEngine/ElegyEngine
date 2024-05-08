// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Utilities;
using Elegy.AssetSystem.Interfaces;
using Elegy.AssetSystem.Interfaces.Rendering;
using Elegy.RenderSystem.Interfaces;
using Elegy.RenderSystem.Interfaces.Rendering;
using Elegy.RenderBackend.Extensions;
using Elegy.RenderStandard.Extensions;

using Collections.Pooled;
using System.Numerics;
using Veldrid;

namespace Elegy.RenderStandard;

public class RenderEntity : IEntity
{
	private Matrix4x4 mTransform;
	private SpanIndirect<Matrix4x4> mBoneBuffer = Array.Empty<Matrix4x4>();

	public RenderEntity( GraphicsDevice device, bool animated, in ResourceLayout perEntityLayout )
	{
		TransformBuffer = device.ResourceFactory.CreateBufferForStruct<Matrix4x4>( BufferUsage.UniformBuffer );
		BoneTransformBuffer = animated 
			? device.ResourceFactory.CreateBufferForSpan<Matrix4x4>( BufferUsage.StructuredBufferReadOnly, BoneBuffer )
			: null;

		PerEntitySet = device.ResourceFactory.CreateSet( perEntityLayout, TransformBuffer );
	}

	public int Mask { get; set; }
	public IMesh Mesh { get; set; }

	public Matrix4x4 Transform
	{
		get => mTransform;
		set
		{
			mTransform = value;
			TransformBufferDirty = true;
		}
	}
	
	public SpanIndirect<Matrix4x4> BoneBuffer
	{
		get => mBoneBuffer;
		set
		{
			mBoneBuffer = value;
			BoneTransformBufferDirty = true;
		}
	}

	public void UpdateBuffers( GraphicsDevice gd )
	{
		if ( TransformBufferDirty )
		{
			gd.UpdateBuffer( TransformBuffer, 0, mTransform );
			TransformBufferDirty = false;
		}

		if ( BoneTransformBufferDirty )
		{
			gd.UpdateBuffer( BoneTransformBuffer, 0, (Span<Matrix4x4>)mBoneBuffer );
			BoneTransformBufferDirty = false;
		}
	}

	public bool TransformBufferDirty { get; private set; } = false;
	public DeviceBuffer TransformBuffer { get; set; }
	public bool BoneTransformBufferDirty { get; private set; } = false;
	public DeviceBuffer? BoneTransformBuffer { get; set; }
	public ResourceSet PerEntitySet { get; private set; }
}

public partial class RenderStandard : IRenderFrontend
{
	private PooledSet<RenderEntity> mEntitySet = new( 1024 );

	public IEntity CreateEntity( bool animated )
		=> mEntitySet.AddAndGet( new( mDevice, animated, mPerEntityLayout ) );

	public bool FreeEntity( IEntity entity )
		=> mEntitySet.RemoveAndThen( (RenderEntity)entity, ( entity ) =>
		{
			entity.PerEntitySet.Dispose();
			entity.TransformBuffer.Dispose();
			entity.BoneTransformBuffer?.Dispose();
		} );
}
