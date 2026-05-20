using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Training_APP.Data;
using Training_APP.Service;

namespace Training_APP
{
    public partial class App : Application
    {
        // ყველა სერვისი გლობალურია — ნებისმიერი View-იდან ხელმისაწვდომი
        public static UserService      UserService      { get; private set; } = null!;
        public static NutritionService NutritionService { get; private set; } = null!;
        public static WorkoutService   WorkoutService   { get; private set; } = null!;
        public static WeightService    WeightService    { get; private set; } = null!;
        public static FoodCacheService FoodCacheService { get; private set; } = null!;
        public static CloudService     CloudService     { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 0. Load .env file (populates env vars before anything else reads them)
            EnvLoader.Load();

            // 1. Apply saved theme before any window opens
            ThemeService.LoadSaved();

            // 2. Apply EF Core migrations.
            //    Three cases are handled safely:
            //    a) Fresh install   → Migrate() creates DB + all tables
            //    b) Old install     → DB exists but has no migration history
            //                         (was created with EnsureCreated).
            //                         We stamp the history so Migrate() doesn't
            //                         try to re-create tables that already exist.
            //    c) Already migrated → Migrate() applies only new migrations.
            using var db = new AppDbContext();
            try
            {
                if (db.Database.CanConnect())
                {
                    // Stamp InitialCreate as already applied on old EnsureCreated installs
                    db.Database.ExecuteSqlRaw(@"
                        IF NOT EXISTS (
                            SELECT * FROM sysobjects
                            WHERE name='__EFMigrationsHistory' AND xtype='U')
                        BEGIN
                            CREATE TABLE __EFMigrationsHistory (
                                MigrationId    NVARCHAR(150) NOT NULL,
                                ProductVersion NVARCHAR(32)  NOT NULL,
                                CONSTRAINT PK___EFMigrationsHistory
                                    PRIMARY KEY (MigrationId)
                            );
                            INSERT INTO __EFMigrationsHistory VALUES
                                ('20260520141034_InitialCreate', '8.0.0');
                        END");
                }
            }
            catch { /* DB doesn't exist yet — Migrate() will create it */ }

            db.Database.Migrate();

            // 3. Services — each method creates its own short-lived DbContext
            UserService      = new UserService();
            NutritionService = new NutritionService();
            WorkoutService   = new WorkoutService();
            WeightService    = new WeightService();
            FoodCacheService = new FoodCacheService();

            // 4. Claude AI — key is read from .env or real environment variable
            string apiKey = (Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY") ?? "")
                            .Replace("\r", "").Replace("\n", "").Trim();

            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException(
                    "ANTHROPIC_API_KEY is not set. Add it to Training_APP/.env");

            CloudService = new CloudService(apiKey);

            // 5. Purge old data in background (non-blocking)
            //    Meals & workouts: keep 7 days   |   Weight log: keep 30 days
            _ = Task.Run(async () =>
            {
                try
                {
                    await NutritionService.PurgeOldEntriesAsync(keepDays: 7);
                    await WorkoutService.PurgeOldEntriesAsync(keepDays: 7);
                    await WeightService.PurgeOldEntriesAsync(keepDays: 30);
                }
                catch { /* non-critical — app continues normally */ }
            });
        }
    }
}