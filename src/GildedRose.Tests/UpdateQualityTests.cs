using GildedRose.Console;
using Xunit;

namespace GildedRose.Tests;

public class UpdateQualityTests
{
    private const string Brie = "Aged Brie";
    private const string Sulfuras = "Sulfuras, Hand of Ragnaros";
    private const string Pass = "Backstage passes to a TAFKAL80ETC concert";

    private static Item Run(string name, int sellIn, int quality)
    {
        var item = new Item { Name = name, SellIn = sellIn, Quality = quality };
        new Program { Items = new List<Item> { item } }.UpdateQuality();
        return item;
    }

    [Theory]
    [InlineData("Vest", 10, 20, 9, 19)]
    [InlineData("Vest", 0, 20, -1, 18)]
    [InlineData("Vest", 5, 0, 4, 0)]
    [InlineData(Brie, 2, 0, 1, 1)]
    [InlineData(Brie, 0, 10, -1, 12)]
    [InlineData(Brie, 2, 50, 1, 50)]
    [InlineData(Sulfuras, 0, 80, 0, 80)]
    [InlineData(Pass, 15, 20, 14, 21)]
    [InlineData(Pass, 10, 20, 9, 22)]
    [InlineData(Pass, 5, 20, 4, 23)]
    [InlineData(Pass, 5, 49, 4, 50)]
    [InlineData(Pass, 0, 20, -1, 0)]
    public void Updates_item_for_one_day(string name, int sellIn, int quality, int expectedSellIn, int expectedQuality)
    {
        var item = Run(name, sellIn, quality);

        Assert.Equal(expectedSellIn, item.SellIn);
        Assert.Equal(expectedQuality, item.Quality);
    }
}
