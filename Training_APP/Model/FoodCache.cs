using System;

namespace Training_APP.Model
{
    /// <summary>
    /// Stores per-100 g nutritional values so we never need to call AI
    /// twice for the same food. The entry is keyed on a normalised food name
    /// (lowercase, trimmed).  When a user asks for X grams we scale on-the-fly.
    /// </summary>
    public class FoodCache
    {
        public int    Id          { get; set; }
        public string FoodKey     { get; set; } = "";   // normalised lookup key
        public string DisplayName { get; set; } = "";   // pretty name shown in UI

        // ── Per-100 g values ──────────────────────────────────────────
        public double CaloriesPer100g  { get; set; }
        public double ProteinPer100g   { get; set; }
        public double CarbsPer100g     { get; set; }
        public double FatsPer100g      { get; set; }
        public double FiberPer100g     { get; set; }
        public double SugarPer100g     { get; set; }
        public double SodiumPer100g    { get; set; }
        public double CalciumPer100g   { get; set; }
        public double IronPer100g      { get; set; }
        public double VitaminCPer100g  { get; set; }
        public double VitaminDPer100g  { get; set; }
        public double WaterPer100g     { get; set; }

        public DateTime LastUsed { get; set; } = DateTime.Now;
        public int      UseCount { get; set; } = 1;
    }
}
