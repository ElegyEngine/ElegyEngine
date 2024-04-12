
Write-Host "========= BUILDING PACKAGES ========="

dotnet publish "src/Core/Elegy.AppTemplate/Elegy.AppTemplate.csproj" -c Release
dotnet publish "src/Core/Elegy.Common/Elegy.Common.csproj" -c Release
dotnet publish "src/Core/Elegy.Framework/Elegy.Framework.csproj" -c Release
dotnet publish "src/Core/Elegy.RenderBackend/Elegy.RenderBackend.csproj" -c Release

dotnet publish "src/Modules/Elegy.AssetSystem/Elegy.AssetSystem.csproj" -c Release
dotnet publish "src/Modules/Elegy.ConsoleSystem/Elegy.ConsoleSystem.csproj" -c Release
dotnet publish "src/Modules/Elegy.FileSystem/Elegy.FileSystem.csproj" -c Release
dotnet publish "src/Modules/Elegy.InputSystem/Elegy.InputSystem.csproj" -c Release
dotnet publish "src/Modules/Elegy.PlatformSystem/Elegy.PlatformSystem.csproj" -c Release
dotnet publish "src/Modules/Elegy.PluginSystem/Elegy.PluginSystem.csproj" -c Release
dotnet publish "src/Modules/Elegy.RenderSystem/Elegy.RenderSystem.csproj" -c Release
