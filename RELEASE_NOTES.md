# Riot Account Switcher v0.7.0 — public beta

> **Unapproved experimental beta — use at your own risk.** This project is not affiliated with or endorsed by Riot Games. It restores local Riot Client session files using an undocumented mechanism; Riot has not confirmed that this use is permitted. Client updates may break it, and use of unauthorized third-party software may carry account risks. External game imagery belongs to its owners.

## Changes from v0.6.6

- Replaced the bundled Riot / VALORANT / LoL / TFT marks with original geometric icons.
- Introduced an original minimal monochrome application icon and shipped editable SVG masters.
- Deleted `OpggIconService` and the old `DownloadIconWindow`.
- Added an optional avatar gallery: offline original choices, LoL/TFT Data Dragon icons, and VALORANT player cards via unofficial Valorant-API.
- Catalog access only happens when a user explicitly selects a provider. No Riot IDs, session files, login credentials or account names are sent to either provider.
- Personal Riot IDs are now an optional *display label only*; no server selection needed for image catalogs.
- Previously saved local avatars remain available. The session encryption, recovery, theme, language, local storage, and Steam-style chooser were not rewritten.

## Testing recommendations

- Compile v0.7.0 on Windows 11 using `COMPILAR-LIGERO.cmd`.
- Verify offline icons and local PNG/JPEG import.
- Test both optional online catalogs, including page navigation and search, if permitted by relevant service policies.
- Verify new account capture, restoring an existing saved session, recovery, settings, and the portable publish script.
- Keep the old executable as a fallback while testing. Never delete `%LOCALAPPDATA%\RiotSwitcherAccount`.

The project creator has reported successful Windows testing of v0.7.0. The packaged portable executable should be independently tested before attaching it to this release.
