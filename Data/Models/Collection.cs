namespace TcgApi.Data.Models;

public class Collection
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TotalCards { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<Card> Cards { get; set; } = [];
    public ICollection<BoosterPackOpen> BoosterPackOpens { get; set; } = [];
    public ICollection<DailyFact> DailyFacts { get; set; } = [];
}
