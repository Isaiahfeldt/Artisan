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

## Ingredient marketboard shortcut temporarily disabled

Version **4.0.5.24** disables the fork's new **Search in game marketboard** and Ctrl+left-click marketboard actions following a reported FFXIV crash. Update from the central repository before using this fork again. Existing Artisan functionality, the original Market Board Lookup integration, and name copying remain available.

The entire custom native submission path and its framework callback have been removed. The new context-menu entry is disabled with an explanation; Ctrl-click only displays that explanation. The feature will remain disabled pending in-game validation and crash investigation. Request-flow tests cannot validate native UI memory safety.

Custom repository URL:

```text
https://raw.githubusercontent.com/Isaiahfeldt/ffxiv/main/pluginmaster.json
```

Build with .NET 10 and initialized C# submodules, using stable Dalamud assemblies:

```sh
dotnet restore Artisan/Artisan.csproj -p:Configuration=Release -p:Platform=x64 -p:EnableWindowsTargeting=true -p:DalamudLibPath="$DALAMUD_HOME/"
dotnet build Artisan/Artisan.csproj --no-restore -c Release -m:1 -p:Platform=x64 -p:EnableWindowsTargeting=true -p:DalamudLibPath="$DALAMUD_HOME/"
dotnet run --project Artisan.Checks
```
