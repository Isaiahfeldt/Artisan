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

## Opt-in ingredient marketboard keyboard search

Version **4.0.5.25** restores **Search in game marketboard** and Ctrl+left-click using a different submission route. Following the crash report on the previous implementation, this mode is **off by default**. Right-click an ingredient and check **Enable keyboard market search (experimental)** to enable it.

The workflow opens a nearby board or uses an already-open one, fills only the text component through its normal API, focuses that component's collision node, waits for shortcut modifiers to be released, and sends Enter to FFXIV's own window using ECommons' keyboard helper. It does not call a custom search signature, write the addon's raw string fields or agent search state, or synthesize a Search-button GUI event.

Before Enter, it requires the game to be in the foreground, the market text field to retain focus, and the marketboard to be the only focused native addon. Loss of focus, dialogs, combat, zone changes, or timeout cancels the request. If the focus checks fail, the filled name is left in place and you can press Enter manually. Existing Shift-click/name-copy behavior is preserved.

First test with the actual marketboard **already open**: opt in through an ingredient's right-click menu, release modifiers after Ctrl+left-clicking its name, and confirm that normal search results appear. Then test opening a nearby board. This implementation is build-checked and has automated request/shortcut checks, but in-game behavior and stability have not been validated in the cloud.

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
