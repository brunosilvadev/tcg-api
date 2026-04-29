using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Data.Common;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TcgApi.Data;
using TcgApi.Data.Models;
using Xunit;

namespace TcgApi.Tests;

public class BoosterPackOpenEndpointTests
{
    [Fact]
    public async Task OpenPack_ReturnsSixDistinctCards_WhenTheCollectionHasEnoughCards()
    {
        var databaseName = $"booster-pack-distinct-{Guid.NewGuid()}";
        await using var factory = new BoosterPackApiFactory(databaseName);
        var userId = Guid.NewGuid();
        var collectionId = Guid.NewGuid();

        await SeedBaselineCollectionAsync(factory.Services, userId, collectionId);

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.SchemeName);
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeaderName, userId.ToString());

        using var response = await client.GetAsync("/booster-packs/open");
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync());
        var cards = document.RootElement.GetProperty("cards").EnumerateArray().ToList();
        var cardIds = cards.Select(card => card.GetProperty("id").GetGuid()).ToList();

        Assert.Equal(6, cards.Count);
        Assert.Equal(6, cardIds.Distinct().Count());
        Assert.All(cards, card => Assert.False(card.GetProperty("isRepeat").GetBoolean()));

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Equal(6, await db.UserCards.CountAsync());
        Assert.Equal(6, await db.BoosterPackCards.CountAsync());
    }

    [Fact]
    public async Task OpenPack_IncrementsQuantityForDuplicatesWithinTheSamePack()
    {
        var databaseName = $"booster-pack-open-{Guid.NewGuid()}";
        await using var factory = new BoosterPackApiFactory(databaseName);
        var userId = Guid.NewGuid();
        var collectionId = Guid.NewGuid();
        var cardId = Guid.NewGuid();

        await SeedDataAsync(factory.Services, userId, collectionId, cardId);

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.SchemeName);
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeaderName, userId.ToString());

        using var response = await client.GetAsync("/booster-packs/open");
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync());
        var cards = document.RootElement.GetProperty("cards").EnumerateArray().ToList();

        Assert.Equal(6, cards.Count);
        for (var index = 0; index < cards.Count; index++)
        {
            Assert.False(cards[index].GetProperty("isRepeat").GetBoolean());
            Assert.Equal(index + 1, cards[index].GetProperty("quantityOwned").GetInt32());
        }

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userCard = await db.UserCards.SingleAsync();
        var user = await db.Users.SingleAsync();

        Assert.Equal(cardId, userCard.CardId);
        Assert.Equal(6, userCard.Quantity);
        Assert.Equal(0, user.BoosterPacksAvailable);
        Assert.Equal(1, await db.BoosterPackOpens.CountAsync());
        Assert.Equal(6, await db.BoosterPackCards.CountAsync());
    }

    [Fact]
    public async Task OpenPack_MarksCardsAsRepeat_WhenTheUserOwnedThemBeforeOpening()
    {
        var databaseName = $"booster-pack-owned-{Guid.NewGuid()}";
        await using var factory = new BoosterPackApiFactory(databaseName);
        var userId = Guid.NewGuid();
        var collectionId = Guid.NewGuid();
        var cardId = Guid.NewGuid();

        await SeedDataAsync(factory.Services, userId, collectionId, cardId, ownsCardBeforeOpening: true);

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.SchemeName);
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeaderName, userId.ToString());

        using var response = await client.GetAsync("/booster-packs/open");
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync());
        var cards = document.RootElement.GetProperty("cards").EnumerateArray().ToList();

        Assert.Equal(6, cards.Count);

        for (var index = 0; index < cards.Count; index++)
        {
            Assert.True(cards[index].GetProperty("isRepeat").GetBoolean());
            Assert.Equal(index + 2, cards[index].GetProperty("quantityOwned").GetInt32());
        }

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userCard = await db.UserCards.SingleAsync();

        Assert.Equal(7, userCard.Quantity);
    }

    private static async Task SeedDataAsync(
        IServiceProvider services,
        Guid userId,
        Guid collectionId,
        Guid cardId,
        bool ownsCardBeforeOpening = false)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.EnsureCreatedAsync();

        db.Collections.Add(new Collection
        {
            Id = collectionId,
            Name = "Test Collection",
            Slug = "test-collection",
            TotalCards = 1,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        });

        db.Cards.Add(new Card
        {
            Id = cardId,
            CollectionId = collectionId,
            Number = 1,
            Name = "Only Card",
            Type = CardType.Creature,
            Rarity = CardRarity.Common,
            CreatedAt = DateTimeOffset.UtcNow
        });

        db.Users.Add(new User
        {
            Id = userId,
            Email = "player@example.com",
            Username = "player",
            PasswordHash = "hash",
            BoosterPacksAvailable = 1,
            CreatedAt = DateTimeOffset.UtcNow
        });

        if (ownsCardBeforeOpening)
        {
            db.UserCards.Add(new UserCard
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CardId = cardId,
                Quantity = 1,
                FirstObtainedAt = DateTimeOffset.UtcNow.AddDays(-1)
            });
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedBaselineCollectionAsync(IServiceProvider services, Guid userId, Guid collectionId)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.EnsureCreatedAsync();

        db.Collections.Add(new Collection
        {
            Id = collectionId,
            Name = "Baseline Collection",
            Slug = $"baseline-{collectionId:N}",
            TotalCards = 36,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        });

        db.Users.Add(new User
        {
            Id = userId,
            Email = $"player-{userId:N}@example.com",
            Username = $"player-{userId:N}",
            PasswordHash = "hash",
            BoosterPacksAvailable = 1,
            CreatedAt = DateTimeOffset.UtcNow
        });

        db.Cards.AddRange(CreateCards(collectionId, CardRarity.Common, 1, 17));
        db.Cards.AddRange(CreateCards(collectionId, CardRarity.Uncommon, 18, 12));
        db.Cards.AddRange(CreateCards(collectionId, CardRarity.Rare, 30, 6));
        db.Cards.AddRange(CreateCards(collectionId, CardRarity.Legendary, 36, 1));

        await db.SaveChangesAsync();
    }

    private static IEnumerable<Card> CreateCards(Guid collectionId, CardRarity rarity, int startNumber, int count)
        => Enumerable.Range(startNumber, count)
            .Select(number => new Card
            {
                Id = Guid.NewGuid(),
                CollectionId = collectionId,
                Number = number,
                Name = $"{rarity} Card {number}",
                Type = CardType.Creature,
                Rarity = rarity,
                CreatedAt = DateTimeOffset.UtcNow
            });
}

internal sealed class BoosterPackApiFactory(string databaseName) : WebApplicationFactory<Program>
{
    private readonly DbConnection connection = CreateOpenConnection(databaseName);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "0123456789abcdef0123456789abcdef",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.RemoveAll<DbConnection>();
            services.AddSingleton(connection);
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                options.DefaultScheme = TestAuthHandler.SchemeName;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            connection.Dispose();
    }

    private static DbConnection CreateOpenConnection(string databaseName)
    {
        var sqliteConnection = new SqliteConnection($"Data Source={databaseName};Mode=Memory;Cache=Shared");
        sqliteConnection.Open();
        return sqliteConnection;
    }
}

internal sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    System.Text.Encodings.Web.UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";
    public const string UserIdHeaderName = "X-Test-UserId";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(UserIdHeaderName, out var userIdValues)
            || !Guid.TryParse(userIdValues.SingleOrDefault(), out var userId))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing test user id header."));
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}