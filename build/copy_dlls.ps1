
param(
	[switch]$release = $false,
	$plugin = "none"
)

Write-Host "========= COPYING MODULES ========="

## Put your own paths here
$input_dir = "$PSScriptRoot/.."
$output_dir = "$input_dir/testgame"

$build_config = "Debug"
if ( $release )
{
	$build_config = "Release"
}

Write-Output "Configuration: $build_config"
if ( !$release )
{
	Write-Output "If you'd like to use the Release configuration, use the -release flag"
}

if ( $plugin = "none" )
{
	## Copy the launcher executable, engine DLL and its dependencies
	Copy-Item "$input_dir/src/Launchers/Elegy.Launcher2/bin/$build_config/net8.0/*" 						-Destination "$output_dir" -Recurse -Force
	
	## Copy the plugin DLLs without their dependencies
	Copy-Item "$input_dir/src/Plugins/Elegy.DevConsole/bin/$build_config/net8.0/Elegy.DevConsole.*"			-Destination "$output_dir/engine/plugins/DevConsole"
	Copy-Item "$input_dir/src/Plugins/Elegy.Game/bin/$build_config/net8.0/Game.*"							-Destination "$output_dir/game/plugins/Game"
	Copy-Item "$input_dir/src/Plugins/Elegy.Game/bin/$build_config/net8.0/Bepu*"							-Destination "$output_dir/game/plugins/Game"
	Copy-Item "$input_dir/src/Plugins/Elegy.Game/bin/$build_config/net8.0/fennecs.*" 						-Destination "$output_dir/game/plugins/Game"
	Copy-Item "$input_dir/src/Plugins/Elegy.Game/bin/$build_config/net8.0/Elegy.ECS.*" 						-Destination "$output_dir/game/plugins/Game"
	Copy-Item "$input_dir/src/Plugins/Elegy.Game/bin/$build_config/net8.0/Elegy.RenderWorld.*" 				-Destination "$output_dir/game/plugins/Game"
}
else
{
	## Copy a plugin DLL
	Copy-Item "$input_dir/src/$plugin/bin/$build_config/net8.0/$plugin.*" -Destination "$output_dir/game/plugins/$plugin"
}

Write-Output "Successfully copied DLLs into '$output_dir'"
