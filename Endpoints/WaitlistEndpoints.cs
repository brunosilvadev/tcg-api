using Microsoft.AspNetCore.Mvc;
using TcgApi.Data.Repositories;
using TcgApi.Data.Models.Requests;

namespace TcgApi.Endpoints;

public class WaitlistEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/waitlist");

        group.MapPost("", async (
            [FromBody] JoinWaitlistRequest request,
            WaitlistRepository repo) =>
        {
            if (await repo.ExistsByEmailAsync(request.Email))
                return Results.Conflict(new { message = "Email already on the waitlist." });

            var entry = await repo.AddAsync(request.Email);
            return Results.Created($"/waitlist/{entry.Id}", entry);
        });

        group.MapGet("", async (WaitlistRepository repo) =>
            Results.Ok(await repo.GetAllAsync())
        ).RequireAuthorization();

        group.MapDelete("/{id:guid}", async (Guid id, WaitlistRepository repo) =>
        {
            var entry = await repo.GetByIdAsync(id);
            if (entry is null) return Results.NotFound();

            await repo.RemoveAsync(entry);
            return Results.NoContent();
        }).RequireAuthorization();
    }
}
