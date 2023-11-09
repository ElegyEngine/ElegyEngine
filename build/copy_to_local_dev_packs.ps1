
Write-Host "========= COPYING TO LOCAL NUGET REPO ========="

## Your NuGet package repo here
$LocalDevPacks = "C:/LocalNuGetPackages"

$CurrentDir = Get-Location
$Files = Get-ChildItem -File -Filter "Elegy*.nupkg" -Recurse

foreach ( $file in $Files )
{
	Write-Host "I see a '$file'"

	if ( $file -like "*Release*" )
	{
		$filePath = $file.FullName
		$fileName = $file.Name
		$fileRelative = $filePath.Replace( "$CurrentDir\", "" )

		Copy-Item -Path $file -Destination "$LocalDevPacks/" -Force
		Write-Host "Copied $fileRelative into '$LocalDevPacks/$fileName'"
	}
}
