using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TcgApi.Data.Repositories;

namespace TcgApi.Endpoints;

public class CollectionEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/collections");

        group.MapGet("", async (CollectionRepository repo) =>
        {
            var collections = await repo.GetAllActiveAsync();
            return Results.Ok(collections);
        });

        group.MapGet("{id:guid}", async (Guid id, CollectionRepository repo) =>
        {
            var collection = await repo.GetByIdAsync(id);
            return collection is null ? Results.NotFound() : Results.Ok(collection);
        });

        group.MapGet("{id:guid}/cards", async (Guid id, CollectionRepository repo) =>
        {
            if (!await repo.ExistsActiveAsync(id))
                return Results.NotFound();

            var cards = await repo.GetCardsByCollectionIdAsync(id);
            return Results.Ok(cards);
        });

        group.MapGet("{id:guid}/progress", async (
            Guid id,
            HttpContext ctx,
            CollectionRepository repo) =>
        {
            var userIdClaim = ctx.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var progress = await repo.GetUserProgressAsync(id, userId);
            return progress is null ? Results.NotFound() : Results.Ok(progress);
        }).RequireAuthorization();
    }
}
