
## Put your own paths here
$project_root = "$PSScriptRoot/.."
$output_dir = "$project_root/testgame"

function Build-Project {
	param (
		$ProjectName
	)

	Write-Host "====== BUILDING PROJECT $ProjectName ======"
	dotnet build "$project_root/src/$ProjectName/$ProjectName.csproj" -c Release --verbosity quiet /property:WarningLevel=0
}

## Step 1: Build the launcher which will also drag all other core dependencies with it
Build-Project Elegy.Launcher2
Build-Project Elegy.DevConsole
Build-Project Elegy.RenderStandard
Build-Project Elegy.TestGame
Write-Host ""

## Step 2: Copy all the DLLs into the right place
Invoke-Expression "$PSScriptRoot/copy_dlls.ps1 -release"
Write-Host ""

## Step 3: Build the tools
Build-Project Elegy.MapCompiler
Build-Project Elegy.MaterialGenerator
Build-Project Elegy.ShaderTool
Write-Host ""

## Step 4: Copy the tools
Write-Host "====== COPYING TOOLS ======"
Copy-Item "$project_root/src/Elegy.MapCompiler/bin/Release/net8.0/Elegy.MapCompiler*" -Destination $output_dir -Force
Write-Host "Copied Elegy.MapCompiler"
Copy-Item "$project_root/src/Elegy.MaterialGenerator/bin/Release/net8.0/Elegy.MaterialGenerator*" -Destination $output_dir -Force
Write-Host "Copied Elegy.MaterialGenerator"
Copy-Item "$project_root/src/Elegy.ShaderTool/bin/Release/net8.0/Elegy.ShaderTool*" -Destination $output_dir -Force
Write-Host "Copied Elegy.ShaderTool"
Write-Host ""

## Step 5: Finally, compile the shaders
Invoke-Expression "$output_dir/Elegy.ShaderTool $output_dir/engine/shaders -force"
Write-Host ""

Write-Host "Done!"
Write-Host "You can launch testgame/Elegy.Launcher2 now!"
