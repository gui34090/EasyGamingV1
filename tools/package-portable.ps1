[CmdletBinding()]
param(
  [string]$Configuration = "Release",
  [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$out = Join-Path $root "artifacts/portable/EasyGamingV1"
if (Test-Path $out) { Remove-Item $out -Recurse -Force }
New-Item -ItemType Directory -Force -Path $out | Out-Null

Write-Host "Publishing self-contained single-file..."
dotnet publish "$root/src/EasyGamingV1.App/EasyGamingV1.App.csproj" `
  -c $Configuration -r $Runtime `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:SelfContained=true `
  -p:DebugType=None `
  -p:DebugSymbols=false

$pub = Get-ChildItem "$root/src/EasyGamingV1.App/bin/$Configuration/$Runtime/publish" -Directory:$false
Copy-Item $pub.FullName -Destination $out -Recurse -Force

# Ship data alongside EXE
Copy-Item "$root/data/*" -Destination (Join-Path $out "data") -Recurse -Force

# Zip
$zip = Join-Path $root "artifacts/EasyGamingV1_Portable_${Configuration}_$Runtime.zip"
if (Test-Path $zip) { Remove-Item $zip -Force }
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($out, $zip)
Write-Host "Created $zip"
