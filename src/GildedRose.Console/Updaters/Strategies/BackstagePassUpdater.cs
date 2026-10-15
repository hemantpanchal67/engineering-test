using GildedRose.Console.Updaters.Abstractions;

namespace GildedRose.Console.Updaters.Strategies;

/// <summary>
/// Rule: "Backstage passes" increase in Quality as the SellIn value approaches:
/// by 2 when there are 10 days or less, by 3 when there are 5 days or less,
/// but Quality drops to 0 after the concert.
/// </summary>
public class BackstagePassUpdater : ItemUpdater
{
    private const int BaseIncrease = 1;
    private const int ApproachingIncrease = 2;
    private const int CriticalIncrease = 3;

    // Days left (SellIn) at or below which the increase applies.
    private const int ApproachingThreshold = 10;
    private const int CriticalThreshold = 5;

    protected override void UpdateQuality(Item item)
    {
        // Evaluated before SellIn is decremented, so thresholds are inclusive.
        var delta = item.SellIn <= CriticalThreshold ? CriticalIncrease
                  : item.SellIn <= ApproachingThreshold ? ApproachingIncrease
                  : BaseIncrease;

        ChangeQuality(item, delta);
    }

    // Rule: Quality drops to 0 after the concert.
    protected override void UpdateExpiredQuality(Item item) => item.Quality = MinQuality;
}
