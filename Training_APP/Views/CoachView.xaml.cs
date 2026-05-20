using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Training_APP.Model;

namespace Training_APP.Views
{
    public partial class CoachView : UserControl
    {
        private DailyLog? _dailyLog;
        private string _workoutPlan = "";

        public CoachView()
        {
            InitializeComponent();
            LoadDataAsync();
            AddMessage("Hey! I'm your AI Coach. Ask me anything about your nutrition or workout.", isUser: false);
        }

        public void Refresh() => LoadDataAsync();

        private async void LoadDataAsync()
        {
            _dailyLog = await App.NutritionService.GetTodayLogAsync();
        }

        private void MessageBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(MessageBox.Text))
                SendMessage_Click(sender, e);
        }

        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            string question = MessageBox.Text.Trim();
            if (string.IsNullOrEmpty(question)) return;

            AddMessage(question, isUser: true);
            MessageBox.Text = "";

            var loadingBubble = AddMessage("⏳ Thinking...", isUser: false);

            try
            {
                string topic = ((ComboBoxItem)TopicCombo.SelectedItem).Content.ToString()!;
                string response;

                if (topic.Contains("Nutrition"))
                {
                    _dailyLog = await App.NutritionService.GetTodayLogAsync();
                    response  = await App.CloudService.AskNutritionQuestionAsync(question, _dailyLog);
                }
                else
                {
                    response = await App.CloudService.AskWorkoutQuestionAsync(question, _workoutPlan);
                }

                ChatPanel.Children.Remove(loadingBubble);
                AddMessage(response, isUser: false);
            }
            catch (Exception ex)
            {
                ChatPanel.Children.Remove(loadingBubble);
                AddMessage($"Sorry, something went wrong: {ex.Message}", isUser: false);
            }

            ChatScroll.ScrollToBottom();
        }

        private Border AddMessage(string text, bool isUser)
        {
            // Pull colors from the active theme so they work in both dark and light mode
            var accentBrush = Application.Current.Resources["AccentGreenBrush"] as Brush
                              ?? new SolidColorBrush(Color.FromRgb(0, 245, 160));
            var cardBrush   = Application.Current.Resources["CardBrush"] as Brush
                              ?? new SolidColorBrush(Color.FromRgb(20, 23, 34));
            var textBrush   = Application.Current.Resources["TextPrimaryBrush"] as Brush
                              ?? Brushes.White;

            // User bubble: accent colour at low opacity; AI bubble: card colour
            Brush userBg = accentBrush is SolidColorBrush sb
                ? new SolidColorBrush(Color.FromArgb(180, sb.Color.R, sb.Color.G, sb.Color.B))
                : accentBrush;

            var bubble = new Border
            {
                CornerRadius = new CornerRadius(12, 12, isUser ? 4 : 12, isUser ? 12 : 4),
                Padding      = new Thickness(14, 10, 14, 10),
                Margin       = new Thickness(isUser ? 80 : 0, 0, isUser ? 0 : 80, 10),
                Background   = isUser ? userBg : cardBrush
            };

            if (!isUser)
            {
                bubble.BorderBrush     = accentBrush;
                bubble.BorderThickness = new Thickness(2, 0, 0, 0);
            }

            bubble.Child = new TextBlock
            {
                Text         = text,
                Foreground   = textBrush,
                FontFamily   = new FontFamily("Segoe UI"),
                FontSize     = 13,
                LineHeight   = 20,
                TextWrapping = TextWrapping.Wrap
            };

            ChatPanel.Children.Add(bubble);
            return bubble;
        }
    }
}
