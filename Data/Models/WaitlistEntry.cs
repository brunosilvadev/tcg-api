namespace TcgApi.Data.Models;

public class WaitlistEntry
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTimeOffset SignedUpAt { get; set; }
}
