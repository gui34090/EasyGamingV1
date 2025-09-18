# EasyGamingV1 (Portable)

Unpackaged WinUI 3 (.NET 8) desktop utility — **portable .exe only**.  
Overlay is **SAFE** (top-level window), **no injection/hook/driver**.  
Windows 11 23H2+.

## Quick Start
1. **Build**: `dotnet build -c Release`
2. **Package Portable**: `pwsh ./tools/package-portable.ps1`
3. **Run**: unzip `artifacts/EasyGamingV1_Portable_Release_win-x64.zip` → `EasyGamingV1.exe`  
   If Windows App Runtime is missing, the app will bootstrap it (per-user).

## Features
- WinUI 3 UI, dark theme, FR resources
- Overlay crosshair (Safe Mode default), DPI aware
- Raw Input (mouse) wired to our own window only
- Converter (cm/360 baseline), tests
- ETW providers + JSONL logs
- Data validation of `/data/game-profiles.json` vs `.sha256`

## Anti-Cheat Compliance
- **Zero** process injection, hooks, or kernel drivers
- **Desktop-only** overlay (top-level layered window)
- Optional anti-capture (`WDA_EXCLUDEFROMCAPTURE`), **off by default**

## Build Matrix
- **Runtime**: `win-x64`
- **.NET**: 8.0.x (LTS)
- **Windows App SDK**: 1.7.x

## CI
Pipelines: preflight → doctor → build → test → package → sbom → attest → release.  
Sign is disabled unless secrets are provided.

## Data
`/data/game-profiles.json` (v1) is shipped with schema + SHA256 + placeholder `.sig`.

## License
MIT
