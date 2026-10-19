[CmdletBinding()]
param([string]$Dotnet = 'dotnet')

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$fixture = Join-Path $root '.build\test-assembly-version'
$null = New-Item -ItemType Directory -Path $fixture -Force
$fixtureProject = Join-Path $fixture 'Fixture.csproj'
$projects = @{
    'Imlight.CoreLib' = Join-Path $root 'server-build\Imlight.CoreLib.csproj'
    'Imlight.Director' = Join-Path $root 'director-build\Imlight.Director.csproj'
}
Set-Content -LiteralPath (Join-Path $fixture 'Fixture.cs') -Value 'internal sealed class Fixture { }'

foreach ($assembly in $projects.Keys) {
    foreach ($version in @('1.0.0.0', '26.4.42.265')) {
        $projectText = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <AssemblyName>$assembly</AssemblyName>
    <AssemblyVersion>$version</AssemblyVersion>
  </PropertyGroup>
</Project>
"@
        Set-Content -LiteralPath $fixtureProject -Value $projectText
        & $Dotnet build $fixtureProject -c Release -v quiet
        if ($LASTEXITCODE -ne 0) { throw 'Version fixture build failed.' }
        $directory = Join-Path $fixture 'bin\Release\net10.0'
        $actual = & $Dotnet msbuild $projects[$assembly] '-target:MatchInstalledAssemblyVersion' `
            '-getProperty:AssemblyVersion' "-p:ImlightDirectory=$directory"
        if ($LASTEXITCODE -ne 0 -or $actual.Trim() -ne $version) {
            throw "Expected $assembly version $version, received $actual."
        }
    }
}

Write-Output 'Launcher and library match both installed assembly versions correctly.'
