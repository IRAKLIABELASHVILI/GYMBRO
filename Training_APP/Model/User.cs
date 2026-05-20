using System;

namespace Training_APP.Model
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public double WeightKg { get; set; }
        public double HeightCm { get; set; }
        public string Gender { get; set; } = "";
        public string ActivityLevel { get; set; } = "";
        public string Goal { get; set; } = "";

        // Calculated TDEE-based goals (saved to DB after profile save)
        public int CalorieGoal { get; set; } = 2000;
        public double ProteinGoal { get; set; } = 150;
        public double CarbsGoal { get; set; } = 250;
        public double FatsGoal { get; set; } = 65;

        // ── Mifflin-St Jeor TDEE calculation ─────────────────────────
        public void RecalculateGoals()
        {
            // Step 1: BMR
            double bmr = Gender == "Female"
                ? (10 * WeightKg) + (6.25 * HeightCm) - (5 * Age) - 161
                : (10 * WeightKg) + (6.25 * HeightCm) - (5 * Age) + 5;

            // Step 2: Activity multiplier
            double multiplier = ActivityLevel switch
            {
                "Intermediate" => 1.55,
                "Advanced"     => 1.725,
                _              => 1.2    // Beginner / sedentary
            };

            double tdee = bmr * multiplier;

            // Step 3: Goal adjustment
            double calories = Goal switch
            {
                "Weight Loss"        => tdee - 500,
                "Muscle Gain"        => tdee + 300,
                "Improve Endurance"  => tdee + 200,
                _                    => tdee   // Maintain Weight
            };

            calories = Math.Max(1200, calories); // never go below safe minimum
            CalorieGoal = (int)Math.Round(calories);

            // Step 4: Macro split (varies by goal)
            (double proteinPct, double carbsPct, double fatsPct) = Goal switch
            {
                "Weight Loss"       => (0.35, 0.35, 0.30),
                "Muscle Gain"       => (0.30, 0.45, 0.25),
                "Improve Endurance" => (0.20, 0.55, 0.25),
                _                   => (0.25, 0.50, 0.25)
            };

            ProteinGoal = Math.Round((calories * proteinPct) / 4);  // 4 kcal/g
            CarbsGoal   = Math.Round((calories * carbsPct)   / 4);
            FatsGoal    = Math.Round((calories * fatsPct)    / 9);  // 9 kcal/g
        }
    }
}
