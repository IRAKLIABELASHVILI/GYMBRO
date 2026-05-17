using Microsoft.EntityFrameworkCore;
using Training_APP.Data;
using Training_APP.Model;

namespace Training_APP.Service
{
    public class WorkoutService
    {
        private readonly AppDbContext _db;

        public WorkoutService(AppDbContext db)
        {
            _db = db;
        }

        // დღის ვარჯიშები
        public async Task<List<WorkoutEntry>> GetTodayEntriesAsync()
        {
            return await _db.WorkoutEntries
                .Where(w => w.Date == DateTime.Today)
                .ToListAsync();
        }

        // ვარჯიშის შენახვა
        public async Task SaveWorkoutEntryAsync(WorkoutEntry entry)
        {
            _db.WorkoutEntries.Add(entry);
            await _db.SaveChangesAsync();
        }

        // ვარჯიშის წაშლა
        public async Task DeleteWorkoutEntryAsync(int id)
        {
            var entry = await _db.WorkoutEntries.FindAsync(id);
            if (entry != null)
            {
                _db.WorkoutEntries.Remove(entry);
                await _db.SaveChangesAsync();
            }
        }

        // ვარჯიშის რედაქტირება
        public async Task UpdateWorkoutEntryAsync(WorkoutEntry updated)
        {
            var existing = await _db.WorkoutEntries.FindAsync(updated.Id);
            if (existing != null)
            {
                existing.WorkoutName = updated.WorkoutName;
                existing.WorkoutType = updated.WorkoutType;
                existing.DurationMinutes = updated.DurationMinutes;
                existing.CaloriesBurned = updated.CaloriesBurned;
                existing.Intensity = updated.Intensity;
                existing.Notes = updated.Notes;

                await _db.SaveChangesAsync();
            }
        }
    }
}