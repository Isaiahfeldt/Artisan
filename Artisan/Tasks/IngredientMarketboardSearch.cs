using System;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Artisan.CraftingLists;
using Artisan.IPC;
using Dalamud.Game.ClientState.Conditions;
using ECommons;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using EObjName = Lumina.Excel.Sheets.EObjName;
using Item = Lumina.Excel.Sheets.Item;
using IGameObject = Dalamud.Game.ClientState.Objects.Types.IGameObject;
using ObjectKind = Dalamud.Game.ClientState.Objects.Enums.ObjectKind;

namespace Artisan.Tasks;

internal static unsafe class IngredientMarketboardSearch
{
    // Upstream FFXIVClientStructs AddonItemSearch.RunSearch(bool ignoreFilters).
    // https://github.com/aers/FFXIVClientStructs/blob/main/FFXIVClientStructs/FFXIV/Client/UI/AddonItemSearch.cs
    // Bound locally because Artisan's current structs do not expose this member yet.
    private const string SearchSignature = "E8 ?? ?? ?? ?? 48 8D AC 24";
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void RunSearchDelegate(AddonItemSearch* addon, [MarshalAs(UnmanagedType.I1)] bool ignoreFilters);

    private static RunSearchDelegate? runSearch;
    private static readonly MarketboardSearchRequest Request = new();
    private static string searchName = string.Empty;
    private static uint territory;
    private static ulong character;
    private static bool initialized;

    internal static void Init()
    {
        if (initialized) return;
        initialized = true;
        try
        {
            if (Svc.SigScanner.TryScanText(SearchSignature, out var address))
                runSearch = Marshal.GetDelegateForFunctionPointer<RunSearchDelegate>(address);
            else
                Svc.Log.Warning("Ingredient marketboard search is unavailable: native search signature not found.");
        }
        catch (Exception ex) { Svc.Log.Error(ex, "Could not initialize ingredient marketboard search"); }
        Svc.Framework.Update += Update;
    }

    internal static void Dispose()
    {
        Request.Cancel();
        if (initialized) Svc.Framework.Update -= Update;
        initialized = false;
        runSearch = null;
    }

    internal static string? UnavailableReason(Item item)
    {
        if (item.RowId == 0 || item.IsUntradable || item.ItemSearchCategory.RowId == 0)
            return "This item cannot be sold on the marketboard.";
        if (runSearch == null) return "In-game market search is unavailable for this game version.";
        if (Request.Pending) return "An in-game marketboard search is already opening.";
        var player = Svc.Objects.LocalPlayer;
        if (player == null || player.CurrentHp == 0 || Svc.ClientState.IsPvP
            || Svc.Condition[ConditionFlag.InCombat] || Svc.Condition[ConditionFlag.BetweenAreas]
            || Svc.Condition[ConditionFlag.BetweenAreas51] || Svc.Condition[ConditionFlag.Crafting]
            || Svc.Condition[ConditionFlag.WatchingCutscene] || Svc.Condition[ConditionFlag.WatchingCutscene78])
            return "Wait until your character is available and outside combat/crafting.";
        if (P.TM.IsBusy || P.CTM.IsBusy || RetainerInfo.TM.IsBusy || CraftingListUI.Processing)
            return "Finish or stop Artisan's current operation first.";
        if (Visible("SelectYesno") || Visible("ItemSearchResult"))
            return "Close the current listing or confirmation window first.";
        if (SearchReady()) return null;
        if (GenericHelpers.IsOccupied()) return "Finish the current game interaction first.";
        return FindBoard() != null ? null : "Open the marketboard or stand within reach of one.";
    }

    internal static void Start(Item item)
    {
        var reason = UnavailableReason(item);
        if (reason != null) { Svc.Chat.PrintError($"[Artisan] {reason}"); return; }
        searchName = item.Name.GetText().Trim();
        if (searchName.Length == 0) return;
        territory = Svc.ClientState.TerritoryType;
        character = Svc.PlayerState.ContentId;
        Request.Start(Environment.TickCount64);
    }

