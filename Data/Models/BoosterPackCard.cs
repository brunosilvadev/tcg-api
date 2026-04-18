namespace TcgApi.Data.Models;

public class BoosterPackCard
{
    public Guid Id { get; set; }
    public Guid OpenId { get; set; }
    public Guid CardId { get; set; }

    public BoosterPackOpen BoosterPackOpen { get; set; } = null!;
    public Card Card { get; set; } = null!;
}
