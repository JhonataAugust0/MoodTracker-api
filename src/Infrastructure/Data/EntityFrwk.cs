using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<Mood> MoodEntries { get; set; } = null!;
        public DbSet<Habit> Habits { get; set; } = null!;
        public DbSet<HabitCompletion> HabitCompletions { get; set; } = null!;
        public DbSet<QuickNote> QuickNotes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(u => u.Tags)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId);

            modelBuilder.Entity<Mood>()
                .HasMany(m => m.Tags)
                .WithMany();

            modelBuilder.Entity<Habit>()
                .HasMany(h => h.Tags)
                .WithMany();

            modelBuilder.Entity<QuickNote>()
                .HasMany(qn => qn.Tags)
                .WithMany();
        }
    }
}