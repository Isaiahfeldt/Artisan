<!-- Repository Header Begin -->
<div align="center">
<img src="https://github.com/PunishXIV/Artisan/blob/main/PunishImages/artisan-icon.png?raw=true" alt="Artisan IconUrl" width="15%">
<br>
<img src="https://github.com/PunishXIV/Artisan/blob/050a58be7b0ce94c959c17e43dabecb65e38a55c/PunishImages/artisan.png" width="30%" />

Crafting Automation Plugin/Helper for FFXIV

[![image](https://discordapp.com/api/guilds/1001823907193552978/embed.png?style=banner2)](https://discord.gg/Zzrcc8kmvy)

Repo Url: 

`https://love.puni.sh/ment.json`
</div>

## Ingredient marketboard shortcut

Version **4.0.5.26** removes the toggle. **Ctrl+left-click** an ingredient name, or right-click it and choose **Search in game marketboard**, to open a nearby marketboard (or use an already-open one) and fill the ingredient name. **Press Enter or click Search yourself.** There is no automatic keypress, custom Search-button event dispatch, or direct search-state write.

Shift-click and Ctrl+Shift-click retain name copying. When the shortcut is unavailable, hover the context-menu entry for the reason. Build and 16 request/shortcut checks pass; the simpler open-and-fill behavior follows the portion previously confirmed working in-game.

Custom repository URL:

```text
https://raw.githubusercontent.com/Isaiahfeldt/ffxiv/main/pluginmaster.json
```

Build with .NET 10, initialized C# submodules, and stable Dalamud assemblies:

```sh
dotnet restore Artisan/Artisan.csproj -p:Configuration=Release -p:Platform=x64 -p:EnableWindowsTargeting=true -p:DalamudLibPath="$DALAMUD_HOME/"
dotnet build Artisan/Artisan.csproj --no-restore -c Release -m:1 -p:Platform=x64 -p:EnableWindowsTargeting=true -p:DalamudLibPath="$DALAMUD_HOME/"
dotnet run --project Artisan.Checks
```
