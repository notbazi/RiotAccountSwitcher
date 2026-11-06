# Riot Account Switcher

**Unofficial account selector for Riot Client on Windows**, by **notbazi**. Built with C#, WPF and .NET 10.

[Leer en español](docs/README.es.md)

> **Beta / private testing:** not endorsed by Riot Games or any catalog provider. The original UI and glyphs can be distributed under this project's license; external game imagery is controlled by its respective rights holders. The session switching mechanism uses private client files and has not been approved by Riot. See [third-party notices](THIRD_PARTY_NOTICES.md) before any public release.

## Features

- Compact, Steam-inspired account picker with local nicknames and optional display-only Riot IDs.
- Save/restore locally encrypted Riot Client session snapshots (Windows DPAPI).
- English and Spanish UI; customizable accent colors.
- **Original, offline game-category icons** and an original application icon.
- **Optional avatar galleries:** LoL/TFT profile icons from Riot's Data Dragon and VALORANT player cards from the community Valorant-API game-art catalog. Network access is user-initiated; **no account lookups and no OP.GG scraping**.
- Personal PNG/JPEG imports (max. 1 MB and 2048 × 2048 pixels) and right-click avatar editing.
- Optional standalone portable Windows x64 build.

## Use

1. Open the app, click **+**, and enter an account nickname.
2. Sign in using the **official Riot Client**, enable *Stay signed in*, and fully exit Riot Client including its tray icon.
3. Choose the game and optionally add your Riot ID **only as a local display label**. The ID is not sent to an avatar provider.
4. Optionally select an offline avatar or explicitly open an online gallery; click **Save current session**.
5. Choose a saved profile on the main screen to restore its saved local session and open Riot Client.
6. Right-click an existing profile to change its icon, import an image, rename it or remove that local entry.

The app does **not** request your password, access two-factor codes or guarantee a previously saved session is still valid. Riot Client updates may break restoration because it uses undocumented configuration files.

## Build (Windows 10/11 x64)

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), then run:

- `COMPILAR-LIGERO.cmd`: creates `dist/ligero/RiotSwitcherAccount.exe` (needs .NET 10 Desktop Runtime at the destination).
- `COMPILAR-PORTABLE.cmd`: creates `dist/portable/RiotSwitcherAccount.exe` (self-contained).

Or from PowerShell in the repository root:

```powershell
dotnet publish .\src\RiotSwitcherAccount.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o .\dist\portable
```

The binary is unsigned; Windows may show an unverified-publisher warning. Don't disable Defender or SmartScreen to work around it.

## Privacy and networking

- All account/session data is **outside the executable** in `%LOCALAPPDATA%\RiotSwitcherAccount`.
- `profiles.json` includes display names and optional Riot IDs in **plain text**; encrypted `*.session.bin` and `last-recovery.bin` are sensitive, even though protected with Windows DPAPI.
- Never publish that folder, Riot Client private YAML files, debug logs or any session file. An attacker running as your Windows user may still access DPAPI-protected data.
- Avatar catalog browsing (only after the corresponding button is clicked) connects to `ddragon.leagueoflegends.com` or `valorant-api.com` / `media.valorant-api.com`; it **does not send stored account names, Riot IDs, passwords or tokens**.
- A user-chosen image is downloaded on demand, resized and cached locally. Remote images are not embedded into the release package.
- Existing avatars previously saved by older versions are preserved locally when updating.

## Source code

| Component | Path |
| --- | --- |
| Main profile chooser | `src/MainWindow.*` |
| Create account | `src/AddAccountWindow.*` |
| Avatar browser | `src/AvatarGalleryWindow.*` |
| Online catalogs | `src/Services/AvatarCatalogService.cs` |
| Local user files / session switching | `src/Services/` and `src/Security/` |
| Original symbols | `src/Assets/*.svg` |
| Translations | `src/UI/UiStrings.cs` |

Original source code and symbols are provided under the [MIT license](LICENSE); **the MIT license does not cover Riot game artwork retrieved via third-party catalogs**, nor does it guarantee permission to distribute a client session switcher.

**Public release is on hold pending a review of Riot's developer policies and clearance for the specific account-switching behavior.** See [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md), [SECURITY.md](SECURITY.md), and [PUBLISH_CHECKLIST.md](PUBLISH_CHECKLIST.md).
