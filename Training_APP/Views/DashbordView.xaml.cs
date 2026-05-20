using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Training_APP.Views
{
    public partial class DashbordView : UserControl
    {
        public DashbordView()
        {
            InitializeComponent();
            SetGreetingAsync();
            LoadDataAsync();
        }

        /// <summary>Called by MainWindow when the user navigates back to Dashboard.</summary>
        public void Refresh() => LoadDataAsync();

        // ── Greeting ──────────────────────────────────────────────────
        private async void SetGreetingAsync()
        {
            int    hour     = DateTime.Now.Hour;
            string greeting = hour < 12 ? "Good morning" : hour < 17 ? "Good afternoon" : "Good evening";
            DateText.Text   = DateTime.Today.ToString("dddd, MMMM d");

            var user = await App.UserService.GetUserAsync();
            string name = user?.Name ?? "";
            GreetingText.Text = string.IsNullOrEmpty(name) ? $"{greeting}!" : $"{greeting}, {name}!";
        }

        // ── Main data load ────────────────────────────────────────────
        private async void LoadDataAsync()
        {
            try
            {
                var log = await App.NutritionService.GetTodayLogAsync();
                UpdateStatCards(log);
                UpdateCalorieProgress(log);
                UpdateMacros(log);
                UpdateRecentLists(log);
                UpdateWater(log);

                int streak = await App.NutritionService.GetCurrentStreakAsync();
                UpdateStreak(streak);
            }
            catch { /* DB might not be ready on first launch */ }
        }

        // ── Section updaters ──────────────────────────────────────────
        private void UpdateStatCards(Model.DailyLog log)
        {
            bool overGoal = log.NetCalories > log.CalorieGoal;
            var  redBrush = new SolidColorBrush(Color.FromRgb(255, 80, 80));

            CaloriesConsumedText.Text  = log.TotalCalories.ToString("N0");
            CaloriesBurnedText.Text    = log.TotalCaloriesBurned.ToString("N0");
            WorkoutCountText.Text      = log.WorkoutEntries.Count.ToString();
            NetCaloriesText.Text       = log.NetCalories.ToString("N0");
            NetCaloriesGoalText.Text   = $"goal: {log.CalorieGoal:N0} kcal";
            NetCaloriesText.Foreground = overGoal ? redBrush : (Brush)FindResource("TextPrimaryBrush");
        }

        private void UpdateCalorieProgress(Model.DailyLog log)
        {
            bool overGoal = log.TotalCalories > log.CalorieGoal;

            CaloriesRemainingText.Text = Math.Max(0, log.CalorieGoal - log.TotalCalories).ToString("N0");
            CalorieBar.Maximum         = log.CalorieGoal;
            CalorieBar.Value           = Math.Min(log.TotalCalories, log.CalorieGoal);
            CalorieBar.Foreground      = overGoal
                ? new SolidColorBrush(Color.FromRgb(255, 80, 80))
                : (Brush)FindResource("AccentAmberBrush");
            CaloriesEatenLabel.Text    = $"{log.TotalCalories:N0} eaten";
            CaloriesGoalLabel.Text     = $"Goal: {log.CalorieGoal:N0}";
            FormulaEatenText.Text      = $"{log.TotalCalories:N0}";
            FormulaBurnedText.Text     = $"{log.TotalCaloriesBurned:N0}";
            FormulaRemainingText.Text  = $"{log.NetCalories:N0}";
        }

        private void UpdateMacros(Model.DailyLog log)
        {
            ProteinText.Text     = $"{log.TotalProtein:F0}g";
            CarbsText.Text       = $"{log.TotalCarbs:F0}g";
            FatsText.Text        = $"{log.TotalFats:F0}g";
            ProteinGoalText.Text = $" / {log.ProteinGoal:F0}g";
            CarbsGoalText.Text   = $" / {log.CarbsGoal:F0}g";
            FatsGoalText.Text    = $" / {log.FatsGoal:F0}g";

            ProteinBar.Maximum = log.ProteinGoal; ProteinBar.Value = Math.Min(log.TotalProtein, log.ProteinGoal);
            CarbsBar.Maximum   = log.CarbsGoal;   CarbsBar.Value   = Math.Min(log.TotalCarbs,   log.CarbsGoal);
            FatsBar.Maximum    = log.FatsGoal;     FatsBar.Value    = Math.Min(log.TotalFats,    log.FatsGoal);
        }

        private void UpdateRecentLists(Model.DailyLog log)
        {
            RecentFoodList.ItemsSource    = log.FoodEntries;
            RecentWorkoutList.ItemsSource = log.WorkoutEntries;
            NoFoodText.Visibility    = log.FoodEntries.Count    == 0 ? Visibility.Visible : Visibility.Collapsed;
            NoWorkoutText.Visibility = log.WorkoutEntries.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateWater(Model.DailyLog log)
        {
            double ml = log.WaterLoggedMl;
            WaterText.Text = ml >= 1000 ? $"{ml / 1000.0:F1} L" : $"{ml:F0} ml";
            WaterBar.Maximum = 2500;
            WaterBar.Value   = Math.Min(ml, 2500);
        }

        private void UpdateStreak(int streak)
        {
            StreakText.Text    = streak.ToString();
            StreakSubText.Text = streak == 1 ? "day logged — keep it up!" : "consecutive days logged";
        }

        // ── Water quick-add ───────────────────────────────────────────
        private async void Water_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out int ml))
            {
                await App.NutritionService.LogWaterAsync(ml);
                LoadDataAsync();
            }
        }
    }
}
