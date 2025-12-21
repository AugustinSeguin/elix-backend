using Microsoft.EntityFrameworkCore;
using ElixBackend.Domain.Entities;

namespace ElixBackend.Infrastructure;

public class ElixDbContext(DbContextOptions<ElixDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Question> Questions { get; set; }

    public DbSet<Answer> Answers { get; set; }
    public DbSet<UserToken> UserTokens { get; set; }

    public DbSet<Article> Articles { get; set; }
    public DbSet<UserAnswer> UserAnswers { get; set; }

    public DbSet<UserPoint> UserPoints { get; set; }
    public DbSet<Resource> Resources { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Resource>()
            .OwnsOne(r => r.Localization);

        modelBuilder.Entity<User>()
            .Property(u => u.Gender)
            .HasConversion<string>();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}