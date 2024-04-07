
# Elegy.MapCompiler

Level compiler for [Elegy Engine](https://github.com/ElegyEngine).

It parses Quake (Valve220 format) `.map` files, and bakes an Elegy level file (`.elf`) out of that. If you don't know what those are, definitely look up info about Quake mapping and TrenchBroom.

What it currently does and doesn't:
* Geometry:
	* [x] Process TrenchBroom and J.A.C.K. `.map` files
	* [x] Handling origin brushes
	* [x] Basic culling (removal of nodraw faces)
	* [x] Writing in Elegy's `.elf` level format
* Geometry processing:
	* [ ] Advanced culling (CSG-based)
	* [ ] Smooth normals
	* [ ] Dual grid partitioning
	* [ ] Importing meshes
* Visibility:
	* [ ] Visibility computation (grid PVS)
* Lighting:
	* [ ] Generating lightmap UVs
	* [ ] Baking lightmap with compute shaders
	* [ ] Baking lightmap with RT cores?

*Note: super early WiP, check back in Q3 2024!*
