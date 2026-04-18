using Microsoft.EntityFrameworkCore;
using TcgApi.Data.Models;

namespace TcgApi.Data.Repositories;

public class CardRepository(AppDbContext db)
{
    public async Task<List<Card>> GetAllAsync(CardRarity? rarity = null, CardType? type = null)
    {
        var query = db.Cards.Include(c => c.Collection).AsQueryable();

        if (rarity.HasValue)
            query = query.Where(c => c.Rarity == rarity.Value);

        if (type.HasValue)
            query = query.Where(c => c.Type == type.Value);

        return await query.OrderBy(c => c.CollectionId).ThenBy(c => c.Number).ToListAsync();
    }

    public async Task<object?> GetByIdAsync(Guid id)
        => await db.Cards
            .Include(c => c.Collection)
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id,
                c.CollectionId,
                CollectionName = c.Collection.Name,
                c.Number,
                c.Name,
                c.Type,
                c.Rarity,
                c.FlavorText,
                c.ArtUrl,
                c.ArtistCredit,
                c.CreatedAt
            })
            .FirstOrDefaultAsync();

    public async Task<Dictionary<CardRarity, IReadOnlyList<Guid>>> GetCardIdsByRarityAsync(Guid collectionId)
        => await db.Cards
            .Where(c => c.CollectionId == collectionId)
            .GroupBy(c => c.Rarity)
            .ToDictionaryAsync(
                g => g.Key,
                g => (IReadOnlyList<Guid>)g.Select(c => c.Id).ToList());

    public async Task<List<object>> GetByIdsAsync(IEnumerable<Guid> cardIds)
        => await db.Cards
            .Where(c => cardIds.Contains(c.Id))
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
}
