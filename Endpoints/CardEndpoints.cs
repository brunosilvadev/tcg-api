using Microsoft.EntityFrameworkCore;
using TcgApi.Data;
using TcgApi.Models;

namespace TcgApi.Endpoints;

public class CardEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/cards");

        group.MapGet("", async (
            AppDbContext db,
            CardRarity? rarity,
            CardType? type) =>
        {
            var query = db.Cards.Include(c => c.Collection).AsQueryable();

            if (rarity.HasValue)
                query = query.Where(c => c.Rarity == rarity.Value);

            if (type.HasValue)
                query = query.Where(c => c.Type == type.Value);

            var cards = await query.OrderBy(c => c.CollectionId).ThenBy(c => c.Number).ToListAsync();
            return Results.Ok(cards);
        });
    }
}
