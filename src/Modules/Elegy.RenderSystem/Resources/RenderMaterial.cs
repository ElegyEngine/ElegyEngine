// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.Interfaces.Rendering;
using Elegy.Common.Assets;
using Elegy.Common.Utilities;
using Elegy.RenderBackend.Templating;
using System.Numerics;
using Veldrid;

namespace Elegy.RenderSystem.Resources
{
	public class RenderMaterial : IMaterial, IDisposable
	{
		private static TaggedLogger mLogger = new( "RenderMaterial" );

		public RenderMaterial( GraphicsDevice device, MaterialDefinition definition, MaterialTemplate materialTemplate )
		{
			ParameterPool = new( device, materialTemplate, definition );
			GlobalParameterPool = new( device, materialTemplate );
			Definition = definition;
			Template = materialTemplate;
		}

		public List<ResourceSetVariant> ResourceVariants => ParameterPool.ResourceSetVariants;
		public List<ResourceSetVariant> GlobalResourceVariants => GlobalParameterPool.ResourceSetVariants;
		public MaterialDefinition Definition { get; init; }
		public MaterialTemplate Template { get; init; }

		public MaterialParameterPool ParameterPool { get; private set; }

		public MaterialParameterPool GlobalParameterPool { get; private set; }

		public string[] GetParameterNames()
			=> ParameterPool.GetParameterNames();

		public int GetParameterIndex( string name )
			 => ParameterPool.GetParameterIndex( name );

		public void SetParameter( int id, int value )
			=> ParameterPool.SetParameter( id, value );

		public void SetParameter( int id, float value )
			=> ParameterPool.SetParameter( id, value );

		public void SetParameter( int id, bool value )
			=> ParameterPool.SetParameter( id, value );

		public void SetParameter( int id, Vector2 value )
			=> ParameterPool.SetParameter( id, value );

		public void SetParameter( int id, Vector3 value )
			=> ParameterPool.SetParameter( id, value );

		public void SetParameter( int id, Vector4 value )
			=> ParameterPool.SetParameter( id, value );

		public void SetParameter( int id, Matrix4x4 value )
			=> ParameterPool.SetParameter( id, value );

		public void SetBufferParameter<T>( int id, T bufferValue ) where T : unmanaged
			=> ParameterPool.SetBufferParameter( id, bufferValue );

		public void SetTexture( int id, ITexture value )
			=> ParameterPool.SetTexture( id, value );

		public void Dispose()
		{
			ParameterPool.Dispose();
		}
	}
}
