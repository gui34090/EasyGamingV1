[CmdletBinding()]
param([switch]$Strict)
Write-Host "Validating branding assets..."
$root = Split-Path -Parent $PSScriptRoot
$svg = Join-Path $root "assets/branding/icon.svg"
if (!(Test-Path $svg)) {
  if ($Strict) { throw "EG-ASSET-001: icon.svg missing" }
  else { Write-Warning "icon.svg missing (non-strict)"; return }
}
# Very light check
if ((Get-Content $svg -Raw).Length -lt 100) {
  throw "EG-ASSET-002: icon.svg too small"
}
Write-Host "Assets OK"
