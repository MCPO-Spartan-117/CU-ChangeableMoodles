# Template BepInEx Plugin

A small BepInEx plugin template for Unity modding Casualties Unknown with CUCoreLib

## Overview 
- Includes `CUCoreLib` references/usings, Harmony, Newtonsoft.Json, and Unity assembly references
- Includes a `ContentReloadManager.EnableHotReload(...)` starter pattern
- Automatically embeds `.png`, `.jpg`, and `.jpeg` files placed under `images/` or `assets/`
- Expands common Steam install paths on Windows and Linux

## Build

1. Open the project in Visual Studio or JetBrains Rider. (Or any other IDE)
2. Build `ScavTemplate/Template.csproj` via Ctrl + Shift + B (`dotnet build`)
3. If auto-detection misses your setup, open the linked `vars.targets` file from the project and override `BaseGamePath` 

## Usage

1. Update `Plugin.cs` with your plugin GUID, name, version.
2. Update the namespace in `Plugin.cs` and `Patches.cs`.
3. Put startup-only funntions before `ContentReloadManager.EnableHotReload(ModGUID)`.
4. Put reloadable item/recipe/content registration inside `RegisterReloadable()` / `DoStuff()`.
5. Place art files under `images/` or `assets/` to auto-embed them, or set `Build Action = Embedded Resource` manually elsewhere in the project.
6. Run the game with BepInEx and `CUCoreLib` installed in the plugins folder! ^^

## Hot Reload Notes

- Use `reloadcontent <modGuid>` in the in-game console to replay supported registrations.
- Use `autohotreload <modGuid> true` to enable auto reload.
- If Windows locks the deployed plugin DLL, set `[Hot Reload] -> <modGuid>.overridePath` in `CUCoreLib.cfg` to your rebuilt `bin\Debug` DLL.
