namespace TcgApi.Data.Models;

public class DailyFact
{
    public Guid Id { get; set; }
    public Guid CollectionId { get; set; }
    public DateOnly FactDate { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? SourcePrompt { get; set; }
    public DateTimeOffset GeneratedAt { get; set; }

    public Collection Collection { get; set; } = null!;
}
