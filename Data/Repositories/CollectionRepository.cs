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
}
