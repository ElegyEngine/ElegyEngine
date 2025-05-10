
function Build-Shader {
	param (
		$TemplateName,
		$VariantName
	)
	
	$Name = "$TemplateName.$VariantName"

	Write-Host "Compiling '$TemplateName.$VariantName'..."
	slangc.exe "$TemplateName/$Name.slang" -I ./ -target spirv -enable-experimental-passes -o "bin/$Name.spv" -reflection-json "bin/$Name.spv.json"
}

Build-Shader "Standard" "GENERAL"
Build-Shader "Standard" "DEPTH"
Build-Shader "Standard" "LIGHTMAP"
Build-Shader "Standard" "WIREFRAME"
