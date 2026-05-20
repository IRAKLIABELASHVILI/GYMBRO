using Microsoft.EntityFrameworkCore;
using Training_APP.Data;
using Training_APP.Model;

namespace Training_APP.Service
{
    public class WorkoutService
    {
        public async Task<List<WorkoutEntry>> GetTodayEntriesAsync()
        {
            using var db = new AppDbContext();
            return await db.WorkoutEntries
                .AsNoTracking()
                .Where(w => w.Date == DateTime.Today)
                .ToListAsync();
        }

        public async Task SaveWorkoutEntryAsync(WorkoutEntry entry)
        {
            using var db = new AppDbContext();
            db.WorkoutEntries.Add(entry);
            await db.SaveChangesAsync();
        }

        public async Task DeleteWorkoutEntryAsync(int id)
        {
            using var db = new AppDbContext();
            var entry = await db.WorkoutEntries.FindAsync(id);
            if (entry != null)
            {
                db.WorkoutEntries.Remove(entry);
                await db.SaveChangesAsync();
            }
        }

        public async Task UpdateWorkoutEntryAsync(WorkoutEntry updated)
        {
            using var db = new AppDbContext();
            var existing = await db.WorkoutEntries.FindAsync(updated.Id);
            if (existing != null)
            {
                existing.WorkoutName     = updated.WorkoutName;
                existing.WorkoutType     = updated.WorkoutType;
                existing.DurationMinutes = updated.DurationMinutes;
                existing.CaloriesBurned  = updated.CaloriesBurned;
                existing.Intensity       = updated.Intensity;
                existing.Notes           = updated.Notes;
                await db.SaveChangesAsync();
            }
        }

        public async Task SavePlanAsync(Model.WorkoutPlan plan)
        {
            using var db = new AppDbContext();
            var existing = await db.WorkoutPlans.FirstOrDefaultAsync();
            if (existing == null)
            {
                db.WorkoutPlans.Add(plan);
            }
            else
            {
                existing.PlanText    = plan.PlanText;
                existing.Location    = plan.Location;
                existing.Goal        = plan.Goal;
                existing.Equipment   = plan.Equipment;
                existing.DaysPerWeek = plan.DaysPerWeek;
                existing.GeneratedAt = plan.GeneratedAt;
            }
            await db.SaveChangesAsync();
        }

        public async Task<Model.WorkoutPlan?> GetSavedPlanAsync()
        {
            using var db = new AppDbContext();
            return await db.WorkoutPlans.AsNoTracking().FirstOrDefaultAsync();
        }

        /// <summary>Deletes workout entries older than <paramref name="keepDays"/> days.</summary>
        public async Task PurgeOldEntriesAsync(int keepDays = 7)
        {
            using var db = new AppDbContext();
            var cutoff   = DateTime.Today.AddDays(-keepDays);
            await db.WorkoutEntries.Where(w => w.Date < cutoff).ExecuteDeleteAsync();
        }

        /// <summary>Returns workout entries for the last N days (default 7), newest date first.</summary>
        public async Task<List<WorkoutEntry>> GetWeekHistoryAsync(int days = 7)
        {
            using var db = new AppDbContext();
            var cutoff = DateTime.Today.AddDays(-(days - 1));
            return await db.WorkoutEntries
                .AsNoTracking()
                .Where(w => w.Date >= cutoff)
                .OrderByDescending(w => w.Date)
                .ThenBy(w => w.WorkoutName)
                .ToListAsync();
        }
    }
}
