// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Numerics;

namespace Elegy.Engine.Interfaces.Rendering
{
	/// <summary>
	/// 
	/// </summary>
	public interface IMaterial
	{
		/// <summary></summary>
		string[] GetParameterNames();
		/// <summary></summary>
		int GetParameterIndex( string name );

		/// <summary></summary>
		void SetParameter( int id, int value );
		/// <summary></summary>
		void SetParameter( int id, float value );
		/// <summary></summary>
		void SetParameter( int id, bool value );
		/// <summary></summary>
		void SetParameter( int id, Vector2 value );
		/// <summary></summary>
		void SetParameter( int id, Vector3 value );
		/// <summary></summary>
		void SetParameter( int id, Vector4 value );
		/// <summary></summary>
		void SetParameter( int id, Matrix4x4 value );
		/// <summary></summary>
		void SetBufferParameter<T>( int id, T bufferValue ) where T : unmanaged;
		/// <summary></summary>
		void SetTexture( int id, ITexture value );
	}
}
