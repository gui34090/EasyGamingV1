[CmdletBinding()]
param(
  [ValidateSet("Preflight","Fix","Build","Test","Package")]
  [string[]]$Mode = @("Preflight"),
  [switch]$CI
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$report = Join-Path $root "artifacts/doctor-report.md"
New-Item -ItemType Directory -Force -Path (Split-Path $report) | Out-Null
"## Doctor Report`n" | Set-Content $report -Encoding UTF8

function Write-Rep($s) { Add-Content $report $s }

function Invoke-WithJitter([scriptblock]$Action, [int]$Max=3) {
  $rand = New-Object System.Random
  for ($i=1; $i -le $Max; $i++) {
    try { & $Action; return } catch {
      Start-Sleep -Milliseconds ($rand.Next(50, 250) * $i)
      if ($i -eq $Max) { throw }
    }
  }
}

if ($Mode -contains "Preflight") {
  Write-Rep "### Preflight"
  # Check SDK
  $sdk = (dotnet --version)
  Write-Rep "* .NET SDK: $sdk"
  # Validate data hash
  $data = Join-Path $root "data/game-profiles.json"
  $sha = Join-Path $root "data/game-profiles.json.sha256"
  if (!(Test-Path $data) -or !(Test-Path $sha)) { throw "Data files missing." }
  $calc = (Get-FileHash $data -Algorithm SHA256).Hash.ToLower()
  $exp = (Get-Content $sha -Raw).Trim().ToLower()
  if ($calc -ne $exp) { throw "Data SHA256 mismatch." } else { Write-Rep "* Data SHA256 OK" }
}

if ($Mode -contains "Fix") {
  Write-Rep "### Fix"
  # Placeholder: normalize line endings, ensure UTF-8
  Write-Rep "* Normalized line endings & ensured UTF-8 (no-op)."
}

if ($Mode -contains "Build") {
  Write-Rep "### Build"
  Invoke-WithJitter { dotnet build "$root/src/EasyGamingV1.App/EasyGamingV1.App.csproj" -c Release } 3
  Write-Rep "* Build OK"
}

if ($Mode -contains "Test") {
  Write-Rep "### Test"
  Invoke-WithJitter { dotnet test "$root/tests/EasyGamingV1.Tests/EasyGamingV1.Tests.csproj" -c Release --no-build } 3
  Write-Rep "* Tests OK"
}

if ($Mode -contains "Package") {
  Write-Rep "### Package"
  & "$root/tools/package-portable.ps1" -Configuration Release -Runtime win-x64
  Write-Rep "* Portable ZIP created"
}

Write-Host "Doctor OK. Report: $report"
