
Write-Host "========= CLEARING NUGET CACHE ========="

$cache_dir = "$env:USERPROFILE/.nuget/packages/"

$folder_names = Get-ChildItem $cache_dir -Filter "elegy.*"
if ( !$folder_names )
{
	Write-Host "All clear!"
	Return
}

Write-Host "Clearing NuGet cache... this will delete all your Elegy.* NuGet packages from cache."
Write-Host "Why? Well, this way you can easily update your local dev packages if you're working on the engine."

foreach ( $folder in $folder_names )
{
	Write-Host "Clearing '$folder'"
	Remove-Item $folder -Recurse -Force
}

Write-Host "All clear now."
