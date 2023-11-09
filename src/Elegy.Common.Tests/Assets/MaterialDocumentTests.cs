
using Elegy.Assets;

namespace Elegy.Tests;

public class MaterialDocumentTests
{
	private void SimpleMaterialCheck( MaterialDefinition def, string materialName, string templateName, string diffuseMap )
	{
		Assert.Equal( materialName, def.Name );
		Assert.Equal( templateName, def.TemplateName );
		Assert.Equal( diffuseMap, def.DiffuseMap );
	}

	[Fact]
	public void BasicMaterialTest()
	{
		string materialString =
			"""
			textures/mat_test
			{
				materialTemplate Standard
				{
					map textures/dev/256floor.png
				}
			}
			""";

		MaterialDocument doc = new( materialString );

		var def = Assert.Single( doc.Materials );

		Assert.Single( def.Parameters );
		SimpleMaterialCheck( def, "textures/mat_test", "Standard", "textures/dev/256floor.png" );
	}

	[Fact]
	public void ToolMaterialTest()
	{
		string materialString =
			"""
			textures/tools/nodraw
			{
				materialTemplate NoDraw
				{
					map textures/tools/nodraw.png
				}

				compilerParams
				{
					// Stripped away
					Nodraw 1
					// Nodraw implies that lightmap will be 0, but yeah
					Lightmap 0
					// This one will probably need tweaking. If we ever end up
					// using brushes for collision acceleration, this will have
					// to be 1.
					Collide 0
				}
			}
			""";

		MaterialDocument doc = new( materialString );

		var def = Assert.Single( doc.Materials );

		Assert.Single( def.Parameters );
		SimpleMaterialCheck( def, "textures/tools/nodraw", "NoDraw", "textures/tools/nodraw.png" );
	}

	[Fact]
	public void MultipleToolMaterialTest()
	{
		string materialString =
			"""
			textures/tools/nodraw
			{
				materialTemplate NoDraw
				{
					map textures/tools/nodraw.png
				}

				compilerParams
				{
					// Stripped away
					Nodraw 1
					// Nodraw implies that lightmap will be 0, but yeah
					Lightmap 0
					// This one will probably need tweaking. If we ever end up
					// using brushes for collision acceleration, this will have
					// to be 1.
					Collide 0
				}
			}

			textures/tools/blocklight
			{
				materialTemplate NoDraw
				{
					map textures/tools/blocklight.png
				}

				compilerParams
				{
					Nodraw 1
					Lightmap 0
					Blocklight 1
				}
			}

			textures/tools/trigger
			{
				materialTemplate NoDraw
				{
					map textures/tools/trigger.png
				}

				compilerParams
				{
					Nodraw 1
					// How else would a trigger do its thing? AABB tests?!
					Collide 1
				}
			}
			
			""";

		MaterialDocument doc = new( materialString );

		Assert.Equal( 3, doc.Materials.Count );

		SimpleMaterialCheck( doc.Materials[0], "textures/tools/nodraw", "NoDraw", "textures/tools/nodraw.png" );
		SimpleMaterialCheck( doc.Materials[1], "textures/tools/blocklight", "NoDraw", "textures/tools/blocklight.png" );
		SimpleMaterialCheck( doc.Materials[2], "textures/tools/trigger", "NoDraw", "textures/tools/trigger.png" );
	}
}