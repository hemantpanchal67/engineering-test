using GildedRose.Console.Updaters.Abstractions;

namespace GildedRose.Console.Updaters.Strategies;

/// <summary>
/// Rule: normal items lose 1 Quality per day, and once the sell-by date has passed
/// Quality degrades twice as fast (an extra <see cref="DegradeRate"/> per day).
/// </summary>
public class NormalItemUpdater : ItemUpdater
{
    private const int DailyDegradeRate = 1;

    protected virtual int DegradeRate => DailyDegradeRate;

    protected override void UpdateQuality(Item item) => ChangeQuality(item, -DegradeRate);

    protected override void UpdateExpiredQuality(Item item) => ChangeQuality(item, -DegradeRate);
}
