using Microsoft.EntityFrameworkCore;
using Training_APP.Model;

namespace Training_APP.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<FoodEntry> FoodEntries { get; set; }
        public DbSet<WorkoutEntry> WorkoutEntries { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer(
                @"Server=ABELA-PC\SQLEXPRESS01;Database=CalorieMaster;Trusted_Connection=True;TrustServerCertificate=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("UserProfile");
            modelBuilder.Entity<FoodEntry>().ToTable("FoodEntries");
            modelBuilder.Entity<WorkoutEntry>().ToTable("WorkoutEntries");
        }
    }
}