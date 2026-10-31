using ECommons.DalamudServices;
using Item = Lumina.Excel.Sheets.Item;

namespace Artisan.Tasks;

// Keep both UI entry points disabled until native interaction can be validated
// in-game. Do not scan signatures, access game UI pointers, or subscribe updates.
internal static class IngredientMarketboardSearch
{
    internal const string DisabledReason = "Temporarily disabled while investigating a reported game crash.";

    internal static string UnavailableReason(Item item) => DisabledReason;

    internal static void Start(Item item) => Svc.Chat.PrintError($"[Artisan] {DisabledReason}");
}
