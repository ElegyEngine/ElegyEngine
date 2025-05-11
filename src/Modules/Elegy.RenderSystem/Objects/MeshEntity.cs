// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using Elegy.Common.Utilities;
using Elegy.RenderBackend.Extensions;
using Veldrid;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Elegy.RenderSystem.Resources;
using Elegy.RenderSystem.API;

namespace Elegy.RenderSystem.Objects
{
	/// <summary>
	/// Represents a chunk of renderable entity data. One chunk contains up to
	/// 256 renderable entity objects, tightly packed so that all
	/// relevant buffer handles can be cached better, reducing CPU time.
	/// </summary>
	public class MeshEntityChunk
	{
		/// <summary>
		/// Blocks of render entity data that are better grouped together rather than being laid out as an SOA.
		/// </summary>
		[StructLayout( LayoutKind.Sequential )]
		public struct MeshEntityBlock
		{
			public int Mask;
			public Mesh Mesh;
			public ResourceSet EntitySet;
			public List<MaterialParameterPool> InstanceParameterPool;
			public DeviceBuffer? BoneBuffer;
			public SpanIndirect<Matrix4x4> Bones;
		}

		public MeshEntityChunk( GraphicsDevice device )
		{
			TransformBuffer = device.ResourceFactory.CreateBuffer( new()
			{
				Usage = BufferUsage.UniformBuffer,
				SizeInBytes = (uint)(MeshEntitySystem.ChunkSize * Unsafe.SizeOf<Matrix4x4>())
			} );
			TransformStagingBuffer = device.ResourceFactory.CreateBuffer( new()
			{
				Usage = BufferUsage.StagingReadWrite,
				SizeInBytes = TransformBuffer.SizeInBytes
			} );
			TransformStagingBufferView = device.Map<Matrix4x4>( TransformStagingBuffer, MapMode.ReadWrite );
		}

		/// <summary>
		/// All the transforms in this chunk (up to <see cref="MeshEntitySystem.ChunkSize"/> of them).
		/// </summary>
		public Span<Matrix4x4> Transforms => TransformStagingBufferView.AsSpan();

		/// <summary>
		/// Contains <see cref="MeshEntitySystem.ChunkSize"/> transform matrices.
		/// Ranges are derived from <see cref="MeshEntity.ElementIndex"/>.
		/// </summary>
		public DeviceBuffer TransformBuffer { get; }
		
		/// <summary>
		/// Intermediary buffer for uploading to <see cref="TransformBuffer"/>.
		/// </summary>
		public DeviceBuffer TransformStagingBuffer { get; }
		
		/// <summary>
		/// A view into <see cref="TransformStagingBuffer"/>. You can obtain a span from it and modify the buffer that way.
		/// </summary>
		public MappedResourceView<Matrix4x4> TransformStagingBufferView { get; }

		/// <summary>
		/// Blocks of mesh entity data that don't need super cache friendliness.
		/// </summary>
		public MeshEntityBlock[] Blocks { get; } = new MeshEntityBlock[MeshEntitySystem.ChunkSize];

		/// <summary>
		/// Dirty flags for <see cref="TransformBuffer"/>.
		/// </summary>
		public BitVector TransformBufferDirtyBits { get; } = new( MeshEntitySystem.ChunkSize );

		/// <summary>
		/// Dirty flags for <see cref="MeshEntityBlock.BoneBuffer"/>.
		/// </summary>
		public BitVector BoneTransformBufferDirtyBits { get; } = new( MeshEntitySystem.ChunkSize );

		/// <summary>
		/// Free/taken bits.
		/// </summary>
		public BitVector SlotBits { get; } = new( MeshEntitySystem.ChunkSize );
	}

	public class MeshEntitySystem
	{
		public GraphicsDevice Device => Render.Device;
		public const int ChunkSize = 256;
		public List<MeshEntityChunk> Chunks { get; }
		public MeshEntityChunk this[ int i ] => Chunks[i];

		public MeshEntitySystem( int numEntities )
		{
			// Round up to nearest chunk size
			numEntities += ChunkSize - 1;
			Chunks = new( numEntities / ChunkSize );
		}

		public MeshEntity CreateEntity( bool animated, ResourceLayout entityLayout )
		{
			int slot = FindFreeSlot();
			if ( slot == -1 )
			{
				Expand();
				slot = (Chunks.Count - 1) * ChunkSize;
			}

			SetTakenFlag( slot, true );

			// The rest happens in the constructor
			return new( slot, this, Device, animated, entityLayout );
		}

		public bool DestroyEntity( MeshEntity entity )
		{
			if ( !GetTakenFlag( entity.Handle ) )
			{
				return false;
			}

			entity.Dispose();
			SetTakenFlag( entity.Handle, false );

			return true;
		}

		public void UpdateBuffers( CommandList commandList )
		{
			foreach ( var chunk in Chunks )
			{
				if ( chunk.TransformBufferDirtyBits.AnyHigh() )
				{
					// TODO: maybe a more fine update in increments of 16 or 32
					// instead of the whole thing... but this should be fine too
					commandList.CopyBuffer( chunk.TransformStagingBuffer, 0, chunk.TransformBuffer, 0, chunk.TransformBuffer.SizeInBytes );
					chunk.TransformBufferDirtyBits.ClearBits();
				}

				if ( chunk.BoneTransformBufferDirtyBits.AnyHigh() )
				{
					// TODO: Animation
					// ...
					chunk.BoneTransformBufferDirtyBits.ClearBits();
				}
			}
		}

