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
            LoadWorkoutsAsync();
        }

        private async void LoadWorkoutsAsync()
        {
            var entries = await App.WorkoutService.GetTodayEntriesAsync();
            WorkoutList.ItemsSource = entries;
        }

        private async void GeneratePlan_Click(object sender, RoutedEventArgs e)
        {
            var user = await App.UserService.GetUserAsync();
            if (user == null)
            {
                MessageBox.Show("ჯერ პროფილი შეავსე!", "Gymbro",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string location = ((ComboBoxItem)LocationCombo.SelectedItem)?.Content.ToString() ?? "";
            string goal = ((ComboBoxItem)GoalCombo.SelectedItem)?.Content.ToString() ?? "";
            int days = int.Parse(((ComboBoxItem)DaysCombo.SelectedItem).Content
                                .ToString().Split(' ')[0]);

            PlanText.Text = "⏳ AI გეგმას ქმნის...";

            try
            {
                _currentPlan = await App.CloudService.GenerateWorkoutPlanAsync(
                    location, goal, days, user);
                PlanText.Text = _currentPlan;
            }
            catch (Exception ex)
            {
                PlanText.Text = $"შეცდომა: {ex.Message}";
            }
        }

        private async void AddWorkout_Click(object sender, RoutedEventArgs e)
        {
            // ვალიდაცია
            if (string.IsNullOrWhiteSpace(WorkoutNameBox.Text))
            {
                MessageBox.Show("ვარჯიშის სახელი შეიყვანე!", "Gymbro",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(DurationBox.Text, out int duration) || duration < 1 || duration > 300)
            {
                MessageBox.Show("ხანგრძლივობა სწორად შეიყვანე (1-300 წუთი)!", "Gymbro",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var user = await App.UserService.GetUserAsync();
            if (user == null)
            {
                MessageBox.Show("ჯერ პროფილი შეავსე!", "Gymbro",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string intensity = ((ComboBoxItem)IntensityCombo.SelectedItem)?.Content.ToString() ?? "საშუალო";

            try
            {
                var entry = await App.CloudService.EstimateCaloriesBurnedAsync(
                    WorkoutNameBox.Text.Trim(), duration, intensity, user);

                await App.WorkoutService.SaveWorkoutEntryAsync(entry);

                WorkoutNameBox.Text = "";
                DurationBox.Text = "";

                LoadWorkoutsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"შეცდომა: {ex.Message}", "Gymbro",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteWorkout_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                await App.WorkoutService.DeleteWorkoutEntryAsync(id);
                LoadWorkoutsAsync();
            }
        }
    }
}