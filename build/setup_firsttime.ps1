
param(
	[switch]$debug = $false
)

## Put your own paths here
$project_root = "$PSScriptRoot/.."
$output_dir = "$project_root/testgame"

function Build-Project {
	param (
		$ProjectDirectoryPrefix,
		$ProjectName
	)

	Write-Host "====== BUILDING PROJECT '$ProjectName' ======"
	if ( $debug )
	{
		dotnet build "$project_root/src/$ProjectDirectoryPrefix/$ProjectName/$ProjectName.csproj" -c Debug --verbosity quiet /property:WarningLevel=0
	}
	else
	{
		dotnet build "$project_root/src/$ProjectDirectoryPrefix/$ProjectName/$ProjectName.csproj" -c Release --verbosity quiet /property:WarningLevel=0
	}
}

## Step 1: Build the launcher which will also drag all other core dependencies with it
Build-Project Launchers Elegy.Launcher2
Build-Project Plugins Elegy.DevConsole
Build-Project Plugins Elegy.Game
Write-Host ""

## Step 2: Copy all the DLLs into the right place
if ( $debug )
{
	Invoke-Expression "$PSScriptRoot/copy_dlls.ps1"
}
else
{
	Invoke-Expression "$PSScriptRoot/copy_dlls.ps1 -release"
}
Write-Host ""

## Step 3: Build the tools
Build-Project Tools Elegy.MapCompiler
Build-Project Tools Elegy.MaterialGenerator
Build-Project Tools Elegy.ShaderTool
Write-Host ""

## Step 4: Copy the tools
if ( $debug )
{
	Invoke-Expression "$PSScriptRoot/copy_tools.ps1"
}
else
{
	Invoke-Expression "$PSScriptRoot/copy_tools.ps1 -release"
}

## Step 5: Finally, compile the shaders
Write-Host "====== BUILDING SHADERS ======"
Invoke-Expression "$PSScriptRoot/build_shaders.ps1"
Write-Host ""

Write-Host "Done!"
Write-Host "In order to launch the test game project:"
Write-Host " * 1: Change directory to testgame/"
Write-Host " * 2: Run bin/Elegy.Launcher2"
