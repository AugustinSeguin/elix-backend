using Microsoft.EntityFrameworkCore;
using ElixBackend.Domain.Entities;

namespace ElixBackend.Infrastructure
{
    public class ElixDbContext : DbContext
    {
        public ElixDbContext(DbContextOptions<ElixDbContext> options) : base(options) {}

        public DbSet<User> Users { get; set; }
        
        public DbSet<UserToken> UserTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>()
                .Property(u => u.Gender)
                .HasConversion<string>();
        }
    }
}