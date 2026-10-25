param([string]$ClientPath = $PSScriptRoot)
$ErrorActionPreference = 'Stop'
$ClientPath = (Resolve-Path -LiteralPath $ClientPath).Path
$relativePaths = @('PatchClient/BankA/PatchConfig.xml', 'PatchClient/BankB/PatchConfig.xml', 'Bin/PatchConfig.xml')
$files = @($relativePaths | ForEach-Object { Join-Path $ClientPath $_ } | Where-Object { Test-Path -LiteralPath $_ })
if ($files.Count -eq 0) { throw 'Place this script in your Spiral client folder, or pass -ClientPath with that folder.' }
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
foreach ($file in $files) {
    $text = [IO.File]::ReadAllText($file)
    $replacements = @{
        '(<PatchServerHostname\s+host=")[^"]*(")' = '${1}70.23.164.208${2}'
        '(<LoginHostname\s+host=")[^"]*(")' = '${1}70.23.164.208${2}'
        '(<CommandLine\s+args=")[^"]*(")' = '${1}-L 70.23.164.208 12010 @LOCALE_PARAMETERS${2}'
    }
    foreach ($pattern in $replacements.Keys) {
        if ([regex]::Matches($text, $pattern).Count -ne 1) { throw "Unexpected launcher configuration in $file." }
    }
    Copy-Item -LiteralPath $file -Destination "$file.before-public-$stamp" -ErrorAction Stop
    foreach ($pattern in $replacements.Keys) { $text = [regex]::Replace($text, $pattern, $replacements[$pattern]) }
    $text = [regex]::Replace($text, '(<(?:RegistrationURL|MyAccountURL)\s+path=")[^"]*(")', '${1}https://spiral.hypercoloss.us/${2}')
    [IO.File]::WriteAllText($file, $text)
    Write-Host "Configured $file (backup saved)."
}
Write-Host 'Register or sign in at https://spiral.hypercoloss.us/ from this same internet connection.'
Write-Host 'Wait 35 seconds, then open the Spiral launcher and use the same username and password.'
