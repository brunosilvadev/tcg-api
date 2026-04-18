using Microsoft.EntityFrameworkCore;
using TcgApi.Data.Models;

namespace TcgApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Collection> Collections { get; set; } = null!;
    public DbSet<Card> Cards { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserCard> UserCards { get; set; } = null!;
    public DbSet<BoosterPackOpen> BoosterPackOpens { get; set; } = null!;
    public DbSet<BoosterPackCard> BoosterPackCards { get; set; } = null!;
    public DbSet<DailyFact> DailyFacts { get; set; } = null!;
    public DbSet<WaitlistEntry> WaitlistEntries { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Collection>(e =>
        {
            e.Property(c => c.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(c => c.Name).HasMaxLength(100);
            e.Property(c => c.Slug).HasMaxLength(100);
            e.HasIndex(c => c.Slug).IsUnique();
        });

        modelBuilder.Entity<Card>(e =>
        {
            e.Property(c => c.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(c => c.Name).HasMaxLength(100);
            e.Property(c => c.ArtUrl).HasMaxLength(500);
            e.Property(c => c.ArtistCredit).HasMaxLength(100);
            e.Property(c => c.Type).HasConversion<string>();
            e.Property(c => c.Rarity).HasConversion<string>();
            e.HasIndex(c => c.CollectionId);
            e.HasIndex(c => c.Rarity);
            e.HasIndex(c => new { c.CollectionId, c.Number }).IsUnique();
            e.HasOne(c => c.Collection)
                .WithMany(col => col.Cards)
                .HasForeignKey(c => c.CollectionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.Property(u => u.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(u => u.Email).HasMaxLength(255);
            e.Property(u => u.Username).HasMaxLength(50);
            e.Property(u => u.PasswordHash).HasMaxLength(255);
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.Username).IsUnique();
        });

        modelBuilder.Entity<UserCard>(e =>
        {
            e.Property(uc => uc.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(uc => uc.Quantity).HasDefaultValue(1);
            e.ToTable(t => t.HasCheckConstraint("CK_user_cards_quantity", "quantity > 0"));
            e.HasIndex(uc => uc.UserId);
            e.HasIndex(uc => uc.CardId);
            e.HasIndex(uc => new { uc.UserId, uc.CardId }).IsUnique();
            e.HasOne(uc => uc.User)
                .WithMany(u => u.UserCards)
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(uc => uc.Card)
                .WithMany(c => c.UserCards)
                .HasForeignKey(uc => uc.CardId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BoosterPackOpen>(e =>
        {
            e.Property(b => b.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasIndex(b => b.UserId);
            e.HasOne(b => b.User)
                .WithMany(u => u.BoosterPackOpens)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(b => b.Collection)
                .WithMany(col => col.BoosterPackOpens)
                .HasForeignKey(b => b.CollectionId);
        });

        modelBuilder.Entity<BoosterPackCard>(e =>
        {
            e.Property(b => b.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasIndex(b => b.OpenId);
            e.HasOne(b => b.BoosterPackOpen)
                .WithMany(o => o.BoosterPackCards)
                .HasForeignKey(b => b.OpenId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(b => b.Card)
                .WithMany(c => c.BoosterPackCards)
                .HasForeignKey(b => b.CardId);
        });

        modelBuilder.Entity<DailyFact>(e =>
        {
            e.Property(d => d.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasIndex(d => d.FactDate);
            e.HasIndex(d => d.CollectionId);
            e.HasIndex(d => new { d.CollectionId, d.FactDate }).IsUnique();
            e.HasOne(d => d.Collection)
                .WithMany(col => col.DailyFacts)
                .HasForeignKey(d => d.CollectionId);
        });

        modelBuilder.Entity<WaitlistEntry>(e =>
        {
            e.Property(w => w.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(w => w.Email).HasMaxLength(255);
            e.HasIndex(w => w.Email).IsUnique();
            e.HasOne(w => w.User)
                .WithOne(u => u.WaitlistEntry)
                .HasForeignKey<WaitlistEntry>(w => w.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.Property(r => r.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(r => r.Token).HasMaxLength(512);
            e.HasIndex(r => r.Token).IsUnique();
            e.HasIndex(r => r.UserId);
            e.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