    private static bool Visible(string name) =>
        GenericHelpers.TryGetAddonByName<AtkUnitBase>(name, out var addon) && addon->IsVisible;

    private static bool SearchReady() =>
        GenericHelpers.TryGetAddonByName<AddonItemSearch>("ItemSearch", out var addon)
        && addon->AtkUnitBase.IsVisible && GenericHelpers.IsAddonReady(&addon->AtkUnitBase)
        && addon->SearchTextInput != null;

    private static IGameObject? FindBoard()
    {
        var player = Svc.Objects.LocalPlayer;
        if (player == null) return null;
        var name = Svc.Data.GetExcelSheet<EObjName>().GetRow(2000442).Singular.GetText();
        return Svc.Objects.Where(obj => obj.IsTargetable
            && (obj.ObjectKind == ObjectKind.EventObj || obj.ObjectKind == ObjectKind.HousingEventObject)
            && MarketboardObject.Matches(obj.BaseId, obj.Name.TextValue, name)
            && Vector3.Distance(player.Position, obj.Position) <= 4.6f)
            .OrderBy(obj => Vector3.DistanceSquared(player.Position, obj.Position)).FirstOrDefault();
    }

    private static void Update(IFramework framework)
    {
        if (!Request.Pending) return;
        try
        {
            var valid = Svc.Objects.LocalPlayer is { CurrentHp: > 0 } && character == Svc.PlayerState.ContentId
                && territory == Svc.ClientState.TerritoryType && !Svc.Condition[ConditionFlag.InCombat]
                && !Svc.Condition[ConditionFlag.BetweenAreas] && !Svc.Condition[ConditionFlag.BetweenAreas51]
                && !Svc.Condition[ConditionFlag.Crafting] && !Svc.Condition[ConditionFlag.WatchingCutscene]
                && !Svc.Condition[ConditionFlag.WatchingCutscene78] && !Visible("SelectYesno") && !Visible("ItemSearchResult")
                && !P.TM.IsBusy && !P.CTM.IsBusy && !RetainerInfo.TM.IsBusy && !CraftingListUI.Processing;
            var ready = SearchReady();
            var board = ready ? null : FindBoard();
            switch (Request.Next(Environment.TickCount64, valid, ready, board != null, !GenericHelpers.IsOccupied()))
            {
                case MarketboardSearchStep.SelectBoard:
                    Svc.Targets.Target = board;
                    break;
                case MarketboardSearchStep.InteractWithBoard:
                    if (board != null)
                    {
                        if (Svc.Targets.Target?.GameObjectId != board.GameObjectId) Svc.Targets.Target = board;
                        else TargetSystem.Instance()->InteractWithObject((GameObject*)board.Address, false);
                    }
                    break;
                case MarketboardSearchStep.Search:
                    if (runSearch != null && GenericHelpers.TryGetAddonByName<AddonItemSearch>("ItemSearch", out var addon))
                    {
                        addon->SearchTextInput->SetText(searchName);
                        addon->SearchText.SetString(searchName);
                        addon->SearchText2.SetString(searchName);
                        runSearch(addon, true); // Native search, ignoring stale category filters.
                    }
                    break;
                case MarketboardSearchStep.Cancelled:
                    Svc.Chat.PrintError("[Artisan] Marketboard search cancelled because the character or game interaction changed.");
                    break;
                case MarketboardSearchStep.TimedOut:
                    Svc.Chat.PrintError("[Artisan] Marketboard did not open in time. Move closer or open it manually, then try again.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Request.Cancel();
            Svc.Log.Error(ex, "Ingredient marketboard search failed");
            Svc.Chat.PrintError("[Artisan] Could not search the in-game marketboard. See the plugin log.");
        }
    }
}
