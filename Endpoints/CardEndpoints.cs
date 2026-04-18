using TcgApi.Data.Repositories;
using TcgApi.Data.Models;

namespace TcgApi.Endpoints;

public class CardEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/cards");

        group.MapGet("", async (
            CardRepository repo,
            CardRarity? rarity,
            CardType? type) =>
        {
            var cards = await repo.GetAllAsync(rarity, type);
            return Results.Ok(cards);
        });

        group.MapGet("{id:guid}", async (Guid id, CardRepository repo) =>
        {
            var card = await repo.GetByIdAsync(id);
            return card is null ? Results.NotFound() : Results.Ok(card);
        });
    }
}
