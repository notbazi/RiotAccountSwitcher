# Publication checklist

- [x] Remove OP.GG scraping and all code paths contacting OP.GG.
- [x] Remove bundled Riot, LoL, VALORANT and TFT marks; replace with original graphic assets.
- [x] Keep custom local image imports and local-only profile/session storage.
- [ ] **Confirm permission / policy compliance of Riot Client session-file switching before public distribution.**
- [ ] Confirm catalog API and game-art use fits Riot policies; keep external artwork out of repository and binaries.
- [ ] Review third-party naming, original icon artwork, and trademark attribution.
- [ ] Build and test exact v0.7.0 commit on Windows 11 x64 (both lightweight and portable).
- [ ] Test on a separate computer without transferring profile or session files.
- [ ] Verify no real account IDs, credentials, tokens, AppData, `*.session.bin`, or Riot private YAML files are included in any branch, tag, or release.
- [ ] Run Windows Defender, inspect dependencies and compute SHA-256 (`Get-FileHash .\RiotSwitcherAccount.exe -Algorithm SHA256`).
- [ ] Review source history and update docs / disclaimers before switching repo to Public.
- [ ] Publish an unsigned beta ZIP in GitHub Releases with a transparent compatibility disclaimer if cleared for distribution.
