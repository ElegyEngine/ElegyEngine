// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using Elegy.Common.Assets.MeshData;
using Elegy.Common.Extensions;
using Elegy.Common.Maths;
using Elegy.RenderBackend.Assets;
using Elegy.RenderBackend.Extensions;
using System.Numerics;
using System.Text;
using Veldrid;

namespace Elegy.RenderBackend
{
	/// <summary>
	/// A convenience layer for creating buffers from a <see cref="Mesh"/>.
	/// </summary>
	public class ArrayMesh : IDisposable
	{
		private Vector2SB ConvertVec2ToVec2SB( Vector2 input )
		{
			input *= 127.0f;
			return new Vector2SB( (sbyte)input.X, (sbyte)input.Y );
		}

		private Vector4SB ConvertVec3ToVec4SB( Vector3 input )
		{
			input *= 127.0f;
			return new Vector4SB( (sbyte)input.X, (sbyte)input.Y, (sbyte)input.Z, sbyte.MaxValue );
		}

		private Vector4SB ConvertVec4ToVec4SB( Vector4 input )
		{
			input *= 127.0f;
			return new Vector4SB( (sbyte)input.X, (sbyte)input.Y, (sbyte)input.Z, (sbyte)input.W );
		}

		private void LoadOrUpdateChannel<T>( ref DeviceBuffer? buffer, T[] source, int numVertices = -1 )
			where T : unmanaged
		{
			if ( source.Length == 0 )
			{
				return;
			}

			if ( buffer is null )
			{
				buffer = mDevice.CreateBufferFromSpan<T>( BufferUsage.VertexBuffer, source, numVertices );
				return;
			}

			mDevice.UpdateBufferFromSpan<T>( buffer, source, numVertices );
		}

		private void LoadOrUpdateChannelTransformed<TSource, TDestination>(
			ref DeviceBuffer? buffer, TSource[] source, ref TDestination[] destination, Func<TSource, TDestination> transform,
			int numVertices = -1 )
			where TSource : unmanaged
			where TDestination : unmanaged
		{
			if ( source.Length == 0 )
			{
				return;
			}

			if ( destination.Length != source.Length )
			{
				Array.Resize( ref destination, source.Length );
			}

			Mesh.Transform( source, destination, transform );
			LoadOrUpdateChannel( ref buffer, destination, numVertices );
		}

		private void LoadOrUpdateIndices( ref DeviceBuffer? buffer, uint[] source, int numIndices = -1 )
		{
			if ( source.Length == 0 )
			{
				return;
			}

			if ( buffer is null )
			{
				buffer = mDevice.CreateBufferFromSpan<uint>( BufferUsage.IndexBuffer, source, numIndices );
				return;
			}

			mDevice.UpdateBufferFromSpan<uint>( buffer, source, numIndices );
		}

		private Vector4SB[] mTransformedNormals = [];
		private Vector4SB[] mTransformedTangents = [];
		private Vector2SB[] mTransformedNormals2D = [];
		private Vector4SB[] mTransformedWeights = [];

		private readonly int mMaxDynamicVertices;
		private readonly int mMaxDynamicIndices;
		private GraphicsDevice mDevice;

		public ArrayMesh( GraphicsDevice device, Mesh meshData )
		{
			mDevice = device;

			DynamicVertices = meshData.NumDynamicVertices;
			NumIndices = meshData.NumDynamicIndices == -1 ? (uint)meshData.Indices.Length : (uint)meshData.NumDynamicIndices;

			mMaxDynamicVertices = meshData.Positions.Length > 0 ? meshData.Positions.Length : meshData.Positions2D.Length;
			mMaxDynamicIndices = meshData.Indices.Length;

			Update( meshData, mMaxDynamicVertices, mMaxDynamicIndices );
		}

		private void Update( Mesh data, int numVertices, int numIndices )
		{
			if ( data.Positions.Length > 0 )
			{
				LoadOrUpdateChannel( ref PositionBuffer, data.Positions, numVertices );
				LoadOrUpdateChannelTransformed( ref NormalBuffer, data.Normals, ref mTransformedNormals, ConvertVec3ToVec4SB, numVertices );
			}
			else
			{
				LoadOrUpdateChannel( ref PositionBuffer, data.Positions2D, numVertices );
				LoadOrUpdateChannelTransformed( ref NormalBuffer, data.Normals2D, ref mTransformedNormals2D, ConvertVec2ToVec2SB, numVertices );
			}

			LoadOrUpdateIndices( ref IndexBuffer, data.Indices, numIndices );

			LoadOrUpdateChannelTransformed( ref TangentBuffer, data.Tangents, ref mTransformedTangents, ConvertVec4ToVec4SB, numVertices );

			LoadOrUpdateChannel( ref Uv0Buffer, data.Uv0, numVertices );
			LoadOrUpdateChannel( ref Uv1Buffer, data.Uv1, numVertices );
			LoadOrUpdateChannel( ref Uv2Buffer, data.Uv2, numVertices );
			LoadOrUpdateChannel( ref Uv3Buffer, data.Uv3, numVertices );

			LoadOrUpdateChannel( ref Color0Buffer, data.Color0, numVertices );
			LoadOrUpdateChannel( ref Color1Buffer, data.Color1, numVertices );
			LoadOrUpdateChannel( ref Color2Buffer, data.Color2, numVertices );
			LoadOrUpdateChannel( ref Color3Buffer, data.Color3, numVertices );

			LoadOrUpdateChannel( ref BoneIndexBuffer, data.BoneIndices, numVertices );
			// TODO: use Vector4B for bone weights
			LoadOrUpdateChannelTransformed( ref BoneWeightBuffer, data.BoneWeights, ref mTransformedWeights, ConvertVec4ToVec4SB, numVertices );
		}

