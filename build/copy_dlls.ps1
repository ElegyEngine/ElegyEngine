
Write-Host "========= COPYING MODULES ========="

## Put your own paths here
$input_dir = Get-Location ## Might wanna replace this with something like "$PSScriptRoot/.." but oh well
$output_dir = "E:/Workfolders/Elegy/Games/TestBench"

## Create the bin directory if it doesn't exist
if ( !(Test-Path -Path "$output_dir/game") )
{
	Write-Output "There is no 'game' folder in '$output_dir', creating one right now..."
	New-Item -Path "$output_dir/game" -ItemType directory
}

## Copy the debug DLLs there
Copy-Item "$input_dir/src/Elegy.DevConsole/bin/Debug/net6.0/Elegy.DevConsole.*"	-Destination "$output_dir/engine/plugins"
Copy-Item "$input_dir/src/Elegy.Common/bin/Debug/net6.0/Elegy.Common.*"			-Destination "$output_dir"
Copy-Item "$input_dir/src/Elegy.Engine/bin/Debug/net6.0/Elegy.Engine.*"			-Destination "$output_dir"

Write-Output "Successfully copied Elegy.DevConsole.dll, Elegy.Common.dll and Elegy.Engine.dll into '$output_dir'"
