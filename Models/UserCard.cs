namespace TcgApi.Models;

public class UserCard
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CardId { get; set; }
    public int Quantity { get; set; }
    public DateTimeOffset FirstObtainedAt { get; set; }

    public User User { get; set; } = null!;
    public Card Card { get; set; } = null!;
}
