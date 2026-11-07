param(
    [string]$GamePath = 'D:\SteamLibrary\steamapps\common\Subnautica',
    [switch]$BuildOnly,
    [switch]$HotReload
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
    (Join-Path $managed 'UnityEngine.InputLegacyModule.dll'), (Join-Path $managed 'UnityEngine.UI.dll'), (Join-Path $managed 'UnityEngine.UIModule.dll')
)
foreach ($reference in $references) { if (-not (Test-Path -LiteralPath $reference)) { throw "Missing reference: $reference" } }
$compilerArgs = @('/nologo', '/debug:pdbonly', '/target:library', "/out:$output") + @($references | ForEach-Object { "/reference:$_" }) + $sources
& $compiler @compilerArgs
if ($LASTEXITCODE -ne 0) { throw 'Compilation failed; existing installed plugin was left untouched.' }
Write-Host "Built: $output"
if ($BuildOnly) { return }
if ($HotReload) {
    $engine = Join-Path $pluginDir 'ScriptEngine.dll'
    $normalPlugin = Join-Path $pluginDir 'ZoredPdaScale.dll'
    if (-not (Test-Path -LiteralPath $engine)) { throw 'ScriptEngine is not installed; set it up with the game closed first.' }
    if (Test-Path -LiteralPath $normalPlugin) { throw 'Remove the normal ZoredPdaScale.dll with the game closed before using ScriptEngine; never load both copies.' }
    $scripts = Join-Path $game 'BepInEx\scripts'
    New-Item -ItemType Directory -Force -Path $scripts | Out-Null
    Copy-Item -LiteralPath ([IO.Path]::ChangeExtension($output, '.pdb')) -Destination (Join-Path $scripts 'ZoredPdaScale.pdb') -Force
    Copy-Item -LiteralPath $output -Destination (Join-Path $scripts 'ZoredPdaScale.dll') -Force
    Write-Host 'Updated script; press F6 in game to reload.'
    return
}
if (Test-Path -LiteralPath (Join-Path $game 'BepInEx\scripts\ZoredPdaScale.dll')) { throw 'ScriptEngine copy exists; remove it before installing the normal plugin.' }
if (Get-Process -Name Subnautica -ErrorAction SilentlyContinue) { throw 'Close Subnautica before installing; build succeeded.' }
$destination = Join-Path $pluginDir 'ZoredPdaScale.dll'
if (Test-Path -LiteralPath $destination) { Copy-Item -LiteralPath $destination -Destination "$destination.bak" -Force }
Copy-Item -LiteralPath $output -Destination $destination -Force
if ((Get-FileHash -LiteralPath $output).Hash -ne (Get-FileHash -LiteralPath $destination).Hash) { throw 'Installed file hash does not match build.' }
Write-Host "Installed: $destination"