		/// <summary> Updates the GPU buffers with the new mesh data. </summary>
		public void UpdateDynamic( Mesh data, int numVertices, int numIndices )
		{
			Debug.Assert( IsDynamic );

			if ( numVertices >= mMaxDynamicVertices || numIndices >= mMaxDynamicIndices )
			{
				return;
			}

			DynamicVertices = numVertices;
			NumIndices = (uint)numIndices;
			Update( data, numVertices, numIndices );
		}

		/// <summary> Obtains a GPU buffer from the given vertex semantic and optionally channel. </summary>
		public DeviceBuffer? GetBuffer( VertexSemantic semantic, int channel = 0 )
			=> semantic switch
			{
				VertexSemantic.Position => PositionBuffer,
				VertexSemantic.Normal   => NormalBuffer,
				VertexSemantic.Tangent  => TangentBuffer,
				VertexSemantic.Uv => channel switch
				{
					0 => Uv0Buffer,
					1 => Uv1Buffer,
					2 => Uv2Buffer,
					3 => Uv3Buffer,
					_ => throw new NotSupportedException()
				},
				VertexSemantic.Colour => channel switch
				{
					0 => Color0Buffer,
					1 => Color1Buffer,
					2 => Color2Buffer,
					3 => Color3Buffer,
					_ => throw new NotSupportedException()
				},
				VertexSemantic.BoneWeight => BoneWeightBuffer,
				VertexSemantic.BoneIndex  => BoneIndexBuffer,

				_ => throw new NotSupportedException()
			};

		/// <summary> Frees all device buffers. </summary>
		public void Dispose()
		{
			IndexBuffer?.Dispose();
			PositionBuffer?.Dispose();
			NormalBuffer?.Dispose();
			TangentBuffer?.Dispose();
			Uv0Buffer?.Dispose();
			Uv1Buffer?.Dispose();
			Uv2Buffer?.Dispose();
			Uv3Buffer?.Dispose();
			Color0Buffer?.Dispose();
			Color1Buffer?.Dispose();
			Color2Buffer?.Dispose();
			Color3Buffer?.Dispose();
			BoneIndexBuffer?.Dispose();
			BoneWeightBuffer?.Dispose();

			IndexBuffer = null;
			PositionBuffer = null;
			NormalBuffer = null;
			TangentBuffer = null;
			Uv0Buffer = null;
			Uv1Buffer = null;
			Uv2Buffer = null;
			Uv3Buffer = null;
			Color0Buffer = null;
			Color1Buffer = null;
			Color2Buffer = null;
			Color3Buffer = null;
			BoneIndexBuffer = null;
			BoneWeightBuffer = null;
		}

		/// <summary> Whether this mesh is dynamic. </summary>
		public bool IsDynamic => DynamicVertices is not -1;

		/// <summary> Whether this mesh has nothing to render. </summary>
		public bool IsLogicallyEmpty => NumIndices == 0;

		/// <summary> Whether this has 2D position vertices or not. </summary>
		public bool Is2D = false;

		/// <summary> The number of dynamic vertices. </summary>
		public int DynamicVertices;

		/// <summary> Number of indices. Determining it from <see cref="IndexBuffer"/> is not reliable. </summary>
		public uint NumIndices;

		/// <summary> Vertex index buffer. </summary>
		public DeviceBuffer? IndexBuffer;

		/// <summary> Position buffer. </summary>
		public DeviceBuffer? PositionBuffer;

		/// <summary> Normal buffer. </summary>
		public DeviceBuffer? NormalBuffer;

		/// <summary> Tangent buffer. </summary>
		public DeviceBuffer? TangentBuffer;

		/// <summary> UV buffer, channel 0. </summary>
		public DeviceBuffer? Uv0Buffer;

		/// <summary> UV buffer, channel 1. </summary>
		public DeviceBuffer? Uv1Buffer;

		/// <summary> UV buffer, channel 2. </summary>
		public DeviceBuffer? Uv2Buffer;

		/// <summary> UV buffer, channel 3. </summary>
		public DeviceBuffer? Uv3Buffer;

		/// <summary> Colour buffer, channel 0. </summary>
		public DeviceBuffer? Color0Buffer;

		/// <summary> Colour buffer, channel 1. </summary>
		public DeviceBuffer? Color1Buffer;

		/// <summary> Colour buffer, channel 2. </summary>
		public DeviceBuffer? Color2Buffer;

		/// <summary> Colour buffer, channel 3. </summary>
		public DeviceBuffer? Color3Buffer;

		/// <summary> Bone index buffer. </summary>
		public DeviceBuffer? BoneIndexBuffer;

		/// <summary> Bone weight buffer. </summary>
		public DeviceBuffer? BoneWeightBuffer;
	}
}
