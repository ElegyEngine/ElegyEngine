
$shaderFileNames = Get-ChildItem -Recurse

foreach( $file in $shaderFileNames )
{
	if ( $file.Name.EndsWith(".glsl") )
	{
		$fileNoExt = $file.Name.Replace(".glsl", "")

		Write-Host "Compiling $file"
		glslangValidator -V -S vert -DVERTEX_SHADER=1 -e "main_vs" --sep "main" -o "$fileNoExt.vs.spv" $file
		glslangValidator -V -S frag -DPIXEL_SHADER=1  -e "main_ps" --sep "main" -o "$fileNoExt.ps.spv" $file
		#glslangValidator -V -S comp -DCOMPUTE_SHADER=1  -e "main_cs" --sep "main" -o "$fileNoExt.cs.spv" $file
	}
}

