using Microsoft.EntityFrameworkCore;
using TcgApi.Data.Models;

namespace TcgApi.Data.Repositories;

public class UserRepository(AppDbContext db)
{
    public Task<User?> GetByEmailAsync(string email)
        => db.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<object?> GetPackStatusByEmailAsync(string email)
        => await db.Users
            .Where(u => u.Email == email)
            .Select(u => new
            {
                u.BoosterPacksAvailable,
                u.LoginStreak,
                u.LastLoginDate
            })
            .FirstOrDefaultAsync();

    public Task<int> GetTotalPacksOpenedAsync(Guid userId)
        => db.BoosterPackOpens.CountAsync(p => p.UserId == userId);

    public Task<int> GetTotalCardsCollectedAsync(Guid userId)
        => db.UserCards.CountAsync(uc => uc.UserId == userId);

    public async Task SaveChangesAsync()
        => await db.SaveChangesAsync();
}
