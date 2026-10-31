using System;

namespace Artisan.Tasks;

internal enum MarketboardSearchStep { Idle, Wait, SelectBoard, InteractWithBoard, FillSearch, SubmitSearch, Cancelled, TimedOut }

internal sealed class MarketboardSearchRequest
{
    internal bool Pending { get; private set; }
    private bool selected;
    private bool textFilled;
    private long submitAfter;
    private long deadline;
    private long nextInteraction;

    internal void Start(long now)
    {
        Pending = true;
        selected = false;
        textFilled = false;
        deadline = now + 10000;
        nextInteraction = now;
    }

    internal void Cancel() => Pending = false;
    internal void SearchSubmitted() => Pending = false;

    internal MarketboardSearchStep Next(long now, bool contextValid, bool searchReady, bool boardAvailable, bool canInteract, bool submitAllowed = true)
    {
        if (!Pending) return MarketboardSearchStep.Idle;
        if (!contextValid) { Pending = false; return MarketboardSearchStep.Cancelled; }
        if (now >= deadline) { Pending = false; return MarketboardSearchStep.TimedOut; }
        if (searchReady)
        {
            if (!textFilled)
            {
                textFilled = true;
                submitAfter = now + 100;
                return MarketboardSearchStep.FillSearch;
            }
            return now < submitAfter || !submitAllowed ? MarketboardSearchStep.Wait : MarketboardSearchStep.SubmitSearch;
        }
        if (textFilled) { Pending = false; return MarketboardSearchStep.Cancelled; }
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
