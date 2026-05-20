using System.Windows;
using System.Windows.Controls;
using Training_APP.Service;

namespace Training_APP
{
    public partial class MainWindow : Window
    {
        // ── Cached view instances — created once, reused on every navigation ──
        private readonly Views.DashbordView  _dashboard = new();
        private readonly Views.FoodLogView   _foodLog   = new();
        private readonly Views.WorkoutView   _workout   = new();
        private readonly Views.CoachView     _coach     = new();
        private readonly Views.ProfileView   _profile   = new();

        public MainWindow()
        {
            InitializeComponent();
            NavDashboard.IsChecked = true;
            UpdateThemeIcon();
        }

        private void NavDashboard_Checked(object sender, RoutedEventArgs e)
            => Navigate(_dashboard);
        private void NavFood_Checked(object sender, RoutedEventArgs e)
            => Navigate(_foodLog);
        private void NavWorkout_Checked(object sender, RoutedEventArgs e)
            => Navigate(_workout);
        private void NavCoach_Checked(object sender, RoutedEventArgs e)
            => Navigate(_coach);
        private void NavProfile_Checked(object sender, RoutedEventArgs e)
            => Navigate(_profile);

        private void Navigate(UserControl view)
        {
            // Refresh data before showing — no flash, no stale numbers
            if (view is Views.DashbordView  d) d.Refresh();
            else if (view is Views.FoodLogView  f) f.Refresh();
            else if (view is Views.WorkoutView  w) w.Refresh();
            else if (view is Views.CoachView    c) c.Refresh();
            else if (view is Views.ProfileView  p) p.Refresh();
            MainContent.Content = view;
        }

        private void ThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            ThemeService.Toggle();
            UpdateThemeIcon();
        }

        private void UpdateThemeIcon()
        {
            ThemeToggleBtn.Content = ThemeService.IsDark ? "☀️  Light Mode" : "🌙  Dark Mode";
        }
    }
}
