namespace GildedRose.Console.Updaters.Abstractions;

/// <summary>Chooses the update strategy for an item.</summary>
public interface IItemUpdaterFactory
{
    IItemUpdater GetUpdater(Item item);
}
