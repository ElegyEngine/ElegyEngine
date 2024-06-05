
Write-Host "========= BUILDING PACKAGES ========="

$project_root = "$PSScriptRoot/../.."

function Publish-Project {
	param (
		$ProjectDirectory,
		$ProjectName
	)

	Write-Host "====== PUBLISHING PROJECT '$ProjectName' ======"
	dotnet publish "$project_root/src/$ProjectDirectory/$ProjectName/$ProjectName.csproj" -c Release --verbosity quiet /property:WarningLevel=0
}

Publish-Project Core Elegy.AppTemplate
Publish-Project Core Elegy.Common
Publish-Project Core Elegy.Framework
Publish-Project Core Elegy.RenderBackend

Publish-Project Generators Elegy.Framework.Generator

Publish-Project Modules Elegy.AssetSystem
Publish-Project Modules Elegy.ConsoleSystem
Publish-Project Modules Elegy.FileSystem
Publish-Project Modules Elegy.InputSystem
Publish-Project Modules Elegy.PlatformSystem
Publish-Project Modules Elegy.PluginSystem
Publish-Project Modules Elegy.RenderSystem
Publish-Project Modules Elegy.RenderWorld
