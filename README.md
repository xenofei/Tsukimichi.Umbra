# Tsukimichi for Umbra

Umbra toolbar widgets for [Tsukimichi](https://github.com/xenofei/Tsukimichi), the FFXIV quest tracker. They show tonight at a glance in [Umbra](https://github.com/una-xiv/umbra)'s toolbar, drawn with Umbra's own controls and colours, so they look like the rest of your bar in every Umbra theme.

It is a separate add-on: an Umbra custom plugin, not a Dalamud plugin. **It needs Tsukimichi 1.22 or later**, installed and running in Dalamud. Tsukimichi works the same without it.

## The widgets

| Widget | On the bar | Click |
|---|---|---|
| **Tsukimichi** | your Tsukimichi theme's moon and "12 Ready" | opens the Tsukimichi popup |
| **Tsukimichi: Up next** | the quest's marker and name; its step on hover | your choice |
| **Tsukimichi: Journal** | "Journal 27/30"; the slots left on hover | your choice |
| **Tsukimichi: Story meter** | the expansion over "91 to the latest story" | your choice |
| **Tsukimichi: Next reset** | "Next reset" over "daily in 3 h 12 m" | your choice |

**The popup** has Up next with **Go**, tonight's first three Ready quests with their place (then "9 more"), your journal's room with **Make room**, the events ending soon, and Open Tsukimichi, Route and Settings. A Ready quest opens in Tsukimichi when clicked.

- **Go opens Tsukimichi on Up next. It never starts travel or a run.** Anything that moves your character still needs your click inside Tsukimichi.
- Quest names come from Tsukimichi with its spoiler shield already applied, so the add-on never shows a name Tsukimichi hides.

**Settings.** Each small widget has Umbra's usual options: icon, text, colour, and a click action (open Tsukimichi, open Tonight, show the Tsukimichi popup, or nothing). Next reset needs no Tsukimichi; the reset times are the game's (daily 15:00 UTC, weekly Tuesday 08:00 UTC).

**When Tsukimichi isn't there.** If Tsukimichi isn't running, or is older than 1.22, the bar reads "Tsukimichi" with no count, and the popup says why in one line with a button that opens Dalamud's plugin installer.

## Install

1. In Umbra, open **Settings › Plugins** and turn on **custom plugins**. Umbra asks you to agree first.
2. Add the repository **`xenofei/Tsukimichi.Umbra`**.
3. Add the widgets from Umbra's widget list: search for "Tsukimichi".

Umbra checks this repository's latest GitHub release for updates. Each release is built for the Umbra of its day, and its notes name that Umbra version.

## Privacy

The add-on talks only to Tsukimichi, inside the game, through Dalamud's plugin IPC. It makes no network requests and stores nothing. Umbra itself downloads the add-on from GitHub when you add or update it.

## Building

You need the .NET 10 SDK, Umbra installed through XIVLauncher, and Dalamud's files.

```
dotnet build -c Release -warnaserror
dotnet test -c Release --no-build
```

The project finds Umbra in `%AppData%\XIVLauncher\installedPlugins\Umbra\<version>\` and Dalamud in `%AppData%\XIVLauncher\addon\Hooks\dev\`. Set `UMBRA_HOME` (an unpacked [umbra-dist](https://github.com/una-xiv/umbra-dist) `latest.zip`) or `DALAMUD_HOME` to build against other copies; the release workflow does this. The output is one file, `src/Tsukimichi.Umbra/bin/Release/Tsukimichi.Umbra.dll`. To try it, add that file in Umbra's Plugins settings as a local plugin; Umbra reloads it when it changes.

- `src/Tsukimichi.Umbra.Core`: the pure parts (the version check, every word the widgets show, the reset clock), compiled into the add-on and tested on their own.
- `src/Tsukimichi.Umbra`: the Umbra widgets, the popup and the link to Tsukimichi (`TsukimichiLink`).
- `assets/moons`: the theme moons, made by `tools/make-moons.ps1` (node and resvg).

### Releasing

Set `<Version>` in `src/Tsukimichi.Umbra/Tsukimichi.Umbra.csproj`, commit, and push a tag `vX.Y.Z` with the same version. The release workflow builds against the latest Umbra and Dalamud, runs the tests, and publishes `Tsukimichi.Umbra.zip` as the release named `X.Y.Z`.

Umbra refuses a plugin built against an older Umbra minor version than the one running (3.1 on 3.2, for example). When Umbra moves to a new minor version, bump the patch version and release again.

## Licence

[AGPL-3.0](LICENSE), because the add-on builds on Umbra, which is AGPL-3.0. Tsukimichi itself is MIT and is a separate program; this add-on only talks to it over IPC.
