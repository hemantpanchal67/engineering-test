using GildedRose.Console;
using Xunit;

namespace GildedRose.Tests;

/// <summary>
/// Behavioural tests for <see cref="Program.UpdateQuality"/>, covering the Gilded Rose README rules
/// end to end (client, factory and strategies). Each case is: name, SellIn and Quality before the
/// day, then the expected SellIn and Quality after one day.
/// </summary>
// Shares a collection with every test that runs UpdateQuality, because it writes to System.Console
// and one test below redirects the console; xUnit runs tests within a collection sequentially.
[Collection("Console")]
public class UpdateQualityTests
{
    private const string Brie = "Aged Brie";
    private const string Sulfuras = "Sulfuras, Hand of Ragnaros";
    private const string Pass = "Backstage passes to a TAFKAL80ETC concert";
    private const string Conjured = "Conjured Mana Cake";

    // Runs one day of updates for a single item and returns it so the caller can assert on it.
    private static Item Run(string name, int sellIn, int quality)
    {
        var item = new Item { Name = name, SellIn = sellIn, Quality = quality };
        new Program { Items = new List<Item> { item } }.UpdateQuality();
        return item;
    }

    // Core rules, one row per README rule. "Vest" stands for any normal item.
    [Theory]
    // Normal: -1 per day, -2 per day once the sell-by date has passed, never below 0
    [InlineData("Vest", 10, 20, 9, 19)]
    [InlineData("Vest", 0, 20, -1, 18)]
    [InlineData("Vest", 5, 0, 4, 0)]
    // Aged Brie: gains Quality as it ages (+1, +2 once expired), never above 50
    [InlineData(Brie, 2, 0, 1, 1)]
    [InlineData(Brie, 0, 10, -1, 12)]
    [InlineData(Brie, 2, 50, 1, 50)]
    // Sulfuras: legendary, Quality stays 80 and SellIn never changes
    [InlineData(Sulfuras, 0, 80, 0, 80)]
    // Backstage passes: +1 (>10 days), +2 (10 days or fewer), +3 (5 days or fewer), cap 50, 0 after the concert
    [InlineData(Pass, 15, 20, 14, 21)]
    [InlineData(Pass, 10, 20, 9, 22)]
    [InlineData(Pass, 5, 20, 4, 23)]
    [InlineData(Pass, 5, 49, 4, 50)]
    [InlineData(Pass, 0, 20, -1, 0)]
    // Conjured: degrades twice as fast as normal (-2 per day, -4 once expired)
    [InlineData(Conjured, 3, 6, 2, 4)]
    [InlineData(Conjured, 0, 6, -1, 2)]
    [InlineData(Conjured, 3, 1, 2, 0)]
    // Backstage: values just outside each threshold (11, 6), at the last day (1) and at the cap
    [InlineData(Pass, 11, 20, 10, 21)]
    [InlineData(Pass, 6, 20, 5, 22)]
    [InlineData(Pass, 1, 20, 0, 23)]
    [InlineData(Pass, 10, 49, 9, 50)]
    [InlineData(Pass, 10, 50, 9, 50)]
    // Aged Brie: cap when expired
    [InlineData(Brie, 0, 49, -1, 50)]
    [InlineData(Brie, 0, 50, -1, 50)]
    // Normal: floor when expired
    [InlineData("Vest", 0, 1, -1, 0)]
    [InlineData("Vest", 0, 0, -1, 0)]
    // Conjured: already at zero, and the floor holds when the expired -4 would go negative
    [InlineData(Conjured, 3, 0, 2, 0)]
    [InlineData(Conjured, 0, 3, -1, 0)]
    // Sulfuras: never changes whatever the SellIn
    [InlineData(Sulfuras, 5, 80, 5, 80)]
    [InlineData(Sulfuras, -1, 80, -1, 80)]
    public void Updates_item_for_one_day(string name, int sellIn, int quality, int expectedSellIn, int expectedQuality)
    {
        var item = Run(name, sellIn, quality);

        Assert.Equal(expectedSellIn, item.SellIn);
        Assert.Equal(expectedQuality, item.Quality);
    }

    // Negative and unusual inputs: name matching is exact and case-sensitive, and Quality may start
    // outside the normal 0-50 range. The expected values preserve the original (legacy) behaviour.
    [Theory]
    [InlineData("aged brie", 5, 20, 4, 19)]
    [InlineData("", 5, 20, 4, 19)]
    [InlineData("Not Conjured Cake", 5, 20, 4, 19)]
    // Legacy quirk: quality already above the 50 cap is not clamped down by increases
    [InlineData(Brie, 5, 55, 4, 55)]
    [InlineData(Pass, 5, 55, 4, 55)]
    [InlineData("Vest", 5, 55, 4, 54)]
    // Backstage already past the concert stays worthless
    [InlineData(Pass, -1, 0, -2, 0)]
    public void Handles_unusual_names_and_out_of_range_quality(string name, int sellIn, int quality, int expectedSellIn, int expectedQuality)
    {
        var item = Run(name, sellIn, quality);

        Assert.Equal(expectedSellIn, item.SellIn);
        Assert.Equal(expectedQuality, item.Quality);
    }

    // Every item in the list is updated, each with its own strategy.
    [Fact]
    public void Updates_every_item_in_the_list()
    {
        var items = new List<Item>
        {
            new() { Name = "Vest", SellIn = 5, Quality = 10 },
            new() { Name = Brie, SellIn = 5, Quality = 10 },
            new() { Name = Sulfuras, SellIn = 0, Quality = 80 },
            new() { Name = Conjured, SellIn = 5, Quality = 10 },
        };

        new Program { Items = items }.UpdateQuality();

        Assert.Equal(new[] { 9, 11, 80, 8 }, items.Select(i => i.Quality));
        Assert.Equal(new[] { 4, 4, 0, 4 }, items.Select(i => i.SellIn));
    }

    // Edge case: nothing to update must not throw.
    [Fact]
    public void Does_nothing_for_an_empty_list()
    {
        var exception = Record.Exception(() => new Program { Items = new List<Item>() }.UpdateQuality());

        Assert.Null(exception);
    }

    // The console report from the original program is preserved. Redirect the console to capture it,
    // and always restore it so other tests are unaffected.
    [Fact]
    public void Writes_one_line_per_item_to_the_console()
    {
        var original = System.Console.Out;
        var writer = new StringWriter();
        System.Console.SetOut(writer);
        try
        {
            Run("Vest", 5, 10);
        }
        finally
        {
            System.Console.SetOut(original);
        }

        Assert.Equal("Item Name: Vest, SellIn: 4, Quality: 9", writer.ToString().Trim());
    }
}
