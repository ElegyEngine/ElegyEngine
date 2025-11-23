
param(
    [switch]$release = $false
)

## Put your own paths here
$project_root = "$PSScriptRoot/.."
$output_dir = "$project_root/testgame"

Write-Host "====== BUILDING PROJECT 'Elegy.Game' ======"

if ( $release )
{
    dotnet build "$project_root/src/Plugins/Elegy.Game/Elegy.Game.csproj" -c Release --verbosity quiet /property:WarningLevel=0
}
else
{
    dotnet build "$project_root/src/Plugins/Elegy.Game/Elegy.Game.csproj" -c Debug --verbosity quiet /property:WarningLevel=0
}

Write-Host "Done building, gonna copy the DLLs now..."

Invoke-Expression "$PSScriptRoot/copy_dlls.ps1"
