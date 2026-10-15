using GildedRose.Console.Updaters.Abstractions;

namespace GildedRose.Console.Updaters.Strategies;

/// <summary>
/// Rule: "Sulfuras", being a legendary item, never has to be sold or decreases in Quality.
/// Its Quality is 80 and it never alters (the only exception to the 0-50 Quality range).
/// Implements the contract directly because none of the daily template steps apply.
/// </summary>
public class SulfurasUpdater : IItemUpdater
{
    public void Update(Item item)
    {
        // Legendary item: SellIn and Quality never change.
    }
}
