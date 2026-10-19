$ErrorActionPreference = 'Stop'
$runtimeDirectory = Join-Path $PSScriptRoot '.runtime'
$settingsPath = Join-Path $runtimeDirectory 'settings.json'
if (-not (Test-Path -LiteralPath $settingsPath)) { throw 'Run Setup.cmd first.' }
$settings = Get-Content -LiteralPath $settingsPath -Raw | ConvertFrom-Json
$serverDirectory = Join-Path $runtimeDirectory 'server'
$serverExecutable = Join-Path $serverDirectory 'Imlight.Director.exe'
$clientExecutable = Join-Path $runtimeDirectory 'client\Bin\WizardGraphicalClient.exe'
$hostScript = Join-Path $PSScriptRoot 'host.py'
if (-not (Test-Path -LiteralPath $clientExecutable)) { throw 'Run Setup.cmd first to copy the client.' }
$dotnetDirectory = Split-Path $settings.dotnet -Parent
$env:DOTNET_ROOT = $dotnetDirectory
$env:DOTNET_ROOT_X64 = $dotnetDirectory
$env:PATH = "$dotnetDirectory;$env:PATH"

$server = Get-CimInstance Win32_Process -Filter "Name = 'Imlight.Director.exe'" |
    Where-Object { $_.ExecutablePath -eq $serverExecutable } | Select-Object -First 1
$hostProcess = Get-CimInstance Win32_Process |
    Where-Object { $_.ExecutablePath -eq $settings.python -and $_.CommandLine -and $_.CommandLine.Contains($hostScript) } |
    Select-Object -First 1
if (-not $server) {
    $ports = @(12010, 12343, 12344, 12345, 12346, 12347, 12348, 12510, 8090, 13009)
    if (-not $hostProcess) { $ports += 12379 }
    foreach ($port in $ports) {
        $listener = New-Object System.Net.Sockets.TcpListener([System.Net.IPAddress]::Loopback, $port)
        try { $listener.Start() }
        catch { throw "Port $port is in use. Stop the other local server, then try again." }
        finally { $listener.Stop() }
    }
}
$startedHost = $null
$startedServer = $null
try {
    if (-not $hostProcess) {
        $startedHost = Start-Process -FilePath $settings.python -ArgumentList ('"{0}"' -f $hostScript) `
            -WorkingDirectory $PSScriptRoot -WindowStyle Hidden `
            -RedirectStandardOutput (Join-Path $runtimeDirectory 'patch-output.log') `
            -RedirectStandardError (Join-Path $runtimeDirectory 'patch-error.log') -PassThru
    }
    $patchReady = $false
    for ($attempt = 0; $attempt -lt 30; $attempt++) {
        try {
            $null = Invoke-WebRequest 'http://127.0.0.1:12379/V_r806919.Wizard_1_610/LatestFileList.bin' `
                -Method Head -UseBasicParsing -TimeoutSec 2
            $patchReady = $true
            break
        }
        catch { Start-Sleep -Milliseconds 500 }
    }
    if (-not $patchReady) { throw 'Patch service failed. See .runtime/patch-error.log.' }
    if (-not $server) {
        $server = Start-Process -FilePath $serverExecutable -WorkingDirectory $serverDirectory -WindowStyle Hidden `
            -RedirectStandardOutput (Join-Path $runtimeDirectory 'server-output.log') `
            -RedirectStandardError (Join-Path $runtimeDirectory 'server-error.log') -PassThru
        $startedServer = $server
    }
    else {
        $server = Get-Process -Id $server.ProcessId
    }
    $ready = $false
    $deadline = (Get-Date).AddMinutes(4)
    do {
        $server.Refresh()
        if ($server.HasExited) {
            $details = Get-Content -LiteralPath (Join-Path $runtimeDirectory 'server-error.log') -Raw -ErrorAction SilentlyContinue
            throw "Server stopped. $details"
        }
        if ((Test-Path (Join-Path $runtimeDirectory 'server-output.log')) -and
            (Select-String -LiteralPath (Join-Path $runtimeDirectory 'server-output.log') `
                -Pattern 'Imlight may now be connected to' -Quiet)) {
            $ready = $true
            break
        }
        Start-Sleep -Seconds 1
    } while ((Get-Date) -lt $deadline)
    if (-not $ready) { throw 'Server startup timed out. See .runtime/server-output.log.' }
    $client = Get-CimInstance Win32_Process -Filter "Name = 'WizardGraphicalClient.exe'" |
        Where-Object { $_.ExecutablePath -eq $clientExecutable }
    if (-not $client) {
        Start-Process -FilePath $clientExecutable -ArgumentList '-L', '127.0.0.1', '12010' `
            -WorkingDirectory (Split-Path $clientExecutable -Parent)
    }
}
catch {
    $launchError = $_
    foreach ($launched in @($startedServer, $startedHost)) {
        if ($null -eq $launched) { continue }
        try {
            $launched.Refresh()
            if (-not $launched.HasExited) {
                Stop-Process -Id $launched.Id
                $null = $launched.WaitForExit(5000)
            }
        }
        catch { }
    }
    throw $launchError
}
