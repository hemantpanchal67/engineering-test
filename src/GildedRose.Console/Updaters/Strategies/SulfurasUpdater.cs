using GildedRose.Console.Updaters.Abstractions;

namespace GildedRose.Console.Updaters.Strategies;

/// <summary>
/// Rule: "Sulfuras", being a legendary item, never has to be sold or decreases in Quality.
/// Its Quality is 80 and it never alters.
/// </summary>
public class SulfurasUpdater : ItemUpdater
{
    protected override void UpdateQuality(Item item)
    {
    }

    protected override void UpdateExpiredQuality(Item item)
    {
    }

    protected override void Age(Item item)
    {
    }
}
