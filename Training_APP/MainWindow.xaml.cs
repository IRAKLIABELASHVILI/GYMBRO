using System.Windows;

namespace Training_APP
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // პროგრამა იხსნება Dashboard-ზე
            NavDashboard_Click(null, null);
        }

        private void NavDashboard_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new Views.DashboardView();
        }

        private void NavFood_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new Views.FoodLogView();
        }

        private void NavWorkout_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new Views.WorkoutView();
        }

        private void NavCoach_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new Views.CoachView();
        }

        private void NavProfile_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new Views.ProfileView();
        }
    }
}