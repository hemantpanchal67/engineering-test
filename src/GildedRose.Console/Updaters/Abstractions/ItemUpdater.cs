namespace GildedRose.Console.Updaters.Abstractions;

/// <summary>
/// Template Method: fixes the daily update order (quality, age, expiry) and lets each
/// item category override only the steps that differ. Implementations are stateless,
/// so a single shared instance can be reused for every item.
/// <para>
/// Rules (README, Exercise 2): every item has SellIn (days left to sell) and Quality;
/// at the end of each day both values drop. Quality is never negative and never above 50.
/// </para>
/// </summary>
public abstract class ItemUpdater : IItemUpdater
{
    // Rule: "The Quality of an item is never negative."
    protected const int MinQuality = 0;

    // Rule: "The Quality of an item is never more than 50."
    protected const int MaxQuality = 50;

    public virtual void Update(Item item)
    {
        UpdateQuality(item);
        Age(item);

        // Rule: "Once the sell by date has passed, Quality degrades twice as fast."
        if (item.SellIn < 0)
        {
            UpdateExpiredQuality(item);
        }
    }

    /// <summary>Daily quality change, evaluated before SellIn is decremented.</summary>
    protected abstract void UpdateQuality(Item item);

    /// <summary>Additional quality change once the sell-by date has passed.</summary>
    protected abstract void UpdateExpiredQuality(Item item);

    // Rule: "At the end of each day our system lowers both values for every item."
    private static void Age(Item item) => item.SellIn--;

    /// <summary>Applies a delta while keeping quality within [MinQuality, MaxQuality].</summary>
    protected static void ChangeQuality(Item item, int delta)
    {
        if (delta > 0)
        {
            if (item.Quality < MaxQuality)
            {
                item.Quality = Math.Min(MaxQuality, item.Quality + delta);
            }
        }
        else
        {
            item.Quality = Math.Max(MinQuality, item.Quality + delta);
        }
    }
}
