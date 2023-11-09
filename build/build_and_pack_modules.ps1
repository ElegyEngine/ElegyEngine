
Write-Host "========= BUILDING PACKAGES ========="

dotnet publish "src/Elegy.Common/Elegy.Common.csproj" -c Release
dotnet publish "src/Elegy.Engine/Elegy.Engine.csproj" -c Release
