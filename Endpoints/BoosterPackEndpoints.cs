using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TcgApi.Data.Repositories;
using TcgApi.Data.Models;
using TcgApi.Services;

namespace TcgApi.Endpoints;

public class BoosterPackEndpoints : IEndpoint
{
    private static readonly BoosterPackRandomizer Randomizer = new();

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/booster-packs").RequireAuthorization();

        group.MapGet("/open", async (
            HttpContext ctx,
            UserRepository userRepo,
            CardRepository cardRepo,
            CollectionRepository collectionRepo,
            BoosterPackRepository packRepo) =>
        {
            var userIdClaim = ctx.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var user = await userRepo.GetByIdAsync(userId);
            if (user is null)
                return Results.NotFound(new { error = "User not found." });

            if (user.BoosterPacksAvailable <= 0)
                return Results.BadRequest(new { error = "No booster packs available." });

            // v1.0: single collection, pick the first active one
            var collectionIds = await collectionRepo.GetActiveCollectionIdsAsync();
            if (collectionIds.Count == 0)
                return Results.Problem("No active collections available.");

            var collectionId = collectionIds.First();

            // Load all cards from the collection grouped by rarity
            var cardsByRarity = await cardRepo.GetCardIdsByRarityAsync(collectionId);

            if (cardsByRarity.Count == 0)
                return Results.Problem("Selected collection has no cards.");

            // Pull cards using slot-based rarity rules
            var drawnCardIds = Randomizer.Draw(cardsByRarity);

            // Persist the pack open record
            var packOpen = new BoosterPackOpen
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                CollectionId = collectionId,
                OpenedAt = DateTimeOffset.UtcNow
            };
            packRepo.AddPackOpen(packOpen);

            foreach (var cardId in drawnCardIds)
            {
                packRepo.AddPackCard(new BoosterPackCard
                {
                    Id = Guid.NewGuid(),
                    OpenId = packOpen.Id,
                    CardId = cardId
                });
            }

            // Check which drawn cards the user already owns
            var ownedCardIds = await packRepo.GetUserCardIdsAsync(user.Id);
            var repeatCardIds = new HashSet<Guid>(drawnCardIds.Where(id => ownedCardIds.Contains(id)));

            // Only create UserCard for cards the user doesn't already own
            foreach (var cardId in drawnCardIds)
            {
                if (!ownedCardIds.Contains(cardId))
                {
                    packRepo.AddUserCard(new UserCard { Id = Guid.NewGuid(), UserId = user.Id, CardId = cardId, Quantity = 1 });
                    ownedCardIds.Add(cardId); // track within this pack to avoid dupes from same draw
                }
            }

            user.BoosterPacksAvailable--;
            await packRepo.SaveChangesAsync();

            // Return pulled cards with repeat flag
            var cardLookup = (await cardRepo.GetByIdsAsync(drawnCardIds))
                .Cast<dynamic>()
                .ToDictionary(c => (Guid)c.Id);

            return Results.Ok(new
            {
                packOpenId = packOpen.Id,
                collectionId,
                openedAt = packOpen.OpenedAt,
                cards = drawnCardIds.Select(id =>
                {
                    var d = cardLookup[id];
                    return new
                    {
                        d.Id,
                        d.Number,
                        d.Name,
                        d.Type,
                        d.Rarity,
                        d.FlavorText,
                        d.ArtUrl,
                        d.ArtistCredit,
                        isRepeat = repeatCardIds.Contains(id)
                    };
                })
            });
        });

        group.MapGet("/status", async (HttpContext ctx, UserRepository userRepo) =>
        {
            var userIdClaim = ctx.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var user = await userRepo.GetByIdAsync(userId);
            if (user is null)
                return Results.NotFound(new { error = "User not found." });

            var totalPacksOpened = await userRepo.GetTotalPacksOpenedAsync(user.Id);
            var totalCardsCollected = await userRepo.GetTotalCardsCollectedAsync(user.Id);

            return Results.Ok(new
            {
                packsAvailable = user.BoosterPacksAvailable,
                loginStreak = user.LoginStreak,
                lastLoginDate = user.LastLoginDate,
                tasks = new[]
                {
                    new { task = "Open your first pack",   completed = totalPacksOpened >= 1  },
                    new { task = "Open 10 packs",          completed = totalPacksOpened >= 10 },
                    new { task = "Open 50 packs",          completed = totalPacksOpened >= 50 },
                    new { task = "Collect 10 cards",       completed = totalCardsCollected >= 10  },
                    new { task = "Collect 50 cards",       completed = totalCardsCollected >= 50  },
                    new { task = "Collect 100 cards",      completed = totalCardsCollected >= 100 },
                    new { task = "Maintain a 7-day streak", completed = user.LoginStreak >= 7  },
                },
                progress = new
                {
                    totalPacksOpened,
                    totalCardsCollected
                }
            });
        });
    }
}
