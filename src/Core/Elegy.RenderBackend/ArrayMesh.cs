// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

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

		private void LoadChannel<T>( ref DeviceBuffer? buffer, GraphicsDevice device, T[] source )
			where T: unmanaged
		{
			if ( source.Length > 0 )
			{
				buffer = device.CreateBufferFromSpan<T>( BufferUsage.VertexBuffer, source );
			}
		}

		private void LoadChannelTransformed<TSource, TDestination>( ref DeviceBuffer? buffer, GraphicsDevice device, TSource[] source, Func<TSource, TDestination> transform )
			where TSource: unmanaged
			where TDestination: unmanaged
		{
			if ( source.Length > 0 )
			{
				buffer = device.CreateBufferFromSpan<TDestination>( BufferUsage.VertexBuffer, Mesh.Transform( source, transform ) );
			}
		}

		public ArrayMesh( GraphicsDevice device, Mesh meshData )
		{
			PositionBuffer = device.CreateBufferFromSpan<Vector3>( BufferUsage.VertexBuffer, meshData.Positions );
			IndexBuffer = device.CreateBufferFromSpan<uint>( BufferUsage.IndexBuffer, meshData.Indices );

			LoadChannelTransformed( ref NormalBuffer, device, meshData.Normals, ConvertVec3ToVec4SB );
			LoadChannelTransformed( ref TangentBuffer, device, meshData.Tangents, ConvertVec4ToVec4SB );
			
			LoadChannel( ref Uv0Buffer, device, meshData.Uv0 );
			LoadChannel( ref Uv1Buffer, device, meshData.Uv1 );
			LoadChannel( ref Uv2Buffer, device, meshData.Uv2 );
			LoadChannel( ref Uv3Buffer, device, meshData.Uv3 );

			LoadChannel( ref Color0Buffer, device, meshData.Color0 );
			LoadChannel( ref Color1Buffer, device, meshData.Color1 );
			LoadChannel( ref Color2Buffer, device, meshData.Color2 );
			LoadChannel( ref Color3Buffer, device, meshData.Color3 );
			
			LoadChannel( ref BoneIndexBuffer, device, meshData.BoneIndices );
			LoadChannelTransformed( ref BoneWeightBuffer, device, meshData.BoneWeights, ConvertVec4ToVec4SB );
		}

		public bool IsCompatibleWith( in ShaderTemplateEntry shaderTemplateEntry, StringBuilder? errorStringBuilder = null )
		{
			bool compatible = true;
			int numUvBuffersRequired = 0, numColourBuffersRequired = 0;

			ReadOnlySpan<VertexLayoutEntry> span = shaderTemplateEntry.VertexLayouts.AsSpan();
			for ( int i = 0; i < span.Length; i++ )
			{
				switch ( Utils.GetVertexSemantic( span[i].Name ) )
				{
					case VertexSemantic.Position: continue;
					case VertexSemantic.Normal:
						if ( NormalBuffer is null )
						{
							errorStringBuilder.AppendLine( "* Normal" );
							compatible = false;
						}
						break;
					case VertexSemantic.Tangent:
						if ( TangentBuffer is null )
						{
							errorStringBuilder.AppendLine( "* Tangent" );
							compatible = false;
						}
						break;
					case VertexSemantic.Uv:
						numUvBuffersRequired++;
						break;
					case VertexSemantic.Colour:
						numColourBuffersRequired++;
						break;
					case VertexSemantic.BoneWeight:
						if ( BoneWeightBuffer is null )
						{
							errorStringBuilder.AppendLine( "* BoneWeight" );
							compatible = false;
						}
						break;
					case VertexSemantic.BoneIndex:
						if ( BoneIndexBuffer is null )
						{
							errorStringBuilder.AppendLine( "* BoneIndex" );
							compatible = false;
						}
						break;
				}
			}

			int numUvBuffers = 0;
			if ( Uv0Buffer is not null ) { numUvBuffers++; }
			if ( Uv1Buffer is not null ) { numUvBuffers++; }
			if ( Uv2Buffer is not null ) { numUvBuffers++; }
			if ( Uv3Buffer is not null ) { numUvBuffers++; }
			if ( numUvBuffers != numUvBuffersRequired )
			{
				errorStringBuilder.AppendLine( $"* Need {numUvBuffersRequired} UV buffers, have {numUvBuffers}" );
			}

			int numColourBuffers = 0;
			if ( Color0Buffer is not null ) { numColourBuffers++; }
			if ( Color1Buffer is not null ) { numColourBuffers++; }
			if ( Color2Buffer is not null ) { numColourBuffers++; }
			if ( Color3Buffer is not null ) { numColourBuffers++; }
			if ( numColourBuffers != numColourBuffersRequired )
			{
				errorStringBuilder.AppendLine( $"* Need {numColourBuffersRequired} colour buffers, have {numColourBuffers}" );
			}

			return compatible;
		}

		public bool IsCompatibleWith( in ShaderTemplate shaderTemplate, StringBuilder? errorStringBuilder = null )
		{
			bool compatible = true;
			foreach ( var item in shaderTemplate.ShaderVariants )
			{
				if ( !IsCompatibleWith( item, errorStringBuilder ) )
				{
					errorStringBuilder?.AppendLine( $" ^ in shader variant '{item.ShaderDefine}'" );
					compatible = false;
				}
			}

			return compatible;
		}

		/// <summary> Frees all device buffers. </summary>
		public void Dispose()
		{
			IndexBuffer?.Dispose();
			PositionBuffer.Dispose();
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

		/// <summary> Vertex index buffer. </summary>
		public DeviceBuffer IndexBuffer;
		/// <summary> Position buffer. </summary>
		public DeviceBuffer PositionBuffer;
		/// <summary> Normal buffer. </summary>
		public DeviceBuffer? NormalBuffer = null;
		/// <summary> Tangent buffer. </summary>
		public DeviceBuffer? TangentBuffer = null;
		/// <summary> UV buffer, channel 0. </summary>
		public DeviceBuffer? Uv0Buffer = null;
		/// <summary> UV buffer, channel 1. </summary>
		public DeviceBuffer? Uv1Buffer = null;
		/// <summary> UV buffer, channel 2. </summary>
		public DeviceBuffer? Uv2Buffer = null;
		/// <summary> UV buffer, channel 3. </summary>
		public DeviceBuffer? Uv3Buffer = null;
		/// <summary> Colour buffer, channel 0. </summary>
		public DeviceBuffer? Color0Buffer = null;
		/// <summary> Colour buffer, channel 1. </summary>
		public DeviceBuffer? Color1Buffer = null;
		/// <summary> Colour buffer, channel 2. </summary>
		public DeviceBuffer? Color2Buffer = null;
		/// <summary> Colour buffer, channel 3. </summary>
		public DeviceBuffer? Color3Buffer = null;
		/// <summary> Bone index buffer. </summary>
		public DeviceBuffer? BoneIndexBuffer = null;
		/// <summary> Bone weight buffer. </summary>
		public DeviceBuffer? BoneWeightBuffer = null;
	}
}
