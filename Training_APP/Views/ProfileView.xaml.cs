using System.Windows;
using System.Windows.Controls;
using Training_APP.Model;

namespace Training_APP.Views
{
    public partial class ProfileView : UserControl
    {
        public ProfileView()
        {
            InitializeComponent();
            LoadProfileAsync();
        }

        private async void LoadProfileAsync()
        {
            var user = await App.UserService.GetUserAsync();
            if (user == null) return;

            NameBox.Text = user.Name;
            AgeBox.Text = user.Age.ToString();
            WeightBox.Text = user.WeightKg.ToString();
            HeightBox.Text = user.HeightCm.ToString();
            GenderCombo.Text = user.Gender;
            ActivityCombo.Text = user.ActivityLevel;
            GoalCombo.Text = user.Goal;
        }


            private async void SaveProfile_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("სახელი შეიყვანე!", "Gymbro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(AgeBox.Text, out int age) || age < 16 || age > 80)
            {
                MessageBox.Show("ასაკი უნდა იყოს 16-დან 80-მდე!", "Gymbro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            if (!double.TryParse(WeightBox.Text, out double weight) || weight < 30 || weight > 300)
            {
                MessageBox.Show("წონა უნდა იყოს 30-დან 300კგ-მდე!", "Gymbro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(HeightBox.Text, out double height) || height < 100 || height > 250)
            {
                MessageBox.Show("სიმაღლე უნდა იყოს 100-დან 250სმ-მდე!", "Gymbro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (GenderCombo.SelectedItem == null)
            {
                MessageBox.Show("სქესი აირჩიე!", "Gymbro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (ActivityCombo.SelectedItem == null)
            {
                MessageBox.Show("აქტივობის დონე აირჩიე!", "Gymbro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (GoalCombo.SelectedItem == null)
            {
                MessageBox.Show("მიზანი აირჩიე!", "Gymbro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var user = new User
                {
                    Name = NameBox.Text.Trim(),
                    Age = age,
                    WeightKg = weight,
                    HeightCm = height,
                    Gender = ((ComboBoxItem)GenderCombo.SelectedItem).Content.ToString(),
                    ActivityLevel = ((ComboBoxItem)ActivityCombo.SelectedItem).Content.ToString(),
                    Goal = ((ComboBoxItem)GoalCombo.SelectedItem).Content.ToString()
                };

                await App.UserService.SaveUserAsync(user);
                MessageBox.Show("პროფილი შენახულია! ✓", "Gymbro",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"შეცდომა: {ex.Message}", "Gymbro",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}