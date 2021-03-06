# ****************************************************************************
# VSTSSetVersion.ps1
# Description: Search/replace tools to update build version numbers
# Uses BUILD_BUILDID as a build number
# ****************************************************************************

Write-Host Executing VSTSSetVersion.ps1

# Stop on errors
$ErrorActionPreference = "Stop" 

$searchtext = "0\.9\.0\.0"
$replacetext = "0.9." + $Env:BUILD_BUILDID + ".0"

Write-Host Replacing $searchtext with $replacetext

$files = Get-ChildItem -Recurse -include AssemblyInfo.cs

foreach ($file in $files)
{
	Write-Host "File:" $file.FullName
	(Get-Content $file.PSPath) |
	Foreach-Object { $_ -replace $searchtext, $replacetext } |
	Set-Content $file.PSPath
}

Write-Host VSTSSetVersion.ps1 execution done