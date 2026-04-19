namespace TcgApi.Data.Models;

public class BoosterPackOpen
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CollectionId { get; set; }
    public DateTimeOffset OpenedAt { get; set; }
}
