using TcgApi.Data.Models;

namespace TcgApi.Services;

public class BoosterPackRandomizer
{
    public const int CardsPerPack = 6;

    private readonly record struct RarityWeight(CardRarity Rarity, int Weight);

    // Slot definitions: choose a rarity by weight, then draw a random card from that rarity.
    private static readonly RarityWeight[][] SlotWeights =
    [
        [new(CardRarity.Common, 100)],
        [new(CardRarity.Common, 100)],
        [new(CardRarity.Common, 80), new(CardRarity.Uncommon, 20)],
        [new(CardRarity.Uncommon, 100)],
        [new(CardRarity.Common, 20), new(CardRarity.Uncommon, 75), new(CardRarity.Rare, 5)],
        [new(CardRarity.Uncommon, 70), new(CardRarity.Rare, 25), new(CardRarity.Legendary, 5)],
    ];

    /// <summary>
    /// Selects <see cref="CardsPerPack"/> card IDs from <paramref name="cardsByRarity"/>
    /// according to weighted slot rules. If the chosen rarity is absent from the collection,
    /// the draw falls back to the nearest available rarity tier.
    /// </summary>
    /// <param name="cardsByRarity">All cards in the collection, keyed by rarity.</param>
    /// <returns>List of drawn card IDs (may contain duplicates).</returns>
    public IReadOnlyList<Guid> Draw(IReadOnlyDictionary<CardRarity, IReadOnlyList<Guid>> cardsByRarity)
    {
        var drawn = new List<Guid>(CardsPerPack);

        foreach (var slotWeights in SlotWeights)
        {
            var rarity = ChooseRarity(slotWeights);
            var pool = GetCardsForRarityOrNearest(rarity, cardsByRarity);
            drawn.Add(pool[Random.Shared.Next(pool.Count)]);
        }

        return drawn;
    }

    private static CardRarity ChooseRarity(IReadOnlyList<RarityWeight> slotWeights)
    {
        var totalWeight = slotWeights.Sum(slotWeight => slotWeight.Weight);
        var roll = Random.Shared.Next(totalWeight);

        foreach (var slotWeight in slotWeights)
        {
            if (roll < slotWeight.Weight)
                return slotWeight.Rarity;

            roll -= slotWeight.Weight;
        }

        throw new InvalidOperationException("Slot weights must contain at least one positive weight.");
    }

    private static IReadOnlyList<Guid> GetCardsForRarityOrNearest(
        CardRarity selectedRarity,
        IReadOnlyDictionary<CardRarity, IReadOnlyList<Guid>> cardsByRarity)
    {
        if (TryGetAvailableCards(selectedRarity, cardsByRarity, out var cards))
            return cards;

        var selectedIndex = (int)selectedRarity;
        var rarityCount = Enum.GetValues<CardRarity>().Length;

        for (var distance = 1; distance < rarityCount; distance++)
        {
            var lowerIndex = selectedIndex - distance;
            if (lowerIndex >= 0
                && TryGetAvailableCards((CardRarity)lowerIndex, cardsByRarity, out cards))
            {
                return cards;
            }

            var higherIndex = selectedIndex + distance;
            if (higherIndex < rarityCount
                && TryGetAvailableCards((CardRarity)higherIndex, cardsByRarity, out cards))
            {
                return cards;
            }
        }

        throw new InvalidOperationException("Collection contains no cards.");
    }

    private static bool TryGetAvailableCards(
        CardRarity rarity,
        IReadOnlyDictionary<CardRarity, IReadOnlyList<Guid>> cardsByRarity,
        out IReadOnlyList<Guid> cards)
    {
        if (cardsByRarity.TryGetValue(rarity, out var availableCards) && availableCards.Count > 0)
        {
            cards = availableCards;
            return true;
        }

        cards = Array.Empty<Guid>();
        return false;
    }
}
