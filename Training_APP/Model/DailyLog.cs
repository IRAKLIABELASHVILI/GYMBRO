using System;
using System.Collections.Generic;
using System.Text;

namespace Training_APP.Model
{
    public class DailyLog
    {

        public DateTime Date { get; set; } = DateTime.Today;
        public List<FoodEntry> FoodEntries { get; set; } = new();
        public List<WorkoutEntry> WorkoutEntries { get; set; } = new();

        // Water explicitly logged (ml) — set by service, not computed
        public double WaterLoggedMl { get; set; } = 0;

        // მიზნები
        public int CalorieGoal { get; set; } = 2000;
        public double ProteinGoal { get; set; } = 150;
        public double CarbsGoal { get; set; } = 250;
        public double FatsGoal { get; set; } = 65;
        public double WaterGoal { get; set; } = 2500;

        // ჯამები (ავტომატური)
        public int TotalCalories => FoodEntries.Sum(f => f.Calories);
        public double TotalProtein => FoodEntries.Sum(f => f.Protein);
        public double TotalCarbs => FoodEntries.Sum(f => f.Carbohydrates);
        public double TotalFats => FoodEntries.Sum(f => f.Fats);
        public double TotalFiber => FoodEntries.Sum(f => f.Fiber);
        public double TotalSugar => FoodEntries.Sum(f => f.Sugar);
        public double TotalSodium => FoodEntries.Sum(f => f.Sodium);
        public double TotalWater => FoodEntries.Sum(f => f.Water);
        public int TotalCaloriesBurned => WorkoutEntries.Sum(w => w.CaloriesBurned);
        public int NetCalories => TotalCalories - TotalCaloriesBurned;

    }
}
