param(
    [string]$GamePath = 'D:\SteamLibrary\steamapps\common\Subnautica',
    [switch]$BuildOnly
)
$ErrorActionPreference = 'Stop'
$game = (Resolve-Path -LiteralPath $GamePath).Path
$managed = Join-Path $game 'Subnautica_Data\Managed'
$core = Join-Path $game 'BepInEx\core'
$pluginDir = Join-Path $game 'BepInEx\plugins'
$sources = @(Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'ZoredPdaScale') -Filter '*.cs' | ForEach-Object { $_.FullName })
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$buildDir = Join-Path $PSScriptRoot 'ZoredPdaScale\build'
if (-not (Test-Path -LiteralPath (Join-Path $game 'Subnautica.exe'))) { throw 'Subnautica.exe not found.' }
if (-not (Test-Path -LiteralPath (Join-Path $pluginDir 'SubmersedVR.dll'))) { throw 'SubmersedVR.dll not found.' }
if (-not (Test-Path -LiteralPath $compiler)) { throw 'C# compiler from .NET Framework not found.' }
New-Item -ItemType Directory -Force -Path $buildDir | Out-Null
$output = Join-Path $buildDir 'ZoredPdaScale.dll'
$references = @(
    (Join-Path $core 'BepInEx.dll'), (Join-Path $core '0Harmony.dll'),
    (Join-Path $managed 'Assembly-CSharp.dll'), (Join-Path $managed 'Assembly-CSharp-firstpass.dll'),
    (Join-Path $managed 'UnityEngine.dll'), (Join-Path $managed 'UnityEngine.CoreModule.dll'),
    (Join-Path $managed 'UnityEngine.InputLegacyModule.dll'), (Join-Path $managed 'UnityEngine.UI.dll')
)
foreach ($reference in $references) { if (-not (Test-Path -LiteralPath $reference)) { throw "Missing reference: $reference" } }
$compilerArgs = @('/nologo', '/target:library', "/out:$output") + @($references | ForEach-Object { "/reference:$_" }) + $sources
& $compiler @compilerArgs
if ($LASTEXITCODE -ne 0) { throw 'Compilation failed; existing installed plugin was left untouched.' }
Write-Host "Built: $output"
if ($BuildOnly) { return }
if (Get-Process -Name Subnautica -ErrorAction SilentlyContinue) { throw 'Close Subnautica before installing; build succeeded.' }
$destination = Join-Path $pluginDir 'ZoredPdaScale.dll'
if (Test-Path -LiteralPath $destination) { Copy-Item -LiteralPath $destination -Destination "$destination.bak" -Force }
Copy-Item -LiteralPath $output -Destination $destination -Force
if ((Get-FileHash -LiteralPath $output).Hash -ne (Get-FileHash -LiteralPath $destination).Hash) { throw 'Installed file hash does not match build.' }
Write-Host "Installed: $destination"
