using Artisan.Tasks;

var checks = new (bool Ctrl, bool Shift, bool Alt, IngredientClickAction Expected)[]
{
    (true, false, false, IngredientClickAction.MarketboardSearch),
    (false, true, false, IngredientClickAction.CopyName),
    (true, true, false, IngredientClickAction.CopyName),
    (false, false, false, IngredientClickAction.CopyName),
    (true, false, true, IngredientClickAction.CopyName),
};
foreach (var c in checks)
{
    if (IngredientClick.Resolve(c.Ctrl, c.Shift, c.Alt) != c.Expected)
        throw new Exception("Unexpected shortcut mapping");
}
Console.WriteLine($"{checks.Length} shortcut checks passed. Marketboard action is disabled in the plugin.");
