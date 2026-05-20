using Microsoft.EntityFrameworkCore;
using Training_APP.Data;
using Training_APP.Model;

namespace Training_APP.Service
{
    public class NutritionService
    {
        // Each method creates its own short-lived context so concurrent
        // async calls never share a DbContext instance.

        // Water-only entries use this special meal type so they stay out of food lists
        private const string WaterMealType = "💧 Water";

        public async Task<List<FoodEntry>> GetTodayEntriesAsync()
        {
            using var db = new AppDbContext();
            return await db.FoodEntries
                .AsNoTracking()
                .Where(f => f.Date == DateTime.Today && f.MealType != WaterMealType)
                .ToListAsync();
        }

        /// <summary>Logs a glass / bottle of water. Stored as a zero-calorie food entry.</summary>
        public async Task LogWaterAsync(int ml)
        {
            using var db = new AppDbContext();
            db.FoodEntries.Add(new FoodEntry
            {
                FoodName = "Water",
                MealType = WaterMealType,
                Date     = DateTime.Today,
                Water    = ml
            });
            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Counts how many consecutive days (ending today or yesterday) the user
        /// logged at least one real food entry.
        /// </summary>
        public async Task<int> GetCurrentStreakAsync()
        {
            using var db = new AppDbContext();
            var loggedDates = await db.FoodEntries
                .AsNoTracking()
                .Where(f => f.MealType != WaterMealType)
                .Select(f => f.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToListAsync();

            if (loggedDates.Count == 0) return 0;

            // Start from today; if nothing today yet, start from yesterday
            var check = loggedDates[0] == DateTime.Today
                ? DateTime.Today
                : DateTime.Today.AddDays(-1);

            int streak = 0;
            foreach (var date in loggedDates)
            {
                if (date == check) { streak++; check = check.AddDays(-1); }
                else if (date < check) break;
            }
            return streak;
        }

        public async Task SaveFoodEntryAsync(FoodEntry entry)
        {
            using var db = new AppDbContext();
            db.FoodEntries.Add(entry);
            await db.SaveChangesAsync();
        }

        public async Task DeleteFoodEntryAsync(int id)
        {
            using var db = new AppDbContext();
            var entry = await db.FoodEntries.FindAsync(id);
            if (entry != null)
            {
                db.FoodEntries.Remove(entry);
                await db.SaveChangesAsync();
            }
        }

        public async Task<DailyLog> GetTodayLogAsync()
        {
            using var db = new AppDbContext();

            var allFood = await db.FoodEntries
                .AsNoTracking()
                .Where(f => f.Date == DateTime.Today)
                .ToListAsync();

            // Separate water logs from real food so they don't pollute the meal lists
            var foodEntries   = allFood.Where(f => f.MealType != WaterMealType).ToList();
            var waterLoggedMl = allFood
                .Where(f => f.MealType == WaterMealType)
                .Sum(f => f.Water);

            var workoutEntries = await db.WorkoutEntries
                .AsNoTracking()
                .Where(w => w.Date == DateTime.Today)
                .ToListAsync();

            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync();

            var log = new DailyLog
            {
                Date           = DateTime.Today,
                FoodEntries    = foodEntries,
                WorkoutEntries = workoutEntries,
                WaterLoggedMl  = waterLoggedMl
            };

            if (user != null && user.CalorieGoal > 0)
            {
                log.CalorieGoal = user.CalorieGoal;
                log.ProteinGoal = user.ProteinGoal;
                log.CarbsGoal   = user.CarbsGoal;
                log.FatsGoal    = user.FatsGoal;
            }

            return log;
        }

        /// <summary>Deletes food entries older than <paramref name="keepDays"/> days.</summary>
        public async Task PurgeOldEntriesAsync(int keepDays = 7)
        {
            using var db = new AppDbContext();
            var cutoff   = DateTime.Today.AddDays(-keepDays);
            await db.FoodEntries.Where(f => f.Date < cutoff).ExecuteDeleteAsync();
        }

        public async Task UpdateFoodEntryAsync(FoodEntry updated)
        {
            using var db = new AppDbContext();
            var existing = await db.FoodEntries.FindAsync(updated.Id);
            if (existing == null) return;

            db.Entry(existing).CurrentValues.SetValues(updated);
            await db.SaveChangesAsync();
        }
    }
}
