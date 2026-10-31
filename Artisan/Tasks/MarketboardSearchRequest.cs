using System;

namespace Artisan.Tasks;

internal enum MarketboardSearchStep { Idle, Wait, SelectBoard, InteractWithBoard, Search, Cancelled, TimedOut }

internal sealed class MarketboardSearchRequest
{
    internal bool Pending { get; private set; }
    private bool selected;
    private long deadline;
    private long nextInteraction;

    internal void Start(long now)
    {
        Pending = true;
        selected = false;
        deadline = now + 10000;
        nextInteraction = now;
    }

    internal void Cancel() => Pending = false;

    internal MarketboardSearchStep Next(long now, bool contextValid, bool searchReady, bool boardAvailable, bool canInteract)
    {
        if (!Pending) return MarketboardSearchStep.Idle;
        if (!contextValid) { Pending = false; return MarketboardSearchStep.Cancelled; }
        if (now >= deadline) { Pending = false; return MarketboardSearchStep.TimedOut; }
        if (searchReady) { Pending = false; return MarketboardSearchStep.Search; }
        if (!boardAvailable || !canInteract || now < nextInteraction) return MarketboardSearchStep.Wait;
        if (!selected) { selected = true; return MarketboardSearchStep.SelectBoard; }
        selected = false;
        nextInteraction = now + 2500;
        return MarketboardSearchStep.InteractWithBoard;
    }
}

internal static class MarketboardObject
{
    // EObjName rows with the English noun "market board" in game data.
    internal static bool Matches(uint baseId, string name, string localizedName) =>
        baseId is 2000073 or 2000402 or 2000440 or 2000442 or 2010285
        || (!string.IsNullOrWhiteSpace(localizedName)
            && name.Trim().Equals(localizedName.Trim(), StringComparison.OrdinalIgnoreCase));
}
