namespace TcgApi.Data.Models;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int BoosterPacksAvailable { get; set; }
    public int LoginStreak { get; set; }
    public DateOnly? LastLoginDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<UserCard> UserCards { get; set; } = [];
    public ICollection<BoosterPackOpen> BoosterPackOpens { get; set; } = [];
    public WaitlistEntry? WaitlistEntry { get; set; }
}
