
Write-Host "========= BUILDING PACKAGES ========="

dotnet publish "src/Core/Elegy.Common/Elegy.Common.csproj" -c Release
dotnet publish "src/Core/Elegy.Engine/Elegy.Engine.csproj" -c Release
dotnet publish "src/Core/Elegy.RenderBackend/Elegy.RenderBackend.csproj" -c Release
