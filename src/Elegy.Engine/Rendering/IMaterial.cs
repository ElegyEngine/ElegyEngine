// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Numerics;

namespace Elegy.Rendering
{
	public interface IMaterial
	{
		string[] GetParameterNames();
		int GetParameterIndex( string name );

		void SetParameter( int id, int value );
		void SetParameter( int id, float value );
		void SetParameter( int id, bool value );
		void SetParameter( int id, Vector2 value );
		void SetParameter( int id, Vector3 value );
		void SetParameter( int id, Vector4 value );
		void SetParameter( int id, Matrix4x4 value );
		void SetBufferParameter<T>( int id, T bufferValue ) where T: unmanaged;
	}
}
