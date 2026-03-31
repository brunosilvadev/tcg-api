using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TcgApi.Data;
using TcgApi.Models;
using TcgApi.Models.Requests;

namespace TcgApi.Endpoints;

public class WaitlistEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/waitlist");

        group.MapPost("/", async (
            [FromBody] JoinWaitlistRequest request,
            AppDbContext db) =>
        {
            if (await db.WaitlistEntries.AnyAsync(w => w.Email == request.Email))
                return Results.Conflict(new { message = "Email already on the waitlist." });

            var entry = new WaitlistEntry { Email = request.Email };
            db.WaitlistEntries.Add(entry);
            await db.SaveChangesAsync();

            return Results.Created($"/waitlist/{entry.Id}", entry);
        });

        group.MapGet("/", async (AppDbContext db) =>
            Results.Ok(await db.WaitlistEntries.OrderBy(w => w.SignedUpAt).ToListAsync())
        ).RequireAuthorization();

        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var entry = await db.WaitlistEntries.FindAsync(id);
            if (entry is null) return Results.NotFound();

            db.WaitlistEntries.Remove(entry);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();
    }
}
