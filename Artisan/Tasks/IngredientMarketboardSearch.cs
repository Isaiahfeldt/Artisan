using System;
using System.Linq;
using System.Numerics;
using Artisan.CraftingLists;
using Artisan.IPC;
using Dalamud.Game.ClientState.Conditions;
using ECommons;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using EObjName = Lumina.Excel.Sheets.EObjName;
using Item = Lumina.Excel.Sheets.Item;
using IGameObject = Dalamud.Game.ClientState.Objects.Types.IGameObject;
using ObjectKind = Dalamud.Game.ClientState.Objects.Enums.ObjectKind;

namespace Artisan.Tasks;

internal static unsafe class IngredientMarketboardSearch
{
    private static readonly MarketboardSearchRequest Request = new();
    private static string searchName = string.Empty;
    private static uint territory;
    private static ulong character;
    private static bool initialized;

    internal static void Init()
    {
        if (initialized) return;
        initialized = true;
        Svc.Framework.Update += Update;
    }

    internal static void Dispose()
    {
        Request.Cancel();
        if (initialized) Svc.Framework.Update -= Update;
        initialized = false;
    }

    internal static string? UnavailableReason(Item item)
    {
        if (item.RowId == 0 || item.IsUntradable || item.ItemSearchCategory.RowId == 0)
            return "This item cannot be sold on the marketboard.";
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

    private static bool ClickSearchButton(AtkUnitBase* addon)
    {
        // Resolve the real button by its localized market-search label, rather than
        // assuming a node ID or bypassing the UI's input/submit event handling.
        var label = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Addon>().GetRow(1942).Text.GetText().Trim();
        for (var index = 0; index < addon->UldManager.NodeListCount; index++)
        {
            var node = addon->UldManager.NodeList[index];
            if (node == null || (int)node->Type < 1000 || !node->IsVisible()) continue;
            var component = ((AtkComponentNode*)node)->Component;
            if (component == null || component->GetComponentType() != ComponentType.Button) continue;
            var button = (AtkComponentButton*)component;
            if (button->ButtonTextNode == null || !button->IsEnabled
                || !button->ButtonTextNode->NodeText.GetText().Trim().Equals(label, StringComparison.OrdinalIgnoreCase)) continue;
            // Dispatch the button's own registered click event and parameter.
            AtkEvent* click = null;
            for (var evt = (AtkEvent*)node->AtkEventManager.Event; evt != null; evt = evt->NextEvent)
            {
                if (evt->State.EventType == AtkEventType.ButtonClick) { click = evt; break; }
                if (click == null && evt->State.EventType == AtkEventType.MouseClick) click = evt;
            }
            if (click != null)
            {
                addon->ReceiveEvent(click->State.EventType, (int)click->Param, click);
                return true;
            }
        }
        return false; // Wait for the button to finish loading/enabling, until timeout.
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
                case MarketboardSearchStep.FillSearch:
                    if (GenericHelpers.TryGetAddonByName<AddonItemSearch>("ItemSearch", out var addon))
                    {
                        addon->SearchTextInput->SetText(searchName);
                        addon->SearchText.SetString(searchName);
                        addon->SearchText2.SetString(searchName);
                        var agent = AgentItemSearch.Instance();
                        if (agent != null && agent->StringData != null)
                            agent->StringData->SearchParam.SetString(searchName);
                    }
                    break;
                case MarketboardSearchStep.SubmitSearch:
                    if (GenericHelpers.TryGetAddonByName<AddonItemSearch>("ItemSearch", out var searchAddon)
                        && ClickSearchButton(&searchAddon->AtkUnitBase))
                        Request.SearchSubmitted();
                    break;
                case MarketboardSearchStep.Cancelled:
                    Svc.Chat.PrintError("[Artisan] Marketboard search cancelled because the character or game interaction changed.");
                    break;
                case MarketboardSearchStep.TimedOut:
                    Svc.Chat.PrintError("[Artisan] Marketboard search could not be submitted in time. Open the board manually and try again.");
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
