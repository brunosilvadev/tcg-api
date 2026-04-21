using Microsoft.EntityFrameworkCore;
using TcgApi.Data.Models;

namespace TcgApi.Data.Repositories;

public class BoosterPackRepository(AppDbContext db)
{
    public sealed class LastPackBestCardResult
    {
        public Guid Id { get; init; }
        public int Number { get; init; }
        public string Name { get; init; } = string.Empty;
        public CardType Type { get; init; }
        public CardRarity Rarity { get; init; }
        public string? FlavorText { get; init; }
        public string? ArtUrl { get; init; }
        public string? ArtistCredit { get; init; }
        public Guid PackOpenId { get; init; }
        public DateTimeOffset OpenedAt { get; init; }
    }

    public void AddPackOpen(BoosterPackOpen packOpen)
        => db.BoosterPackOpens.Add(packOpen);

    public void AddPackCard(BoosterPackCard packCard)
        => db.BoosterPackCards.Add(packCard);

    public async Task<UserCard?> GetUserCardAsync(Guid userId, Guid cardId)
        => await db.UserCards
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CardId == cardId);

    public void AddUserCard(UserCard userCard)
        => db.UserCards.Add(userCard);

    public async Task<HashSet<Guid>> GetUserCardIdsAsync(Guid userId)
        => (await db.UserCards
            .Where(uc => uc.UserId == userId)
            .Select(uc => uc.CardId)
            .ToListAsync())
            .ToHashSet();

    public async Task<LastPackBestCardResult?> GetLastPackBestCardAsync(Guid userId)
    {
        var lastPack = await db.BoosterPackOpens
            .Where(packOpen => packOpen.UserId == userId)
            .OrderByDescending(packOpen => packOpen.OpenedAt)
            .ThenByDescending(packOpen => packOpen.Id)
            .Select(packOpen => new
            {
                packOpen.Id,
                packOpen.OpenedAt
            })
            .FirstOrDefaultAsync();

        if (lastPack is null)
            return null;

        return await db.BoosterPackCards
            .Where(packCard => packCard.OpenId == lastPack.Id)
            .Join(
                db.Cards,
                packCard => packCard.CardId,
                card => card.Id,
                (_, card) => card)
            .OrderByDescending(card => card.Rarity)
            .ThenBy(card => card.Number)
            .ThenBy(card => card.Id)
            .Select(card => new LastPackBestCardResult
            {
                Id = card.Id,
                Number = card.Number,
                Name = card.Name,
                Type = card.Type,
                Rarity = card.Rarity,
                FlavorText = card.FlavorText,
                ArtUrl = card.ArtUrl,
                ArtistCredit = card.ArtistCredit,
                PackOpenId = lastPack.Id,
                OpenedAt = lastPack.OpenedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task SaveChangesAsync()
        => await db.SaveChangesAsync();
}
