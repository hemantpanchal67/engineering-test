using GildedRose.Console.Updaters.Abstractions;

namespace GildedRose.Console.Updaters.Strategies;

/// <summary>
/// Rule (new requirement): "Conjured" items degrade in Quality twice as fast as normal items.
/// Assumption: this also doubles the post-expiry degradation (4 per day after the sell-by date).
/// </summary>
public class ConjuredItemUpdater : NormalItemUpdater
{
    private const int DailyDegradeRate = 2;

    protected override int DegradeRate => DailyDegradeRate;
}
