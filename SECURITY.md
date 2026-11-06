# Security

This tool stores snapshots of local Riot Client authentication sessions. Treat these as **secrets**, even when encrypted.

## Report a vulnerability

Please do **not** open a public GitHub issue containing secrets, session files, screenshots showing authentication data, tokens, or detailed exploitation steps. Contact the project maintainer privately via their public GitHub profile to arrange secure reporting.

## Data handling

- The app does not ask for the Riot password.
- Saved sessions and recovery snapshots are encrypted using Windows DPAPI and remain bound to the local Windows user context.
- `profiles.json` is plain-text metadata and can contain public Riot IDs or profile URLs.
- DPAPI encryption is **not** protection against compromise by malware running as the same Windows user.
- Never publish files from `%LOCALAPPDATA%\RiotSwitcherAccount`, Riot Client private YAML files, or unchecked error logs.
- If you believe credentials/session data were exposed, log out sessions and rotate account credentials using Riot's official security tools.

## Scope

The app interacts with undocumented Riot Client session files. It is not an official authentication integration and cannot guarantee compatibility after Riot updates.
