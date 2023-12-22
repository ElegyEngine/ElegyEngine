
param(
	[switch]$release = $false
)

Write-Host "========= COPYING MODULES ========="

## Put your own paths here
$input_dir = "$PSScriptRoot/.."
$output_dir = "$input_dir/testgame"

$build_config = $release ? "Release" : "Debug"
Write-Output "Configuration: $build_config"
if ( !$release )
{
	Write-Output "If you'd like to use the Release configuration, use the -release flag"
}

## Copy the debug DLLs there
Copy-Item "$input_dir/src/Elegy.DevConsole/bin/$build_config/net6.0/Elegy.DevConsole.*"	-Destination "$output_dir/engine/plugins/DevConsole"
Copy-Item "$input_dir/src/Elegy.TestGame/bin/$build_config/net6.0/Game.*"				-Destination "$output_dir/game/plugins/Game"
Copy-Item "$input_dir/src/Elegy.Common/bin/$build_config/net6.0/Elegy.Common.*"			-Destination "$output_dir"
Copy-Item "$input_dir/src/Elegy.Engine/bin/$build_config/net6.0/Elegy.Engine.*"			-Destination "$output_dir"

Write-Output "Successfully copied Elegy.DevConsole.dll, Elegy.Common.dll and Elegy.Engine.dll into '$output_dir'"
