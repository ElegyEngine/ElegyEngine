
param(
	[switch]$release = $false
)

Write-Host "========= COPYING TOOLS ========="

## Put your own paths here
$input_dir = "$PSScriptRoot/.."
$output_dir = "$input_dir/testgame/bin"

$build_config = "Debug"
if ( $release )
{
	$build_config = "Release"
}

Write-Host "====== COPYING TOOLS ======"
Copy-Item "$input_dir/src/Tools/Elegy.MapCompiler/bin/$build_config/net8.0/Elegy.MapCompiler*" -Destination $output_dir -Force
Write-Host "Copied Elegy.MapCompiler"
Copy-Item "$input_dir/src/Tools/Elegy.MaterialGenerator/bin/$build_config/net8.0/Elegy.MaterialGenerator*" -Destination $output_dir -Force
Write-Host "Copied Elegy.MaterialGenerator"
Copy-Item "$input_dir/src/Tools/Elegy.ShaderTool/bin/$build_config/net8.0/Elegy.ShaderTool*" -Destination $output_dir -Force
Write-Host "Copied Elegy.ShaderTool"
Write-Host ""

Write-Output "Successfully copied tools into '$output_dir'"
