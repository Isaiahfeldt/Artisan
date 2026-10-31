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

## Fork feature: ingredient search in the game marketboard

This fork adds **Search in game marketboard** to the List Editor's Ingredients-panel right-click menu. It is separate from **Market Board Lookup**, which still uses the MarketBoard plugin's `/pmb` command.

Open the actual marketboard first, or stand within interaction range of a board. Right-click an ingredient and choose **Search in game marketboard**. Artisan selects and opens a nearby board when needed, waits for the game search window, fills the ingredient's localized name, then activates the actual Search button after the input has updated. Choose the item/listing and purchase through the normal game UI.

The option is disabled with a tooltip for unmarketable items, when no board is reachable/open, while Artisan is busy crafting/restocking, or while an existing listing or confirmation dialog needs to be closed. Pending opening requests time out after 10 seconds and cancel on logout, territory change, combat, or conflicting activity. There is no walking, teleporting, or automatic purchasing.

The search uses the actual localized Search button and its registered click event rather than a separate native search signature. The request stays pending while the button loads/enables and times out rather than reporting success merely because text was filled. In-game verification of search submission is still needed.

Install the fork from this central Dalamud repository (replace the original Artisan installation; do not load two copies):

```text
https://raw.githubusercontent.com/Isaiahfeldt/ffxiv/main/pluginmaster.json
```

Build with .NET 10, initialize the PunishLib, OtterGui, KamiToolKit, and AutoRetainerAPI submodules, and point `DalamudLibPath` at stable Dalamud assemblies. For Linux build validation:

```sh
dotnet restore Artisan/Artisan.csproj -p:Configuration=Release -p:Platform=x64 -p:EnableWindowsTargeting=true -p:DalamudLibPath="$DALAMUD_HOME/"
dotnet build Artisan/Artisan.csproj --no-restore -c Release -m:1 -p:Platform=x64 -p:EnableWindowsTargeting=true -p:DalamudLibPath="$DALAMUD_HOME/"
dotnet run --project Artisan.Checks
```

The existing tracked `raphael-cli.bin` supplies the solver for this package; the Rust submodule need not be rebuilt for this UI change.

Version 4.0.5.23 adds **Ctrl+left-click** on ingredient names as a shortcut for the same action. Shift+left-click and Ctrl+Shift+left-click retain copying; ordinary clicks retain their previous behavior. Both the shortcut and the right-click option share the same search workflow.
