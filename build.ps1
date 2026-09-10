#Requires -Version 5.1
<#
.SYNOPSIS
  Build MoreSlugHUD and stage a clean mod folder under dist/.

.EXAMPLE
  .\build.ps1

.EXAMPLE
  .\build.ps1 -Configuration Debug

.EXAMPLE
  .\build.ps1 -RWDir "D:\SteamLibrary\steamapps\common\Rain World"
#>
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',

    [string] $RWDir = '',

    [string] $IICDir = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$Root = $PSScriptRoot
$ProjectFile = Join-Path $Root 'MoreSlugHUD.csproj'
$ModInfoFile = Join-Path $Root 'modinfo.json'
$IdFile = Join-Path $Root 'id.txt'
$PluginsDir = Join-Path $Root 'plugins'
$ResolvedIICDir = if ([string]::IsNullOrWhiteSpace($IICDir)) { Join-Path $Root 'lib' } else { $IICDir }
$LibDll = Join-Path $ResolvedIICDir 'ImprovedInput.dll'
$AssemblyName = 'MoreSlugHUD'

function Write-Step([string] $Message) {
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Ensure-Directory([string] $Path) {
    if (-not (Test-Path -LiteralPath $Path)) {
        New-Item -ItemType Directory -Path $Path | Out-Null
    }
}

function Copy-IfExists([string] $Source, [string] $Destination) {
    if (Test-Path -LiteralPath $Source) {
        Copy-Item -LiteralPath $Source -Destination $Destination -Force
        return $true
    }

    return $false
}

Write-Step 'Checking prerequisites'
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw 'dotnet SDK not found. Install .NET SDK and ensure dotnet is on PATH.'
}

foreach ($path in @($ProjectFile, $ModInfoFile, $IdFile, $LibDll, (Join-Path $Root 'atlases\input_history.png'), (Join-Path $Root 'atlases\input_history.txt'))) {
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Missing required file: $path"
    }
}

$modInfo = Get-Content -LiteralPath $ModInfoFile -Raw -Encoding UTF8 | ConvertFrom-Json
$modId = (Get-Content -LiteralPath $IdFile -Raw -Encoding UTF8).Trim()
if ([string]::IsNullOrWhiteSpace($modId)) {
    throw "Mod id is empty: $IdFile"
}
if ($modInfo.id -ne $modId) {
    throw "Mod id mismatch: modinfo.json has '$($modInfo.id)', id.txt has '$modId'."
}

$dllSource = Join-Path $PluginsDir "$AssemblyName.dll"
$stageDir = Join-Path (Join-Path $Root 'dist') $modId
$buildArgs = @('build', $ProjectFile, '-c', $Configuration, '--nologo')
if (-not [string]::IsNullOrWhiteSpace($RWDir)) {
    $buildArgs += @('-p:RWDir=' + $RWDir)
}
if (-not [string]::IsNullOrWhiteSpace($IICDir)) {
    $buildArgs += @('-p:IICDir=' + $IICDir)
}

Write-Step "Building $AssemblyName ($Configuration)"
& dotnet @buildArgs
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed with exit code $LASTEXITCODE."
}
if (-not (Test-Path -LiteralPath $dllSource)) {
    throw "Built assembly not found: $dllSource"
}

Write-Step "Staging mod folder: $stageDir"
if (Test-Path -LiteralPath $stageDir) {
    Remove-Item -LiteralPath $stageDir -Recurse -Force
}
Ensure-Directory $stageDir
Ensure-Directory (Join-Path $stageDir 'plugins')

Copy-Item -LiteralPath $ModInfoFile -Destination (Join-Path $stageDir 'modinfo.json') -Force
Copy-Item -LiteralPath $IdFile -Destination (Join-Path $stageDir 'id.txt') -Force
Copy-Item -LiteralPath $dllSource -Destination (Join-Path $stageDir "plugins\$AssemblyName.dll") -Force

foreach ($directory in @('text', 'atlases')) {
    $source = Join-Path $Root $directory
    if (Test-Path -LiteralPath $source) {
        Copy-Item -LiteralPath $source -Destination (Join-Path $stageDir $directory) -Recurse -Force
    }
}

# Rain World splits strings.txt on CRLF only.
Get-ChildItem -LiteralPath (Join-Path $stageDir 'text') -Recurse -Filter 'strings.txt' -ErrorAction SilentlyContinue | ForEach-Object {
    $raw = [System.IO.File]::ReadAllText($_.FullName)
    $crlf = ($raw -replace "`r`n", "`n" -replace "`n", "`r`n").TrimEnd() + "`r`n"
    [System.IO.File]::WriteAllText($_.FullName, $crlf, [System.Text.UTF8Encoding]::new($false))
}

foreach ($asset in @('thumbnail.png', 'banner.png')) {
    if (Copy-IfExists (Join-Path $Root $asset) (Join-Path $stageDir $asset)) {
        Write-Host "  included $asset"
    }
}

Write-Step 'Validating staged mod'
foreach ($path in @(
    (Join-Path $stageDir 'modinfo.json'),
    (Join-Path $stageDir 'id.txt'),
    (Join-Path $stageDir "plugins\$AssemblyName.dll"),
    (Join-Path $stageDir 'text\text_eng\strings.txt'),
    (Join-Path $stageDir 'text\text_chi\strings.txt'),
    (Join-Path $stageDir 'atlases\input_history.png'),
    (Join-Path $stageDir 'atlases\input_history.txt')
)) {
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Staged mod validation failed, missing: $path"
    }
}

Write-Host ''
Write-Host 'Build complete.' -ForegroundColor Green
Write-Host "  Staged mod : $stageDir"
Write-Host "  DLL        : plugins\$AssemblyName.dll"


