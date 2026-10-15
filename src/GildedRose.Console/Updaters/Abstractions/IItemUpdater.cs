namespace GildedRose.Console.Updaters.Abstractions;

/// <summary>Strategy: knows how a single category of item changes at the end of a day.</summary>
public interface IItemUpdater
{
    void Update(Item item);
}
