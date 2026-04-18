using System.Security.Claims;
using TcgApi.Data.Repositories;
using TcgApi.Data.Models;
using TcgApi.Data.Models.Requests;
using TcgApi.Services;

namespace TcgApi.Endpoints;

public class BoosterPackEndpoints : IEndpoint
{
    private static readonly BoosterPackRandomizer Randomizer = new();

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/booster-packs").RequireAuthorization();

        group.MapPost("/open", async (
            OpenBoosterPackRequest req,
            HttpContext ctx,
            UserRepository userRepo,
            CardRepository cardRepo,
            CollectionRepository collectionRepo,
            BoosterPackRepository packRepo) =>
        {
            var email = ctx.User.FindFirstValue(ClaimTypes.Email);
            if (email is null)
                return Results.Unauthorized();

            var user = await userRepo.GetByEmailAsync(email);
            if (user is null)
                return Results.NotFound(new { error = "User not found." });

            if (user.BoosterPacksAvailable <= 0)
                return Results.BadRequest(new { error = "No booster packs available." });

            // Resolve collection: use requested one or pick a random active collection
            Guid collectionId;
            if (req.CollectionId.HasValue)
            {
                if (!await collectionRepo.ExistsActiveAsync(req.CollectionId.Value))
                    return Results.NotFound(new { error = "Collection not found." });
                collectionId = req.CollectionId.Value;
            }
            else
            {
                var collectionIds = await collectionRepo.GetActiveCollectionIdsAsync();
                if (collectionIds.Count == 0)
                    return Results.Problem("No active collections available.");

                collectionId = collectionIds[Random.Shared.Next(collectionIds.Count)];
            }

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

            // Upsert UserCard quantities
            foreach (var cardId in drawnCardIds)
            {
                var userCard = await packRepo.GetUserCardAsync(user.Id, cardId);

                if (userCard is null)
                    packRepo.AddUserCard(new UserCard { Id = Guid.NewGuid(), UserId = user.Id, CardId = cardId, Quantity = 1 });
                else
                    userCard.Quantity++;
            }

            user.BoosterPacksAvailable--;
            await packRepo.SaveChangesAsync();

            // Return pulled cards
            var cards = await cardRepo.GetByIdsAsync(drawnCardIds);

            return Results.Ok(new
            {
                packOpenId = packOpen.Id,
                collectionId,
                openedAt = packOpen.OpenedAt,
                cards
            });
        });

        group.MapGet("/status", async (HttpContext ctx, UserRepository userRepo) =>
        {
            var email = ctx.User.FindFirstValue(ClaimTypes.Email);
            if (email is null)
                return Results.Unauthorized();

            var user = await userRepo.GetByEmailAsync(email);
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
