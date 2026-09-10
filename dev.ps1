#Requires -Version 5.1
<#
.SYNOPSIS
  Link, unlink, or inspect the staged mod in Rain World's mods folder.
#>
[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [ValidateSet('link', 'unlink', 'status')]
    [string] $Action = 'link',

    [string] $RWDir = '',

    [switch] $Force
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$Root = $PSScriptRoot
$ModInfoFile = Join-Path $Root 'modinfo.json'
$IdFile = Join-Path $Root 'id.txt'
$GamePathsLocal = Join-Path $Root 'GamePaths.local.props'
$GamePathsShared = Join-Path $Root 'GamePaths.props'
$AssemblyName = 'MoreSlugHUD'

function Get-RWDirFromProps {
    foreach ($file in @($GamePathsLocal, $GamePathsShared)) {
        if (-not (Test-Path -LiteralPath $file)) {
            continue
        }

        [xml] $xml = Get-Content -LiteralPath $file -Raw -Encoding UTF8
        $value = $xml.Project.PropertyGroup.RWDir
        if (-not [string]::IsNullOrWhiteSpace($value)) {
            return $value.Trim().TrimEnd('\', '/')
        }
    }

    return $null
}

function Resolve-RWDir {
    if (-not [string]::IsNullOrWhiteSpace($RWDir)) {
        return $RWDir.Trim().TrimEnd('\', '/')
    }

    $fromProps = Get-RWDirFromProps
    if ($fromProps) {
        return $fromProps
    }

    throw 'Pass -RWDir or set RWDir in GamePaths.local.props.'
}

function Test-Junction([string] $Path) {
    if (-not (Test-Path -LiteralPath $Path)) {
        return $false
    }

    return ([IO.FileAttributes]::ReparsePoint -band (Get-Item -LiteralPath $Path).Attributes) -ne 0
}

$modInfo = Get-Content -LiteralPath $ModInfoFile -Raw -Encoding UTF8 | ConvertFrom-Json
$modId = (Get-Content -LiteralPath $IdFile -Raw -Encoding UTF8).Trim()
if ([string]::IsNullOrWhiteSpace($modId) -or $modInfo.id -ne $modId) {
    throw 'modinfo.json and id.txt must contain the same non-empty mod id.'
}

$resolvedRWDir = Resolve-RWDir
$modsRoot = Join-Path $resolvedRWDir 'RainWorld_Data\StreamingAssets\mods'
$linkPath = Join-Path $modsRoot $modId
$stageDir = Join-Path (Join-Path $Root 'dist') $modId

if (-not (Test-Path -LiteralPath $modsRoot)) {
    throw "Game mods directory not found: $modsRoot"
}

if ($Action -eq 'status') {
    Write-Host "Game path : $linkPath"
    Write-Host "Stage dir : $stageDir"
    if (-not (Test-Path -LiteralPath $linkPath)) {
        Write-Host 'State     : not linked' -ForegroundColor Yellow
    } elseif (Test-Junction $linkPath) {
        Write-Host "State     : junction -> $((Get-Item -LiteralPath $linkPath).Target)" -ForegroundColor Green
    } else {
        Write-Host 'State     : real directory (not managed by this script)' -ForegroundColor Yellow
    }
    return
}

if ($Action -eq 'unlink') {
    if (-not (Test-Path -LiteralPath $linkPath)) {
        Write-Host 'Nothing to unlink.'
        return
    }
    if (-not (Test-Junction $linkPath)) {
        throw "Refusing to remove a real directory: $linkPath"
    }
    $expected = (Resolve-Path -LiteralPath $stageDir).Path
    $actual = [string](Get-Item -LiteralPath $linkPath).Target
    if ([string]::IsNullOrWhiteSpace($actual)) {
        throw "Could not resolve junction target: $linkPath"
    }
    $actual = $actual.TrimEnd('\', '/')
    $expectedNorm = $expected.TrimEnd('\', '/')
    if (-not [string]::Equals($actual, $expectedNorm, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to remove a junction that does not point at this project's staging folder.`n  link   : $linkPath`n  actual : $actual`n  expect : $expectedNorm"
    }
    if ($PSCmdlet.ShouldProcess($linkPath, 'Remove junction')) {
        Remove-Item -LiteralPath $linkPath -Force
    }
    return
}

$required = @(
    (Join-Path $stageDir 'modinfo.json'),
    (Join-Path $stageDir "plugins\$AssemblyName.dll")
)
foreach ($path in $required) {
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Staged mod is incomplete; run .\build.ps1 first. Missing: $path"
    }
}

if (Test-Path -LiteralPath $linkPath) {
    if (-not (Test-Junction $linkPath)) {
        throw "Refusing to replace a real directory: $linkPath"
    }
    if (-not $Force) {
        throw 'A junction already exists. Use -Force to replace it.'
    }
    if ($PSCmdlet.ShouldProcess($linkPath, 'Replace existing junction')) {
        Remove-Item -LiteralPath $linkPath -Force
    }
}

$target = (Resolve-Path -LiteralPath $stageDir).Path
if ($PSCmdlet.ShouldProcess($linkPath, "Create junction -> $target")) {
    New-Item -ItemType Junction -Path $linkPath -Target $target | Out-Null
    Write-Host "Linked $linkPath -> $target" -ForegroundColor Green
}