		public void SetTakenFlag( int handle, bool value )
			=> Chunks[handle / ChunkSize].SlotBits.SetBit( handle % ChunkSize, value );

		public bool GetTakenFlag( int handle )
			=> Chunks[handle / ChunkSize].SlotBits.GetBit( handle % ChunkSize );

		public int FindFreeSlot()
		{
			for ( int i = 0; i < Chunks.Count; i++ )
			{
				var chunk = Chunks[i];
				
				if ( !chunk.SlotBits.AnyLow() )
				{
					continue;
				}

				return i * ChunkSize + chunk.SlotBits.FindLowBit();
			}

			return -1;
		}

		public void Expand( int numChunks = 1 )
		{
			for ( int i = 0; i < numChunks; i++ )
			{
				Chunks.Add( new( Device ) );
			}
		}
	}

	[StructLayout( LayoutKind.Sequential )]
	public struct MeshEntity : IDisposable
	{
		public int Handle;
		public MeshEntitySystem System;

		public int ChunkIndex => Handle   / MeshEntitySystem.ChunkSize;
		public int ElementIndex => Handle % MeshEntitySystem.ChunkSize;

		internal MeshEntity( int handle, MeshEntitySystem system, GraphicsDevice device, bool animated, in ResourceLayout perEntityLayout )
		{
			Handle = handle;
			System = system;

			Transform = Matrix4x4.Identity;

			if ( animated )
			{
				throw new NotImplementedException();
				// TODO: This needs a separate resource set t.b.h.
				//BoneTransformBuffer = device.ResourceFactory.CreateBufferForSpan<Matrix4x4>( BufferUsage.StructuredBufferReadOnly, BoneBuffer );
				//PerEntitySet = device.ResourceFactory.CreateSet( perEntityLayout, TransformBuffer, BoneTransformBuffer );
			}

			BoneTransformBuffer = null;
			PerEntitySet = device.ResourceFactory.CreateSet( perEntityLayout, new DeviceBufferRange
			{
				Buffer = TransformBuffer,
				Offset = (uint)(ElementIndex * Unsafe.SizeOf<Matrix4x4>()),
				SizeInBytes = (uint)Unsafe.SizeOf<Matrix4x4>()
			} );

			PerInstanceParameterPools = new();
		}

		public ref MeshEntityChunk.MeshEntityBlock Block
			=> ref System[ChunkIndex].Blocks[ElementIndex];

		public int Mask
		{
			get => System[ChunkIndex].Blocks[ElementIndex].Mask;
			set => System[ChunkIndex].Blocks[ElementIndex].Mask = value;
		}

		public Mesh Mesh
		{
			get => System[ChunkIndex].Blocks[ElementIndex].Mesh;
			set
			{
				System[ChunkIndex].Blocks[ElementIndex].Mesh = value;
				RegenerateParameterPools();
			}
		}

		public Matrix4x4 Transform
		{
			get => System[ChunkIndex].Transforms[ElementIndex];
			set
			{
				TransformBufferDirty = TransformBufferDirty || System[ChunkIndex].Transforms[ElementIndex] != value;
				System[ChunkIndex].Transforms[ElementIndex] = value;
			}
		}

		public SpanIndirect<Matrix4x4> BoneBuffer
		{
			get => System[ChunkIndex].Blocks[ElementIndex].Bones;
			set
			{
				System[ChunkIndex].Blocks[ElementIndex].Bones = value;
				BoneTransformBufferDirty = true;
			}
		}

		public bool TransformBufferDirty
		{
			get => System[ChunkIndex].TransformBufferDirtyBits.GetBit( ElementIndex );
			private set => System[ChunkIndex].TransformBufferDirtyBits.SetBit( ElementIndex, value );
		}

		public DeviceBuffer TransformBuffer => System[ChunkIndex].TransformBuffer;

		public bool BoneTransformBufferDirty
		{
			get => System[ChunkIndex].BoneTransformBufferDirtyBits.GetBit( ElementIndex );
			private set => System[ChunkIndex].BoneTransformBufferDirtyBits.SetBit( ElementIndex, value );
		}

		public DeviceBuffer? BoneTransformBuffer
		{
			get => System[ChunkIndex].Blocks[ElementIndex].BoneBuffer;
			init => System[ChunkIndex].Blocks[ElementIndex].BoneBuffer = value;
		}

		public ResourceSet PerEntitySet
		{
			get => System[ChunkIndex].Blocks[ElementIndex].EntitySet;
			init => System[ChunkIndex].Blocks[ElementIndex].EntitySet = value;
		}

		public List<MaterialParameterPool> PerInstanceParameterPools
		{
			get => System[ChunkIndex].Blocks[ElementIndex].InstanceParameterPool;
			private set => System[ChunkIndex].Blocks[ElementIndex].InstanceParameterPool = value;
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
			BoneTransformBuffer?.Dispose();
			foreach ( var pool in PerInstanceParameterPools )
			{
				pool.Dispose();
			}

			PerInstanceParameterPools.Clear();
		}
	}
}
