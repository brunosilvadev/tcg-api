using TcgApi.Data.Models;
using TcgApi.Services;
using Xunit;

namespace TcgApi.Tests;

public class BoosterPackRandomizerTests
{
    [Fact]
    public void Draw_UsesNearestAvailableRarity_WhenHigherTiersAreMissing()
    {
        var randomizer = new BoosterPackRandomizer();
        var commonCardId = Guid.NewGuid();

        var cardsByRarity = new Dictionary<CardRarity, IReadOnlyList<Guid>>
        {
            [CardRarity.Common] = [commonCardId]
        };

        var drawn = randomizer.Draw(cardsByRarity);

        Assert.Equal(BoosterPackRandomizer.CardsPerPack, drawn.Count);
        Assert.All(drawn, cardId => Assert.Equal(commonCardId, cardId));
    }

    [Fact]
    public void Draw_KeepsPremiumCardsBelowHalfAPack_OnBaselineCollection()
    {
        var randomizer = new BoosterPackRandomizer();
        var cardsByRarity = CreateCollectionCardsByRarity();
        var rarityByCardId = cardsByRarity
            .SelectMany(entry => entry.Value.Select(cardId => new { cardId, entry.Key }))
            .ToDictionary(x => x.cardId, x => x.Key);

        const int packCount = 20000;
        var rareCount = 0;
        var legendaryCount = 0;

        for (var index = 0; index < packCount; index++)
        {
            foreach (var cardId in randomizer.Draw(cardsByRarity))
            {
                switch (rarityByCardId[cardId])
                {
                    case CardRarity.Rare:
                        rareCount++;
                        break;
                    case CardRarity.Legendary:
                        legendaryCount++;
                        break;
                }
            }
        }

        var premiumPerPack = (rareCount + legendaryCount) / (double)packCount;
        var legendaryPerPack = legendaryCount / (double)packCount;

        Assert.InRange(premiumPerPack, 0.30, 0.40);
        Assert.InRange(legendaryPerPack, 0.03, 0.07);
    }

    [Fact]
    public void Draw_ReturnsDistinctCards_WhenTheCollectionHasEnoughCards()
    {
        var randomizer = new BoosterPackRandomizer();
        var cardsByRarity = CreateCollectionCardsByRarity();

        var drawn = randomizer.Draw(cardsByRarity);

        Assert.Equal(BoosterPackRandomizer.CardsPerPack, drawn.Count);
        Assert.Equal(BoosterPackRandomizer.CardsPerPack, drawn.Distinct().Count());
    }

    private static Dictionary<CardRarity, IReadOnlyList<Guid>> CreateCollectionCardsByRarity()
        => new()
        {
            [CardRarity.Common] = CreateCardIds(17),
            [CardRarity.Uncommon] = CreateCardIds(12),
            [CardRarity.Rare] = CreateCardIds(6),
            [CardRarity.Legendary] = CreateCardIds(1)
        };

    private static IReadOnlyList<Guid> CreateCardIds(int count)
        => Enumerable.Range(0, count).Select(_ => Guid.NewGuid()).ToList();
}