using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TcgApi.Data;
using TcgApi.Data.Models;
using TcgApi.Data.Models.Requests;
using TcgApi.Data.Repositories;

namespace TcgApi.Endpoints;

public class AuthEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register", async (
            [FromBody] RegisterRequest request,
            AppDbContext db,
            IConfiguration config) =>
        {
            if (await db.Users.AnyAsync(u => u.Email == request.Email))
                return Results.Conflict(new { message = "Email already in use." });

            if (await db.Users.AnyAsync(u => u.Username == request.Username))
                return Results.Conflict(new { message = "Username already in use." });

            var user = new User
            {
                Email = request.Email,
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                BoosterPacksAvailable = 1,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await using var transaction = await db.Database.BeginTransactionAsync();
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var response = GenerateTokens(user, db, config);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Results.Created($"/users/{user.Id}", response);
        });

        app.MapPost("/auth/login", async (
            [FromBody] LoginRequest request,
            AppDbContext db,
            DailyTaskRepository dailyTasks,
            IConfiguration config) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Results.Unauthorized();

            var response = GenerateTokens(user, db, config);
            await db.SaveChangesAsync();

            var gemResult = await dailyTasks.CompleteTaskAsync(user.Id, DailyTaskRepository.TaskLogin);

            return Results.Ok(new
            {
                response.AccessToken,
                response.RefreshToken,
                response.ExpiresAt,
                GemAwarded = gemResult.WasNew,
                Gems = gemResult.NewGemBalance,
                PackAwarded = gemResult.PackAwarded
            });
        });

        app.MapPost("/auth/refresh", async (
            [FromBody] RefreshTokenRequest request,
            AppDbContext db,
            IConfiguration config) =>
        {
            var stored = await db.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == request.RefreshToken);

            if (stored is null || stored.IsRevoked || stored.ExpiresAt <= DateTimeOffset.UtcNow)
                return Results.Unauthorized();

            stored.IsRevoked = true;

            var response = GenerateTokens(stored.User, db, config);
            await db.SaveChangesAsync();

            return Results.Ok(response);
        });
    }

    private static AuthResponse GenerateTokens(User user, AppDbContext db, IConfiguration config)
    {
        var jwtKey = config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        var jwtIssuer = config["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
        var jwtAudience = config["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience is not configured.");
        var accessTokenMinutes = int.Parse(config["Jwt:AccessTokenExpiryMinutes"] ?? "15");
        var refreshTokenDays = int.Parse(config["Jwt:RefreshTokenExpiryDays"] ?? "30");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTimeOffset.UtcNow.AddMinutes(accessTokenMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: expiry.UtcDateTime,
            signingCredentials: creds);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        var refreshTokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(refreshTokenDays),
            CreatedAt = DateTimeOffset.UtcNow
        });

        return new AuthResponse(accessToken, refreshTokenValue, expiry);
    }
}
