using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Training_APP.Data;
using Training_APP.Model;

namespace Training_APP.Service
{
    public class FoodCacheService
    {
        // ── Normalise food name for lookup ────────────────────────────
        private static string Normalise(string name)
            => name.Trim().ToLowerInvariant();

        // ── Look up cached nutrition and scale to requested grams ─────
        /// <returns>
        /// A ready-to-save <see cref="FoodEntry"/> if the food is cached;
        /// <c>null</c> if we have never seen this food before.
        /// </returns>
        public async Task<FoodEntry?> GetCachedEntryAsync(
            string foodName, int grams, string mealType)
        {
            using var db = new AppDbContext();
            string key   = Normalise(foodName);
            var cached   = await db.FoodCache
                                   .FirstOrDefaultAsync(f => f.FoodKey == key);
            if (cached == null) return null;

            // Update usage stats (fire-and-forget is fine here)
            cached.LastUsed = DateTime.Now;
            cached.UseCount++;
            await db.SaveChangesAsync();

            double scale = grams / 100.0;
            return new FoodEntry
            {
                FoodName      = cached.DisplayName,
                MealType      = mealType,
                Date          = DateTime.Today,
                Calories      = (int)Math.Round(cached.CaloriesPer100g  * scale),
                Protein       = Math.Round(cached.ProteinPer100g   * scale, 1),
                Carbohydrates = Math.Round(cached.CarbsPer100g     * scale, 1),
                Fats          = Math.Round(cached.FatsPer100g      * scale, 1),
                Fiber         = Math.Round(cached.FiberPer100g     * scale, 1),
                Sugar         = Math.Round(cached.SugarPer100g     * scale, 1),
                Sodium        = Math.Round(cached.SodiumPer100g    * scale, 1),
                Calcium       = Math.Round(cached.CalciumPer100g   * scale, 1),
                Iron          = Math.Round(cached.IronPer100g      * scale, 1),
                VitaminC      = Math.Round(cached.VitaminCPer100g  * scale, 1),
                VitaminD      = Math.Round(cached.VitaminDPer100g  * scale, 1),
                Water         = Math.Round(cached.WaterPer100g     * scale, 1)
            };
        }

        // ── Save a new AI result to the cache (per-100 g) ─────────────
        public async Task SaveToCacheAsync(FoodEntry entry, int grams)
        {
            if (grams <= 0) return;

            using var db = new AppDbContext();
            string key   = Normalise(entry.FoodName);
            double scale = 100.0 / grams;   // convert entry values to per-100 g

            var existing = await db.FoodCache.FirstOrDefaultAsync(f => f.FoodKey == key);

            if (existing != null)
            {
                ApplyNutrition(existing, entry, scale);
                existing.LastUsed = DateTime.Now;
                existing.UseCount++;
            }
            else
            {
                var row = new FoodCache { FoodKey = key, UseCount = 1, LastUsed = DateTime.Now };
                ApplyNutrition(row, entry, scale);
                db.FoodCache.Add(row);
            }

            try { await db.SaveChangesAsync(); }
            catch { /* swallow unique-key race condition — cache miss is harmless */ }
        }

        // ── Copies nutritional values from a FoodEntry into a FoodCache row ──
        private static void ApplyNutrition(FoodCache row, FoodEntry entry, double scale)
        {
            row.DisplayName     = entry.FoodName;
            row.CaloriesPer100g = entry.Calories      * scale;
            row.ProteinPer100g  = entry.Protein       * scale;
            row.CarbsPer100g    = entry.Carbohydrates * scale;
            row.FatsPer100g     = entry.Fats          * scale;
            row.FiberPer100g    = entry.Fiber         * scale;
            row.SugarPer100g    = entry.Sugar         * scale;
            row.SodiumPer100g   = entry.Sodium        * scale;
            row.CalciumPer100g  = entry.Calcium       * scale;
            row.IronPer100g     = entry.Iron          * scale;
            row.VitaminCPer100g = entry.VitaminC      * scale;
            row.VitaminDPer100g = entry.VitaminD      * scale;
            row.WaterPer100g    = entry.Water         * scale;
        }
    }
}
