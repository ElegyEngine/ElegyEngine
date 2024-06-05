// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Utilities;
using Elegy.RenderBackend.Extensions;
using Veldrid;

using System.Numerics;
using Elegy.RenderSystem.Resources;
using Elegy.RenderSystem.API;

namespace Elegy.RenderSystem.Objects
{
	public class MeshEntity : IDisposable
	{
		private Mesh mMesh;
		private Matrix4x4 mTransform;
		private SpanIndirect<Matrix4x4> mBoneBuffer = Array.Empty<Matrix4x4>();

		internal MeshEntity( GraphicsDevice device, bool animated, in ResourceLayout perEntityLayout )
		{
			Transform = Matrix4x4.Identity;
			TransformBuffer = device.ResourceFactory.CreateBufferForStruct<Matrix4x4>( BufferUsage.UniformBuffer );

			if ( animated )
			{
				throw new NotImplementedException();
				// TODO: This needs a separate resource set t.b.h.
				//BoneTransformBuffer = device.ResourceFactory.CreateBufferForSpan<Matrix4x4>( BufferUsage.StructuredBufferReadOnly, BoneBuffer );
				//PerEntitySet = device.ResourceFactory.CreateSet( perEntityLayout, TransformBuffer, BoneTransformBuffer );
			}
			else
			{
				BoneTransformBuffer = null;
				PerEntitySet = device.ResourceFactory.CreateSet( perEntityLayout, TransformBuffer );
			}
		}

		public int Mask { get; set; }
		public Mesh Mesh
		{
			get => mMesh;
			set
			{
				mMesh = value;
				RegenerateParameterPools();
			}
		}

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

		private void RegenerateParameterPools()
		{
			foreach ( var pool in PerInstanceParameterPools )
			{
				pool.Dispose();
			}

			PerInstanceParameterPools.Clear();
			PerInstanceParameterPools.EnsureCapacity( Mesh.Materials.Count );

			foreach ( var material in Mesh.Materials )
			{
				MaterialParameterPool pool = new( Render.Device, material.Template, material.Definition, perInstance: true );
				PerInstanceParameterPools.Add( pool );
			}
		}

		public void Dispose()
		{
			PerEntitySet.Dispose();
			TransformBuffer.Dispose();
			BoneTransformBuffer?.Dispose();
			foreach ( var pool in PerInstanceParameterPools )
			{
				pool.Dispose();
			}
			PerInstanceParameterPools.Clear();
		}

		public bool TransformBufferDirty { get; private set; } = false;
		public DeviceBuffer TransformBuffer { get; set; }
		public bool BoneTransformBufferDirty { get; private set; } = false;
		public DeviceBuffer? BoneTransformBuffer { get; set; }
		public ResourceSet PerEntitySet { get; private set; }
		public List<MaterialParameterPool> PerInstanceParameterPools { get; private set; } = new();
	}
}
