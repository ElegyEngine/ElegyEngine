
## Put your own paths here
$project_root = "$PSScriptRoot/.."
$output_dir = "$project_root/testgame"

Invoke-Expression "$output_dir/Elegy.ShaderTool $output_dir/engine/shaders -force"
