using Artisan.Tasks;

var passed = 0;
void Check(string name, Action test)
{
    test(); passed++; Console.WriteLine($"PASS {name}");
}
void Expect(MarketboardSearchStep actual, MarketboardSearchStep expected)
{
    if (actual != expected) throw new Exception($"Expected {expected}, received {actual}");
}
void True(bool value) { if (!value) throw new Exception("Assertion failed"); }
MarketboardSearchRequest Start() { var r = new MarketboardSearchRequest(); r.Start(0); return r; }
Check("Idle request takes no action", () => Expect(new MarketboardSearchRequest().Next(0, true, false, true, true), MarketboardSearchStep.Idle));
Check("Open marketboard searches once without retargeting", () =>
{
    var r = Start(); Expect(r.Next(1, true, true, false, false), MarketboardSearchStep.Search);
    Expect(r.Next(2, true, true, false, false), MarketboardSearchStep.Idle);
});
Check("Select before interacting with a nearby board", () =>
{
    var r = Start(); Expect(r.Next(1, true, false, true, true), MarketboardSearchStep.SelectBoard);
    Expect(r.Next(2, true, false, true, true), MarketboardSearchStep.InteractWithBoard);
});
Check("Board interaction is throttled and retried", () =>
{
    var r = Start(); r.Next(1, true, false, true, true); r.Next(2, true, false, true, true);
    Expect(r.Next(2501, true, false, true, true), MarketboardSearchStep.Wait);
    Expect(r.Next(2502, true, false, true, true), MarketboardSearchStep.SelectBoard);
    Expect(r.Next(2503, true, false, true, true), MarketboardSearchStep.InteractWithBoard);
});
Check("Opening only succeeds after the search window is ready", () =>
{
    var r = Start(); r.Next(1, true, false, true, true); r.Next(2, true, false, true, true);
    True(r.Pending); Expect(r.Next(3, true, true, false, false), MarketboardSearchStep.Search); True(!r.Pending);
});
Check("Opening times out and cannot search afterward", () =>
{
    var r = Start(); Expect(r.Next(10000, true, false, false, false), MarketboardSearchStep.TimedOut);
    Expect(r.Next(10001, true, true, false, false), MarketboardSearchStep.Idle);
});
Check("Context change cancels before search even if a window is ready", () =>
{
    var r = Start(); Expect(r.Next(1, false, true, true, true), MarketboardSearchStep.Cancelled); True(!r.Pending);
});
Check("Unrelated occupied interactions are not interrupted", () =>
{
    var r = Start(); Expect(r.Next(1, true, false, true, false), MarketboardSearchStep.Wait);
});
Check("A disappearing board waits rather than issuing an interaction", () =>
{
    var r = Start(); r.Next(1, true, false, true, true);
    Expect(r.Next(2, true, false, false, true), MarketboardSearchStep.Wait);
});
Check("Plugin disposal cancels a pending request", () =>
{
    var r = Start(); r.Cancel(); Expect(r.Next(1, true, true, true, true), MarketboardSearchStep.Idle);
});
Check("Localized board names ignore display capitalization", () =>
{
    True(MarketboardObject.Matches(0, "Market Board", "market board"));
    True(MarketboardObject.Matches(0, "TABLEAU DES VENTES", "tableau des ventes"));
});
Check("Known board object IDs work independently of language", () =>
{
    foreach (var id in new uint[] { 2000073, 2000402, 2000440, 2000442, 2010285 })
        True(MarketboardObject.Matches(id, "", ""));
});
Check("Non-marketboard objects are rejected", () =>
{
    True(!MarketboardObject.Matches(2000401, "Summoning Bell", "market board"));
    True(!MarketboardObject.Matches(0, "", ""));
});
Console.WriteLine($"{passed} marketboard search checks passed.");
