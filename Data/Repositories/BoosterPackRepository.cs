using Microsoft.EntityFrameworkCore;
using TcgApi.Data.Models;

namespace TcgApi.Data.Repositories;

public class BoosterPackRepository(AppDbContext db)
{
    public void AddPackOpen(BoosterPackOpen packOpen)
        => db.BoosterPackOpens.Add(packOpen);

    public void AddPackCard(BoosterPackCard packCard)
        => db.BoosterPackCards.Add(packCard);

    public async Task<UserCard?> GetUserCardAsync(Guid userId, Guid cardId)
        => await db.UserCards
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CardId == cardId);

    public void AddUserCard(UserCard userCard)
        => db.UserCards.Add(userCard);

    public async Task SaveChangesAsync()
        => await db.SaveChangesAsync();
}
