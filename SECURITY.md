# Security

- No code injection, no API hooking across processes, no kernel drivers.
- Overlay window is top-level, desktop-only. It never attaches to or modifies game processes.
- Optional `SetWindowDisplayAffinity(WDA_EXCLUDEFROMCAPTURE)` is **off by default**.
- Supply chain:
  - SBOM (SPDX + CycloneDX) generated in CI
  - Build provenance attestation (Sigstore/in-toto) emitted in CI
- Report issues via GitHub Security advisories.
