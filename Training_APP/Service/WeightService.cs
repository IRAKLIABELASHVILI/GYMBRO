using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Training_APP.Data;
using Training_APP.Model;

namespace Training_APP.Service
{
    public class WeightService
    {
        /// <summary>Log a new weight entry (one per day — upserts today's record).</summary>
        public async Task LogWeightAsync(double weightKg, string? note = null)
        {
            using var db = new AppDbContext();
            var today = DateTime.Today;
            var existing = await db.WeightEntries
                .FirstOrDefaultAsync(w => w.Date == today);

            if (existing != null)
            {
                existing.WeightKg = weightKg;
                existing.Note     = note;
            }
            else
            {
                db.WeightEntries.Add(new WeightEntry
                {
                    WeightKg = weightKg,
                    Date     = today,
                    Note     = note
                });
            }
            await db.SaveChangesAsync();
        }

        /// <summary>Returns weight entries for the last N days, oldest first (for charting).</summary>
        public async Task<List<WeightEntry>> GetRecentAsync(int days = 30)
        {
            using var db = new AppDbContext();
            var cutoff = DateTime.Today.AddDays(-days + 1);
            return await db.WeightEntries
                .AsNoTracking()
                .Where(w => w.Date >= cutoff)
                .OrderBy(w => w.Date)
                .ToListAsync();
        }

        /// <summary>Deletes weight entries older than <paramref name="keepDays"/> days.</summary>
        public async Task PurgeOldEntriesAsync(int keepDays = 30)
        {
            using var db = new AppDbContext();
            var cutoff   = DateTime.Today.AddDays(-keepDays);
            await db.WeightEntries.Where(w => w.Date < cutoff).ExecuteDeleteAsync();
        }

        /// <summary>Returns today's entry or null.</summary>
        public async Task<WeightEntry?> GetTodayAsync()
        {
            using var db = new AppDbContext();
            return await db.WeightEntries
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Date == DateTime.Today);
        }
    }
}
