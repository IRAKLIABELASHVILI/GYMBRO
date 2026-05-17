using Microsoft.EntityFrameworkCore;
using Training_APP.Data;
using Training_APP.Model;

namespace Training_APP.Service
{
    public class NutritionService
    {
        private readonly AppDbContext _db;

        public NutritionService(AppDbContext db)
        {
            _db = db;
        }

        // დღის ყველა საჭმელი
        public async Task<List<FoodEntry>> GetTodayEntriesAsync()
        {
            return await _db.FoodEntries
                .Where(f => f.Date == DateTime.Today)
                .ToListAsync();
        }

        // საჭმლის შენახვა
        public async Task SaveFoodEntryAsync(FoodEntry entry)
        {
            _db.FoodEntries.Add(entry);
            await _db.SaveChangesAsync();
        }

        // საჭმლის წაშლა
        public async Task DeleteFoodEntryAsync(int id)
        {
            var entry = await _db.FoodEntries.FindAsync(id);
            if (entry != null)
            {
                _db.FoodEntries.Remove(entry);
                await _db.SaveChangesAsync();
            }
        }

        // დღის DailyLog აწყობა
        public async Task<DailyLog> GetTodayLogAsync()
        {
            var foodEntries = await GetTodayEntriesAsync();
            var workoutEntries = await _db.WorkoutEntries
                .Where(w => w.Date == DateTime.Today)
                .ToListAsync();

            return new DailyLog
            {
                Date = DateTime.Today,
                FoodEntries = foodEntries,
                WorkoutEntries = workoutEntries
            };
        }


        public async Task UpdateFoodEntryAsync(FoodEntry updated)
        {
            var existing = await _db.FoodEntries.FindAsync(updated.Id);
            if (existing != null)
            {
                existing.FoodName = updated.FoodName;
                existing.MealType = updated.MealType;
                existing.Calories = updated.Calories;
                existing.Protein = updated.Protein;
                existing.Carbohydrates = updated.Carbohydrates;
                existing.Fats = updated.Fats;
                existing.Fiber = updated.Fiber;
                existing.Sugar = updated.Sugar;
                existing.Sodium = updated.Sodium;
                existing.Calcium = updated.Calcium;
                existing.Iron = updated.Iron;
                existing.VitaminC = updated.VitaminC;
                existing.VitaminD = updated.VitaminD;
                existing.Water = updated.Water;

                await _db.SaveChangesAsync();
            }
        }
    }
}