using Microsoft.EntityFrameworkCore;
using TcgApi.Data.Models;

namespace TcgApi.Data.Repositories;

public class WaitlistRepository(AppDbContext db)
{
    public Task<bool> ExistsByEmailAsync(string email)
        => db.WaitlistEntries.AnyAsync(w => w.Email == email);

    public async Task<WaitlistEntry> AddAsync(string email)
    {
        var entry = new WaitlistEntry { Email = email };
        db.WaitlistEntries.Add(entry);
        await db.SaveChangesAsync();
        return entry;
    }

    public async Task<List<WaitlistEntry>> GetAllAsync()
        => await db.WaitlistEntries.OrderBy(w => w.SignedUpAt).ToListAsync();

    public async Task<WaitlistEntry?> GetByIdAsync(Guid id)
        => await db.WaitlistEntries.FindAsync(id);

    public async Task RemoveAsync(WaitlistEntry entry)
    {
        db.WaitlistEntries.Remove(entry);
        await db.SaveChangesAsync();
    }
}
