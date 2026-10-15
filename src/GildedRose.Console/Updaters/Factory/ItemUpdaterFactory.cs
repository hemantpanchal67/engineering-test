using GildedRose.Console.Updaters.Abstractions;
using GildedRose.Console.Updaters.Strategies;

namespace GildedRose.Console.Updaters.Factory;

/// <summary>
/// Factory: chooses the updater for an item. To support a new category, add a class
/// and one registration here; nothing else changes.
/// </summary>
public class ItemUpdaterFactory : IItemUpdaterFactory
{
    private const string ConjuredPrefix = "Conjured";

    private static readonly IItemUpdater Normal = new NormalItemUpdater();
    private static readonly IItemUpdater Conjured = new ConjuredItemUpdater();

    private readonly Dictionary<string, IItemUpdater> _byName = new(StringComparer.Ordinal)
    {
        ["Aged Brie"] = new AgedBrieUpdater(),
        ["Backstage passes to a TAFKAL80ETC concert"] = new BackstagePassUpdater(),
        ["Sulfuras, Hand of Ragnaros"] = new SulfurasUpdater(),
    };

    public IItemUpdater GetUpdater(Item item)
    {
        if (_byName.TryGetValue(item.Name, out var updater))
        {
            return updater;
        }

        return item.Name.StartsWith(ConjuredPrefix, StringComparison.Ordinal) ? Conjured : Normal;
    }
}
