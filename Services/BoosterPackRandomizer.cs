using TcgApi.Data.Models;

namespace TcgApi.Services;

public class BoosterPackRandomizer
{
    public const int CardsPerPack = 6;

    // Slot definitions: each slot is drawn from a pool of allowed rarities at equal odds.
    private static readonly CardRarity[][] SlotPools =
    [
        [CardRarity.Common],
        [CardRarity.Common],
        [CardRarity.Common, CardRarity.Uncommon],
        [CardRarity.Uncommon],
        [CardRarity.Uncommon, CardRarity.Rare],
        [CardRarity.Rare, CardRarity.Legendary],
    ];

    /// <summary>
    /// Selects <see cref="CardsPerPack"/> card IDs from <paramref name="cardsByRarity"/>
    /// according to the fixed slot rules. Slots whose required rarities are entirely absent
    /// from the collection fall back to the closest available rarity tier.
    /// </summary>
    /// <param name="cardsByRarity">All cards in the collection, keyed by rarity.</param>
    /// <returns>List of drawn card IDs (may contain duplicates).</returns>
    public IReadOnlyList<Guid> Draw(IReadOnlyDictionary<CardRarity, IReadOnlyList<Guid>> cardsByRarity)
    {
        var drawn = new List<Guid>(CardsPerPack);

        foreach (var slotPool in SlotPools)
        {
            var pool = BuildPool(slotPool, cardsByRarity);
            drawn.Add(pool[Random.Shared.Next(pool.Count)]);
        }

        return drawn;
    }

    // Merges the card lists for each rarity in the slot pool.
    // Falls back up the rarity ladder if none of the preferred rarities are present.
    private static IReadOnlyList<Guid> BuildPool(
        CardRarity[] preferredRarities,
        IReadOnlyDictionary<CardRarity, IReadOnlyList<Guid>> cardsByRarity)
    {
        var combined = new List<Guid>();

        foreach (var rarity in preferredRarities)
        {
            if (cardsByRarity.TryGetValue(rarity, out var cards))
                combined.AddRange(cards);
        }

        if (combined.Count > 0)
            return combined;

        // Fallback: use any available rarity in ascending order
        foreach (CardRarity rarity in Enum.GetValues<CardRarity>())
        {
            if (cardsByRarity.TryGetValue(rarity, out var cards) && cards.Count > 0)
                return cards;
        }

        throw new InvalidOperationException("Collection contains no cards.");
    }
}
