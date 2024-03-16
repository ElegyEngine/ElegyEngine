
## Put your own paths here
$project_root = "$PSScriptRoot/.."
$output_dir = "$project_root/testgame"

function Build-Project {
	param (
		$ProjectName
	)

	dotnet build "$project_root/src/$ProjectName/$ProjectName.csproj" -c Release /property:WarningLevel=0
}

## Step 1: Build the launcher which will also drag all other core dependencies with it
Build-Project Elegy.Launcher2
Build-Project Elegy.DevConsole
Build-Project Elegy.RenderStandard
Build-Project Elegy.TestGame

## Step 2: Copy all the DLLs into the right place
Invoke-Expression "$PSScriptRoot/copy_dlls.ps1 -release"

## Step 3: Build the tools
Build-Project Elegy.MapCompiler
Build-Project Elegy.MaterialGenerator
Build-Project Elegy.ShaderTool

## Step 4: Copy the tools
Copy-Item "$project_root/src/Elegy.MapCompiler/bin/Release/net8.0/Elegy.MapCompiler*" -Destination $output_dir -Force
Copy-Item "$project_root/src/Elegy.MaterialGenerator/bin/Release/net8.0/Elegy.MaterialGenerator*" -Destination $output_dir -Force
Copy-Item "$project_root/src/Elegy.ShaderTool/bin/Release/net8.0/Elegy.ShaderTool*" -Destination $output_dir -Force

## Step 5: Finally, compile the shaders
Invoke-Expression "$output_dir/Elegy.ShaderTool $output_dir/engine/shaders -force"
