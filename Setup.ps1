[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)][string]$RevivePath,
    [string]$ClientPath,
    [string]$Seed = 'SPIRAL-101'
)

$ErrorActionPreference = 'Stop'
$setupArguments = @((Join-Path $PSScriptRoot 'setup.py'), '--revive-path', $RevivePath, '--seed', $Seed)
if ($ClientPath) { $setupArguments += @('--client-path', $ClientPath) }
if (Get-Command py -ErrorAction SilentlyContinue) {
    & py -3 @setupArguments
}
elseif (Get-Command python -ErrorAction SilentlyContinue) {
    & python @setupArguments
}
else {
    throw 'Install Python 3.12 or newer first.'
}
exit $LASTEXITCODE
