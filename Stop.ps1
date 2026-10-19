$ErrorActionPreference = 'Stop'
$runtimeDirectory = Join-Path $PSScriptRoot '.runtime'
$settingsPath = Join-Path $runtimeDirectory 'settings.json'
if (-not (Test-Path -LiteralPath $settingsPath)) { exit 0 }
$settings = Get-Content -LiteralPath $settingsPath -Raw | ConvertFrom-Json
$clientExecutable = Join-Path $runtimeDirectory 'client\Bin\WizardGraphicalClient.exe'
$serverExecutable = Join-Path $runtimeDirectory 'server\Imlight.Director.exe'
$hostScript = Join-Path $PSScriptRoot 'host.py'
$client = Get-CimInstance Win32_Process -Filter "Name = 'WizardGraphicalClient.exe'" |
    Where-Object { $_.ExecutablePath -eq $clientExecutable }
if ($client) { throw 'Close the randomizer game, then run Stop.cmd again.' }
Get-CimInstance Win32_Process -Filter "Name = 'Imlight.Director.exe'" |
    Where-Object { $_.ExecutablePath -eq $serverExecutable } |
    ForEach-Object {
        $server = Get-Process -Id $_.ProcessId
        Stop-Process -Id $_.ProcessId
        $server.WaitForExit()
    }
Get-CimInstance Win32_Process |
    Where-Object { $_.ExecutablePath -eq $settings.python -and $_.CommandLine -and $_.CommandLine.Contains($hostScript) } |
    ForEach-Object { Stop-Process -Id $_.ProcessId }
