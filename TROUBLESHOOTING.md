# Troubleshooting

- **EG-BOOT-001**: Windows App Runtime bootstrapper prompt  
  *Resolution*: Accept per-user install. No admin required.

- **EG-OVERLAY-010**: Overlay not visible  
  *Check*: Not behind a fullscreen exclusive mode. Disable games' exclusive fullscreen or run borderless.

- **EG-DATA-020**: `game-profiles.json` hash mismatch  
  *Resolution*: Re-extract ZIP; don't modify data files.

- **EG-RAWINPUT-030**: No Raw Input events  
  *Check*: App must remain focused once for registration to stick. Ensure no global blockers (other hooks).

Logs: `%LocalAppData%\EasyGaming\logs`
