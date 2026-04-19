namespace TcgApi.Data.Models.Requests;

public record OpenBoosterPackRequest
{
    public Guid? CollectionId { get; init; }
}
