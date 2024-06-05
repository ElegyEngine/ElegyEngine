
## This script was designed to be run multiple times.
## Step 1 will produce NuGet cache, step 2 clears it,
## step 3 provides new data for the cache that will
## be produced in step 1. And the cycle repeats.

## This is a workaround around NuGet's caching system,
## so we can develop on the same local NuGet package version.

## Build and pack Elegy.Engine and Elegy.Common
Invoke-Expression "$PSScriptRoot/build_and_pack_modules.ps1"

## Clean NuGet cache so we can reload the packages
Invoke-Expression "$PSScriptRoot/clean_cache.ps1"

## Copy to local NuGet developer package repo
Invoke-Expression "$PSScriptRoot/copy_to_local_dev_packs.ps1"

Write-Host ""
Write-Host "========= DONE ========="
Write-Host ""
