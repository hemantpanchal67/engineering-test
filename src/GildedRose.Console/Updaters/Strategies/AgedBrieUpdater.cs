using GildedRose.Console.Updaters.Abstractions;

namespace GildedRose.Console.Updaters.Strategies;

/// <summary>
/// Rule: "Aged Brie" actually increases in Quality the older it gets (never above 50).
/// Once expired it keeps increasing, by one more per day.
/// </summary>
public class AgedBrieUpdater : ItemUpdater
{
    private const int DailyIncrease = 1;

    protected override void UpdateQuality(Item item) => ChangeQuality(item, DailyIncrease);

    protected override void UpdateExpiredQuality(Item item) => ChangeQuality(item, DailyIncrease);
}
