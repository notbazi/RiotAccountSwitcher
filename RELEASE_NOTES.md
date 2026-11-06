# Riot Account Switcher v0.7.0 — private beta candidate

> **Do not publish publicly yet.** Third-party game-image conditions and the Riot Client session switching mechanism require review.

## Changes from v0.6.6

- Replaced the bundled Riot / VALORANT / LoL / TFT marks with original geometric icons.
- Introduced an original minimal monochrome application icon and shipped editable SVG masters.
- Deleted `OpggIconService` and the old `DownloadIconWindow`.
- Added an optional avatar gallery: offline original choices, LoL/TFT Data Dragon icons, and VALORANT player cards via unofficial Valorant-API.
- Catalog access only happens when a user explicitly selects a provider. No Riot IDs, session files, login credentials or account names are sent to either provider.
- Personal Riot IDs are now an optional *display label only*; no server selection needed for image catalogs.
- Previously saved local avatars remain available. The session encryption, recovery, theme, language, local storage, and Steam-style chooser were not rewritten.

## Testing required

- Compile v0.7.0 on Windows 11 using `COMPILAR-LIGERO.cmd`.
- Verify offline icons and local PNG/JPEG import.
- Test both optional online catalogs, including page navigation and search, if permitted by relevant service policies.
- Verify new account capture, restoring an existing saved session, recovery, settings, and the portable publish script.
- Keep the old executable as a fallback while testing. Never delete `%LOCALAPPDATA%\RiotSwitcherAccount`.

This source package was checked structurally but has **not** been built or run as WPF on Windows in the authoring environment.
