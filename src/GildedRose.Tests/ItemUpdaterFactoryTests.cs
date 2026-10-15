using GildedRose.Console;
using GildedRose.Console.Updaters.Abstractions;
using GildedRose.Console.Updaters.Factory;
using GildedRose.Console.Updaters.Strategies;
using Xunit;

namespace GildedRose.Tests;

/// <summary>
/// Tests the factory's selection logic: exact names first, then the "Conjured" prefix,
/// otherwise the normal updater.
/// </summary>
public class ItemUpdaterFactoryTests
{
    private readonly IItemUpdaterFactory _factory = new ItemUpdaterFactory();

    // Each known category maps to its dedicated strategy. A prefix match covers every Conjured item.
    [Theory]
    [InlineData("Aged Brie", typeof(AgedBrieUpdater))]
    [InlineData("Backstage passes to a TAFKAL80ETC concert", typeof(BackstagePassUpdater))]
    [InlineData("Sulfuras, Hand of Ragnaros", typeof(SulfurasUpdater))]
    [InlineData("Conjured Mana Cake", typeof(ConjuredItemUpdater))]
    [InlineData("Conjured", typeof(ConjuredItemUpdater))]
    [InlineData("+5 Dexterity Vest", typeof(NormalItemUpdater))]
    public void Returns_the_updater_for_the_item(string name, Type expected)
    {
        var updater = _factory.GetUpdater(new Item { Name = name });

        Assert.IsType(expected, updater);
    }

    // Negative cases: matching is exact and case-sensitive, and "Conjured" must be a prefix, so these
    // all fall back to the normal updater.
    [Theory]
    [InlineData("")]
    [InlineData("aged brie")]
    [InlineData("AGED BRIE")]
    [InlineData("conjured mana cake")]
    [InlineData("Not Conjured Cake")]
    [InlineData("Unknown Item")]
    public void Falls_back_to_normal_updater_for_unrecognised_names(string name)
    {
        var updater = _factory.GetUpdater(new Item { Name = name });

        Assert.IsType<NormalItemUpdater>(updater);
    }

    // Updaters are stateless, so the factory shares one instance per strategy instead of allocating.
    [Fact]
    public void Reuses_the_same_updater_instance()
    {
        var first = _factory.GetUpdater(new Item { Name = "Aged Brie" });
        var second = _factory.GetUpdater(new Item { Name = "Aged Brie" });

        Assert.Same(first, second);
    }
}
