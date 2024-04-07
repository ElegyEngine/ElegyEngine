
## Put your own paths here
$project_root = "$PSScriptRoot/.."
$output_dir = "$project_root/testgame"

function Build-Project {
	param (
		$ProjectDirectoryPrefix,
		$ProjectName
	)

	Write-Host "====== BUILDING PROJECT '$ProjectName' ======"
	dotnet build "$project_root/src/$ProjectDirectoryPrefix/$ProjectName/$ProjectName.csproj" -c Release --verbosity quiet /property:WarningLevel=0
}

## Step 1: Build the launcher which will also drag all other core dependencies with it
Build-Project Launchers Elegy.Launcher2
Build-Project Plugins Elegy.DevConsole
Build-Project Plugins Elegy.RenderStandard
Build-Project Plugins Elegy.TestGame
Write-Host ""

## Step 2: Copy all the DLLs into the right place
Invoke-Expression "$PSScriptRoot/copy_dlls.ps1 -release"
Write-Host ""

## Step 3: Build the tools
Build-Project Tools Elegy.MapCompiler
Build-Project Tools Elegy.MaterialGenerator
Build-Project Tools Elegy.ShaderTool
Write-Host ""

## Step 4: Copy the tools
Write-Host "====== COPYING TOOLS ======"
Copy-Item "$project_root/src/Tools/Elegy.MapCompiler/bin/Release/net8.0/Elegy.MapCompiler*" -Destination $output_dir -Force
Write-Host "Copied Elegy.MapCompiler"
Copy-Item "$project_root/src/Tools/Elegy.MaterialGenerator/bin/Release/net8.0/Elegy.MaterialGenerator*" -Destination $output_dir -Force
Write-Host "Copied Elegy.MaterialGenerator"
Copy-Item "$project_root/src/Tools/Elegy.ShaderTool/bin/Release/net8.0/Elegy.ShaderTool*" -Destination $output_dir -Force
Write-Host "Copied Elegy.ShaderTool"
Write-Host ""

## Step 5: Finally, compile the shaders
Write-Host "====== BUILDING SHADERS ======"
Invoke-Expression "$PSScriptRoot/build_shaders.ps1"
Write-Host ""

Write-Host "Done!"
Write-Host "You can launch testgame/Elegy.Launcher2 now!"
