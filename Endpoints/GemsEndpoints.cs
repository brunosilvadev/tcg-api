using System.Security.Claims;
using TcgApi.Data.Repositories;

namespace TcgApi.Endpoints;

public class GemsEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/gems/status", async (
            ClaimsPrincipal principal,
            DailyTaskRepository dailyTasks) =>
        {
            if (!Guid.TryParse(principal.FindFirstValue("sub"), out var userId))
                return Results.Unauthorized();

            var status = await dailyTasks.GetDailyStatusAsync(userId);

            return Results.Ok(new
            {
                status.Gems,
                status.GemsForNextPack,
                status.GemsNeeded,
                DailyTasks = new
                {
                    status.Login,
                    status.ViewCard,
                    status.ClickLink
                }
            });
        }).RequireAuthorization();

        app.MapPost("/gems/tasks/view-card", async (
            ClaimsPrincipal principal,
            DailyTaskRepository dailyTasks) =>
        {
            if (!Guid.TryParse(principal.FindFirstValue("sub"), out var userId))
                return Results.Unauthorized();

            var result = await dailyTasks.CompleteTaskAsync(userId, DailyTaskRepository.TaskViewCard);

            return Results.Ok(new
            {
                result.WasNew,
                Gems = result.NewGemBalance,
                result.PackAwarded
            });
        }).RequireAuthorization();

        app.MapPost("/gems/tasks/click-link", async (
            ClaimsPrincipal principal,
            DailyTaskRepository dailyTasks) =>
        {
            if (!Guid.TryParse(principal.FindFirstValue("sub"), out var userId))
                return Results.Unauthorized();

            var result = await dailyTasks.CompleteTaskAsync(userId, DailyTaskRepository.TaskClickLink);

            return Results.Ok(new
            {
                result.WasNew,
                Gems = result.NewGemBalance,
                result.PackAwarded
            });
        }).RequireAuthorization();
    }
}
