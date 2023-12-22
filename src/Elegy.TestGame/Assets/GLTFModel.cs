// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace TestGame.Assets
{
	public class GLTFModel
	{
		public static ArrayMesh? Load( string path )
		{
			if ( !FileSystem.Exists( path, PathFlags.File ) )
			{
				Console.Error( $"Cannot find image '{path}', oops" );
				// Todo: default "error" model
				return null;
			}

			GltfState state = new();
			GltfDocument document = new();
			if ( document.AppendFromFile( FileSystem.PathTo( path, PathFlags.File ), state ) != Error.Ok )
			{
				Console.Error( "Boohoo" );
				return null;
			}

			return state.Meshes[0].Mesh.GetMesh();
		}
	}
}
