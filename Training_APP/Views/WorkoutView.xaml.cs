using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Training_APP.Model;

namespace Training_APP.Views
{
    public partial class WorkoutView : UserControl
    {
        private string _currentPlan = "";

        public WorkoutView()
        {
            InitializeComponent();
            InitAsync();
        }

        public void Refresh() => _ = LoadWorkoutsAsync();

        private async void InitAsync()
        {
            await LoadSavedPlanAsync();
            await LoadWorkoutsAsync();
        }

        // ── Tab switching ────────────────────────────────────────────
        private async void Tab_Checked(object sender, RoutedEventArgs e)
        {
            if (PlanTab == null) return; // guard during InitializeComponent

            PlanTab.Visibility    = TabPlan.IsChecked    == true ? Visibility.Visible : Visibility.Collapsed;
            LogTab.Visibility     = TabLog.IsChecked     == true ? Visibility.Visible : Visibility.Collapsed;
            HistoryTab.Visibility = TabHistory.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

            if (TabHistory.IsChecked == true)
                await LoadHistoryAsync();
        }

        // ── Load saved plan from DB on open ─────────────────────────
        private async Task LoadSavedPlanAsync()
        {
            var saved = await App.WorkoutService.GetSavedPlanAsync();
            if (saved == null) return;

            _currentPlan  = saved.PlanText;
            PlanText.Text = saved.PlanText;

            SetComboByContent(LocationCombo, saved.Location);
            SetComboByContent(DaysCombo, saved.DaysPerWeek + " days");

            // Restore goal checkboxes
            GoalMuscle.IsChecked    = saved.Goal.Contains("Muscle Gain");
            GoalFatLoss.IsChecked   = saved.Goal.Contains("Fat Loss");
            GoalEndurance.IsChecked = saved.Goal.Contains("Endurance");
            GoalMaintain.IsChecked  = saved.Goal.Contains("Maintenance");

            // Restore equipment
            EquipmentBox.Text = saved.Equipment ?? "";

            PlanDateBadge.Text       = $"Generated {saved.GeneratedAt:MMM d, yyyy}";
            PlanDateBadge.Visibility = Visibility.Visible;
        }

        private static void SetComboByContent(ComboBox combo, string fragment)
        {
            foreach (ComboBoxItem item in combo.Items)
            {
                if (item.Content?.ToString()?.Contains(fragment,
                        StringComparison.OrdinalIgnoreCase) == true)
                {
                    combo.SelectedItem = item;
                    return;
                }
            }
        }

        // ── Load today's workout log ─────────────────────────────────
        private async Task LoadWorkoutsAsync()
        {
            var entries = await App.WorkoutService.GetTodayEntriesAsync();
            WorkoutList.ItemsSource = entries;
            EmptyLogText.Visibility = entries.Count == 0
                ? Visibility.Visible : Visibility.Collapsed;
        }

        // ── Generate AI plan ─────────────────────────────────────────
        private async void GeneratePlan_Click(object sender, RoutedEventArgs e)
        {
            var user = await App.UserService.GetUserAsync();
            if (user == null)
            {
                MessageBox.Show("Please fill in your profile first!", "Gymbro",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string location  = ((ComboBoxItem)LocationCombo.SelectedItem)?.Content.ToString() ?? "";
            string equipment = EquipmentBox.Text.Trim();
            int    days      = int.Parse(((ComboBoxItem)DaysCombo.SelectedItem).Content
                                         .ToString()!.Split(' ')[0]);

            // Collect selected goals
            var selectedGoals = new System.Collections.Generic.List<string>();
            if (GoalMuscle.IsChecked    == true) selectedGoals.Add("Muscle Gain");
            if (GoalFatLoss.IsChecked   == true) selectedGoals.Add("Fat Loss");
            if (GoalEndurance.IsChecked == true) selectedGoals.Add("Endurance");
            if (GoalMaintain.IsChecked  == true) selectedGoals.Add("Maintenance");

            if (selectedGoals.Count == 0)
            {
                MessageBox.Show("Please select at least one goal.", "Gymbro",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string goal = string.Join(" + ", selectedGoals);

            PlanText.Text            = "⏳ Generating your plan...";
            PlanDateBadge.Visibility = Visibility.Collapsed;

            try
            {
                string plan = await App.CloudService.GenerateWorkoutPlanAsync(
                    location, goal, equipment, days, user);

                _currentPlan  = plan;
                PlanText.Text = plan;

                await App.WorkoutService.SavePlanAsync(new WorkoutPlan
                {
                    PlanText    = plan,
                    Location    = location,
                    Goal        = goal,
                    Equipment   = equipment,
                    DaysPerWeek = days,
                    GeneratedAt = DateTime.Now
                });

                PlanDateBadge.Text       = $"Generated {DateTime.Now:MMM d, yyyy}";
                PlanDateBadge.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                PlanText.Text = $"Error: {ex.Message}";
            }
        }

        // ── Log a workout ─────────────────────────────────────────────
        private async void AddWorkout_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(WorkoutNameBox.Text))
            {
                MessageBox.Show("Please enter a workout name.", "Gymbro",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!int.TryParse(DurationBox.Text, out int duration) || duration < 1 || duration > 300)
            {
                MessageBox.Show("Duration must be between 1 and 300 minutes.", "Gymbro",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var    user      = await App.UserService.GetUserAsync();
            string name      = WorkoutNameBox.Text.Trim();
            string intensity = ((ComboBoxItem)IntensityCombo.SelectedItem)?.Content.ToString() ?? "Medium";

            // Save immediately with local MET estimate
            double met          = intensity.Contains("High") ? 8.0 : intensity.Contains("Low") ? 3.5 : 5.0;
            double weightKg     = user?.WeightKg ?? 75;
            int    localCalories = (int)Math.Round(met * weightKg * (duration / 60.0));

            var entry = new WorkoutEntry
            {
                WorkoutName     = name,
                DurationMinutes = duration,
                Intensity       = intensity,
                CaloriesBurned  = localCalories,
                Date            = DateTime.Today
            };

            await App.WorkoutService.SaveWorkoutEntryAsync(entry);

            WorkoutNameBox.Text = "";
            DurationBox.Text    = "";
            await LoadWorkoutsAsync();

            // Refine with AI in background
            if (user != null)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var refined = await App.CloudService.EstimateCaloriesBurnedAsync(
                            name, duration, intensity, user);
                        entry.CaloriesBurned = refined.CaloriesBurned;
                        entry.Notes          = refined.Notes;
                        await App.WorkoutService.UpdateWorkoutEntryAsync(entry);
                        Dispatcher.Invoke(() => { _ = LoadWorkoutsAsync(); });
                    }
                    catch { /* local estimate stays */ }
                });
            }
        }

        // ── Delete workout ────────────────────────────────────────────
        private async void DeleteWorkout_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                await App.WorkoutService.DeleteWorkoutEntryAsync(id);
                await LoadWorkoutsAsync();
            }
        }

        // ── Load history tab ─────────────────────────────────────────
        private async Task LoadHistoryAsync()
        {
            var entries = await App.WorkoutService.GetWeekHistoryAsync(7);
            HistoryList.ItemsSource    = entries;
            EmptyHistoryText.Visibility = entries.Count == 0
                ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
