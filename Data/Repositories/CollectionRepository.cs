using Microsoft.EntityFrameworkCore;
using TcgApi.Data.Models;

namespace TcgApi.Data.Repositories;

public class CollectionRepository(AppDbContext db)
{
    public async Task<List<object>> GetAllActiveAsync()
        => await db.Collections
            .Where(c => c.IsActive)
            .OrderBy(c => c.CreatedAt)
            .Select(c => (object)new
            {
                c.Id,
                c.Name,
                c.Slug,
                c.Description,
                c.TotalCards,
                c.CreatedAt
            })
            .ToListAsync();

    public async Task<object?> GetByIdAsync(Guid id)
        => await db.Collections
            .Where(c => c.Id == id && c.IsActive)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Slug,
                c.Description,
                c.TotalCards,
                c.CreatedAt,
                CardCount = db.Cards.Count(card => card.CollectionId == c.Id),
                RarityBreakdown = db.Cards
                    .Where(card => card.CollectionId == c.Id)
                    .GroupBy(card => card.Rarity)
                    .Select(g => new { Rarity = g.Key.ToString(), Count = g.Count() })
                    .ToList()
            })
            .FirstOrDefaultAsync();

    public Task<bool> ExistsActiveAsync(Guid id)
        => db.Collections.AnyAsync(c => c.Id == id && c.IsActive);

    public async Task<List<object>> GetCardsByCollectionIdAsync(Guid collectionId)
        => await db.Cards
            .Where(c => c.CollectionId == collectionId)
            .OrderBy(c => c.Number)
            .Select(c => (object)new
            {
                c.Id,
                c.Number,
                c.Name,
                c.Type,
                c.Rarity,
                c.FlavorText,
                c.ArtUrl,
                c.ArtistCredit
            })
            .ToListAsync();

    public async Task<List<Guid>> GetActiveCollectionIdsAsync()
        => await db.Collections
            .Where(c => c.IsActive)
            .Select(c => c.Id)
            .ToListAsync();

    public async Task<object?> GetUserProgressAsync(Guid collectionId, Guid userId)
    {
        if (!await ExistsActiveAsync(collectionId))
            return null;

        var cards = await db.Cards
            .Where(c => c.CollectionId == collectionId)
            .Select(c => new { c.Id, c.Rarity })
            .ToListAsync();

        var ownedIds = await db.UserCards
            .Where(uc => uc.UserId == userId
                         && db.Cards.Any(c => c.Id == uc.CardId && c.CollectionId == collectionId))
            .Select(uc => uc.CardId)
            .ToListAsync();

        var ownedSet = new HashSet<Guid>(ownedIds);
        var total = cards.Count;
        var owned = cards.Count(c => ownedSet.Contains(c.Id));

        var rarityBreakdown = cards
            .GroupBy(c => c.Rarity)
            .Select(g => new
            {
                Rarity = g.Key.ToString(),
                Owned = g.Count(c => ownedSet.Contains(c.Id)),
                Total = g.Count()
            })
            .ToList();

        return new
        {
            CollectionId = collectionId,
            Owned = owned,
            Total = total,
            Missing = total - owned,
            RarityBreakdown = rarityBreakdown
        };
    }
}
