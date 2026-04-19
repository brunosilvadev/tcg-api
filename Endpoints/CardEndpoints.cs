using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TcgApi.Data.Repositories;
using TcgApi.Data.Models;

namespace TcgApi.Endpoints;

public class CardEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/cards");

        group.MapGet("", async (HttpContext ctx, CardRepository repo) =>
        {
            var userIdClaim = ctx.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var cards = await repo.GetByUserIdAsync(userId);
            return Results.Ok(cards);
        }).RequireAuthorization();

        group.MapGet("{id:guid}", async (Guid id, CardRepository repo) =>
        {
            var card = await repo.GetByIdAsync(id);
            return card is null ? Results.NotFound() : Results.Ok(card);
        });
    }
}
