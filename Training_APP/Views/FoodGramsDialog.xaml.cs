using System.Windows;

namespace Training_APP.Views
{
    public partial class FoodGramsDialog : Window
    {
        public string FoodName { get; private set; }
        public int Grams { get; private set; }

        public FoodGramsDialog(string foodName, int estimatedGrams)
        {
            InitializeComponent();
            FoodNameText.Text = foodName;
            NameBox.Text = foodName;
            GramsBox.Text = estimatedGrams.ToString();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("საჭმლის სახელი შეიყვანე!", "Gymbro",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(GramsBox.Text, out int grams) || grams < 1 || grams > 5000)
            {
                MessageBox.Show("გრამები სწორად შეიყვანე (1-5000)!", "Gymbro",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            FoodName = NameBox.Text.Trim();
            Grams = grams;
            DialogResult = true;
        }
    }
}