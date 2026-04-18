namespace TcgApi.Data.Models;

public enum CardType
{
    Deity,
    Spirit,
    Creature,
    Ritual,
    Place,
    Artifact,
    Person
}

public enum CardRarity
{
    Common,
    Uncommon,
    Rare,
    Legendary
}

public class Card
{
    public Guid Id { get; set; }
    public Guid CollectionId { get; set; }
    public int Number { get; set; }
    public string Name { get; set; } = string.Empty;
    public CardType Type { get; set; }
    public CardRarity Rarity { get; set; }
    public string? FlavorText { get; set; }
    public string? ArtUrl { get; set; }
    public string? ArtistCredit { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public Collection Collection { get; set; } = null!;
    public ICollection<UserCard> UserCards { get; set; } = [];
    public ICollection<BoosterPackCard> BoosterPackCards { get; set; } = [];
}
