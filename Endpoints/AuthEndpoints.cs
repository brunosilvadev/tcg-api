using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

namespace TcgApi.Endpoints;

public class AuthEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/auth/login", () =>
            Results.Challenge(
                new AuthenticationProperties { RedirectUri = "/" },
                [GoogleDefaults.AuthenticationScheme]));

        app.MapPost("/auth/logout", async (HttpContext ctx) =>
        {
            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Ok();
        }).RequireAuthorization();

        app.MapGet("/auth/me", (HttpContext ctx) =>
        {
            var claims = ctx.User.Claims.Select(c => new { c.Type, c.Value });
            return Results.Ok(new
            {
                name = ctx.User.Identity?.Name,
                isAuthenticated = ctx.User.Identity?.IsAuthenticated,
                claims
            });
        }).RequireAuthorization();
    }
}
