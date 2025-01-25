
materials/tools/nodraw
{
	materialTemplate NoDraw
	{
		// TrenchBroom dies upon encountering quotation marks here, so do not use them
		map textures/tools/nodraw
	}

	compilerParams
	{
		// Stripped away
		NoDraw
		// Do not affect lighting
		NoShadowCast
		NoLightmapReceived
		// If we ever end up using brushes for collision acceleration, this will have
		// to get commented out or something
		NoCollision
	}
}

materials/tools/blocklight
{
	materialTemplate NoDraw
	{
		map textures/tools/blocklight
	}

	compilerParams
	{
		NoDraw
		NoLightmapReceived
		NoCollision
	}
}

materials/tools/trigger
{
	materialTemplate NoDraw
	{
		map textures/tools/trigger
	}

	compilerParams
	{
		NoDraw
		NoShadowCast
		ForceNoLightmap
		// How else would a trigger do its thing? AABB tests?!
		NoCollision
	}
}
