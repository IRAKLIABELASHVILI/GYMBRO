using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Training_APP.Model;

namespace Training_APP.Views
{
    public partial class CoachView : UserControl
    {
        private DailyLog _dailyLog;
        private string _workoutPlan = "";

        public CoachView()
        {
            InitializeComponent();
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            _dailyLog = await App.NutritionService.GetTodayLogAsync();
        }

        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            string question = MessageBox.Text.Trim();
            if (string.IsNullOrEmpty(question)) return;

            // მომხმარებლის შეტყობინება
            AddMessage(question, isUser: true);
            MessageBox.Text = "";

            // Loading...
            var loadingMsg = AddMessage("⏳ ვფიქრობ...", isUser: false);

            try
            {
                string topic = ((ComboBoxItem)TopicCombo.SelectedItem).Content.ToString();
                string response;

                if (topic.Contains("კვება"))
                {
                    _dailyLog = await App.NutritionService.GetTodayLogAsync();
                    response = await App.CloudService.AskNutritionQuestionAsync(question, _dailyLog);
                }
                else
                {
                    response = await App.CloudService.AskWorkoutQuestionAsync(question, _workoutPlan);
                }

                // Loading შეცვალე პასუხით
                ChatPanel.Children.Remove(loadingMsg);
                AddMessage(response, isUser: false);
            }
            catch (Exception ex)
            {
                ChatPanel.Children.Remove(loadingMsg);
                AddMessage($"შეცდომა: {ex.Message}", isUser: false);
            }

            // ქვემოთ გადახვევა
            ChatScroll.ScrollToBottom();
        }

        private Border AddMessage(string text, bool isUser)
        {
            var border = new Border
            {
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(14, 10, 14, 10),
                Margin = new Thickness(isUser ? 80 : 0, 4, isUser ? 0 : 80, 4),
                Background = new SolidColorBrush(
                    isUser
                    ? Color.FromRgb(26, 92, 58)
                    : Color.FromRgb(15, 42, 26))
            };

            border.Child = new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap
            };

            ChatPanel.Children.Add(border);
            return border;
        }
    }
}