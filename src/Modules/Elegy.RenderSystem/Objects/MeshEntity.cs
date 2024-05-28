// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Utilities;
using Elegy.RenderBackend.Extensions;
using Veldrid;

using System.Numerics;
using Elegy.RenderSystem.Resources;

namespace Elegy.RenderSystem.Objects
{
	public class MeshEntity : IDisposable
	{
		private Matrix4x4 mTransform;
		private SpanIndirect<Matrix4x4> mBoneBuffer = Array.Empty<Matrix4x4>();

		internal MeshEntity( GraphicsDevice device, bool animated, in ResourceLayout perEntityLayout )
		{
			TransformBuffer = device.ResourceFactory.CreateBufferForStruct<Matrix4x4>( BufferUsage.UniformBuffer );
			BoneTransformBuffer = animated
				? device.ResourceFactory.CreateBufferForSpan<Matrix4x4>( BufferUsage.StructuredBufferReadOnly, BoneBuffer )
				: null;

			PerEntitySet = device.ResourceFactory.CreateSet( perEntityLayout, TransformBuffer );
		}

		public int Mask { get; set; }
		public Mesh Mesh { get; set; }

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

		internal void UpdateBuffers( GraphicsDevice gd )
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

		public void Dispose()
		{
			PerEntitySet.Dispose();
			TransformBuffer.Dispose();
			BoneTransformBuffer?.Dispose();
		}

		public bool TransformBufferDirty { get; private set; } = false;
		public DeviceBuffer TransformBuffer { get; set; }
		public bool BoneTransformBufferDirty { get; private set; } = false;
		public DeviceBuffer? BoneTransformBuffer { get; set; }
		public ResourceSet PerEntitySet { get; private set; }
		public List<MaterialParameterPool> PerInstanceParameterPools { get; private set; } = new();
	}
}
