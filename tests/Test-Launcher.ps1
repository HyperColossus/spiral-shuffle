$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$fixture = Join-Path $root '.build\test-launcher'
$runtime = Join-Path $fixture '.runtime'
$null = New-Item -ItemType Directory -Path (Join-Path $runtime 'client\Bin') -Force
Copy-Item -LiteralPath (Join-Path $root 'Play.ps1') -Destination (Join-Path $fixture 'Play.ps1') -Force
Set-Content -LiteralPath (Join-Path $runtime 'client\Bin\WizardGraphicalClient.exe') -Value 'fixture'
@{ python = 'fixture-python.exe'; dotnet = 'fixture-dotnet\dotnet.exe' } |
    ConvertTo-Json | Set-Content -LiteralPath (Join-Path $runtime 'settings.json')
$originalPath = $env:PATH
$originalDotnet = $env:DOTNET_ROOT
$originalDotnetX64 = $env:DOTNET_ROOT_X64

try {
    foreach ($scenario in @('crash', 'existing-host', 'timeout', 'success')) {
        & {
            $stopped = [System.Collections.Generic.List[int]]::new()
            $hostScript = Join-Path $fixture 'host.py'
            $errorLog = Join-Path $runtime 'server-error.log'
            Set-Content -LiteralPath $errorLog -Value 'FileNotFoundException: Imlight.CoreLib'
            $readyText = if ($scenario -eq 'success') { 'Imlight may now be connected to' } else { '' }
            Set-Content -LiteralPath (Join-Path $runtime 'server-output.log') -Value $readyText

            function New-FixtureProcess([int]$Id, [bool]$Exited) {
                $process = [pscustomobject]@{ Id = $Id; HasExited = $Exited }
                $process | Add-Member -MemberType ScriptMethod -Name Refresh -Value { }
                $process | Add-Member -MemberType ScriptMethod -Name WaitForExit -Value { param([int]$Timeout) return $true }
                return $process
            }

            function Get-CimInstance {
                param([string]$ClassName, [string]$Filter)
                if (-not $Filter -and $scenario -eq 'existing-host') {
                    [pscustomobject]@{ ExecutablePath = 'fixture-python.exe'; CommandLine = $hostScript }
                }
            }

            function New-Object {
                param([string]$TypeName, [object[]]$ArgumentList)
                $listener = [pscustomobject]@{ }
                $listener | Add-Member -MemberType ScriptMethod -Name Start -Value { }
                $listener | Add-Member -MemberType ScriptMethod -Name Stop -Value { }
                return $listener
            }

            function Start-Process {
                param([string]$FilePath, [string[]]$ArgumentList, [string]$WorkingDirectory,
                    [string]$WindowStyle, [string]$RedirectStandardOutput, [string]$RedirectStandardError,
                    [switch]$PassThru)
                if ($FilePath -eq 'fixture-python.exe') { return New-FixtureProcess 41 $false }
                if ($FilePath.EndsWith('Imlight.Director.exe')) {
                    return New-FixtureProcess 42 ($scenario -in @('crash', 'existing-host'))
                }
            }

            function Invoke-WebRequest { param($Uri, $Method, [switch]$UseBasicParsing, $TimeoutSec) }
            function Start-Sleep { param($Seconds, $Milliseconds) }
            function Stop-Process { param([int]$Id) $stopped.Add($Id) }

            function Get-Date {
                $script:fixtureDateCalls++
                if ($scenario -eq 'timeout' -and $script:fixtureDateCalls -gt 1) {
                    return [datetime]'2030-01-01T00:05:00'
                }
                return [datetime]'2030-01-01T00:00:00'
            }

            $script:fixtureDateCalls = 0
            $failure = $null
            try { . (Join-Path $fixture 'Play.ps1') }
            catch { $failure = $_.Exception.Message }
            $expected = switch ($scenario) {
                'crash' { @(41) }
                'existing-host' { @() }
                'timeout' { @(42, 41) }
                'success' { @() }
            }
            if (($stopped -join ',') -ne ($expected -join ',')) {
                throw "Wrong process cleanup for $scenario : $stopped."
            }
            if ($scenario -in @('crash', 'existing-host') -and $failure -notlike '*FileNotFoundException*') {
                throw "Missing startup error details for $scenario : $failure."
            }
            if ($scenario -eq 'timeout' -and $failure -notlike '*timed out*') { throw "Missing timeout error: $failure" }
            if ($scenario -eq 'success' -and $failure) { throw $failure }
        }
    }
}
finally {
    $env:PATH = $originalPath
    $env:DOTNET_ROOT = $originalDotnet
    $env:DOTNET_ROOT_X64 = $originalDotnetX64
}

Write-Output 'Launcher crash, timeout, shared-process and success checks passed.'
