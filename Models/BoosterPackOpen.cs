namespace TcgApi.Models;

public class BoosterPackOpen
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CollectionId { get; set; }
    public DateTimeOffset OpenedAt { get; set; }

    public User User { get; set; } = null!;
    public Collection Collection { get; set; } = null!;
    public ICollection<BoosterPackCard> BoosterPackCards { get; set; } = [];
}
