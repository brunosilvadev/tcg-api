using Microsoft.EntityFrameworkCore;
using TcgApi.Data.Models;

namespace TcgApi.Data.Repositories;

public class DailyTaskRepository(AppDbContext db)
{
    public const int GemsPerPack = 5;

    public const string TaskLogin = "login";
    public const string TaskViewCard = "view_card";
    public const string TaskClickLink = "click_link";

    public async Task<TaskCompletionResult> CompleteTaskAsync(Guid userId, string taskType)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var alreadyDone = await db.UserDailyTasks.AnyAsync(t =>
            t.UserId == userId && t.TaskType == taskType && t.CompletedDate == today);

        if (alreadyDone)
        {
            var user = await db.Users.FindAsync(userId);
            return new TaskCompletionResult(WasNew: false, NewGemBalance: user?.Gems ?? 0, PackAwarded: false);
        }

        await using var transaction = await db.Database.BeginTransactionAsync();

        db.UserDailyTasks.Add(new UserDailyTask
        {
            UserId = userId,
            TaskType = taskType,
            CompletedDate = today
        });

        var targetUser = await db.Users.FindAsync(userId);
        if (targetUser is null)
        {
            await transaction.RollbackAsync();
            return new TaskCompletionResult(WasNew: false, NewGemBalance: 0, PackAwarded: false);
        }

        targetUser.Gems += 1;

        var packAwarded = false;
        if (targetUser.Gems >= GemsPerPack)
        {
            targetUser.Gems -= GemsPerPack;
            targetUser.BoosterPacksAvailable += 1;
            packAwarded = true;
        }

        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        return new TaskCompletionResult(WasNew: true, NewGemBalance: targetUser.Gems, PackAwarded: packAwarded);
    }

    public async Task<DailyTaskStatus> GetDailyStatusAsync(Guid userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var completedToday = await db.UserDailyTasks
            .Where(t => t.UserId == userId && t.CompletedDate == today)
            .Select(t => t.TaskType)
            .ToListAsync();

        var gems = await db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Gems)
            .FirstOrDefaultAsync();

        return new DailyTaskStatus(
            Gems: gems,
            GemsForNextPack: GemsPerPack,
            GemsNeeded: GemsPerPack - gems,
            Login: completedToday.Contains(TaskLogin),
            ViewCard: completedToday.Contains(TaskViewCard),
            ClickLink: completedToday.Contains(TaskClickLink)
        );
    }
}

public record TaskCompletionResult(bool WasNew, int NewGemBalance, bool PackAwarded);

public record DailyTaskStatus(int Gems, int GemsForNextPack, int GemsNeeded, bool Login, bool ViewCard, bool ClickLink);
