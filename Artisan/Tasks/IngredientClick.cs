namespace Artisan.Tasks;

internal enum IngredientClickAction { CopyName, MarketboardSearch }

internal static class IngredientClick
{
    // Shift retains the existing name-copy behavior, including Ctrl+Shift clicks.
    internal static IngredientClickAction Resolve(bool control, bool shift, bool alt) =>
        control && !shift && !alt ? IngredientClickAction.MarketboardSearch : IngredientClickAction.CopyName;
}
