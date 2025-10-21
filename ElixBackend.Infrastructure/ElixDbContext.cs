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
    
    public DbSet<Quiz> Quizzes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>()
            .Property(u => u.Gender)
            .HasConversion<string>();
    }
}