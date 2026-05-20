using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Training_APP.Model;

namespace Training_APP.Views
{
    public partial class ProfileView : UserControl
    {
        public ProfileView()
        {
            InitializeComponent();
            LoadProfileAsync();
            LoadWeightAsync();
        }

        public void Refresh()
        {
            LoadProfileAsync();
            LoadWeightAsync();
        }

        // ── Load saved profile ───────────────────────────────────────
        private async void LoadProfileAsync()
        {
            var user = await App.UserService.GetUserAsync();
            if (user == null) return;

            NameBox.Text       = user.Name;
            AgeBox.Text        = user.Age.ToString();
            WeightBox.Text     = user.WeightKg.ToString();
            HeightBox.Text     = user.HeightCm.ToString();

            SelectComboByContent(GenderCombo,   user.Gender);
            SelectComboByContent(ActivityCombo, user.ActivityLevel);
            SelectComboByContent(GoalCombo,     user.Goal);

            UpdateAvatar(user.Name);
            UpdateBmi(user.WeightKg, user.HeightCm);

            if (user.CalorieGoal > 0)
                ShowResults(user);
        }

        // ── Live BMI as user types weight/height ────────────────────
        private void PhysicalStats_Changed(object sender, TextChangedEventArgs e)
        {
            if (!double.TryParse(WeightBox.Text, out double w) ||
                !double.TryParse(HeightBox.Text, out double h) ||
                h <= 0) return;
            UpdateBmi(w, h);
        }

        private void Name_Changed(object sender, TextChangedEventArgs e)
            => UpdateAvatar(NameBox.Text);

        // ── Save ────────────────────────────────────────────────────
        private async void SaveProfile_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            { ShowError("Please enter your name."); return; }

            if (!int.TryParse(AgeBox.Text, out int age) || age < 16 || age > 100)
            { ShowError("Age must be between 16 and 100."); return; }

            if (!double.TryParse(WeightBox.Text, out double weight) || weight < 30 || weight > 300)
            { ShowError("Weight must be between 30 and 300 kg."); return; }

            if (!double.TryParse(HeightBox.Text, out double height) || height < 100 || height > 250)
            { ShowError("Height must be between 100 and 250 cm."); return; }

            if (GenderCombo.SelectedItem == null)
            { ShowError("Please select your gender."); return; }

            if (ActivityCombo.SelectedItem == null)
            { ShowError("Please select your activity level."); return; }

            if (GoalCombo.SelectedItem == null)
            { ShowError("Please select your fitness goal."); return; }

            try
            {
                var user = new User
                {
                    Name          = NameBox.Text.Trim(),
                    Age           = age,
                    WeightKg      = weight,
                    HeightCm      = height,
                    Gender        = ((ComboBoxItem)GenderCombo.SelectedItem).Content.ToString()!,
                    ActivityLevel = ((ComboBoxItem)ActivityCombo.SelectedItem).Content.ToString()!,
                    Goal          = ((ComboBoxItem)GoalCombo.SelectedItem).Content.ToString()!
                };

                await App.UserService.SaveUserAsync(user); // RecalculateGoals called inside
                ShowResults(user);
                UpdateAvatar(user.Name);
            }
            catch (Exception ex)
            {
                ShowError($"Could not save: {ex.Message}");
            }
        }

        // ── UI helpers ───────────────────────────────────────────────
        private void ShowResults(User user)
        {
            // Top stat cards
            CalorieGoalText.Text  = user.CalorieGoal.ToString("N0");
            GoalSummaryText.Text  = GoalEmoji(user.Goal) + " " + user.Goal;

            // Macro breakdown card
            TdeeProtein.Text      = user.ProteinGoal.ToString("N0");
            TdeeCarbs.Text        = user.CarbsGoal.ToString("N0");
            TdeeFats.Text         = user.FatsGoal.ToString("N0");
            TdeeCard.Visibility   = Visibility.Visible;
        }

        private void UpdateBmi(double weightKg, double heightCm)
        {
            if (heightCm <= 0) return;
            double hm  = heightCm / 100.0;
            double bmi = weightKg / (hm * hm);

            BmiText.Text = bmi.ToString("F1");

            (string label, Color color) = bmi switch
            {
                < 18.5 => ("Underweight", Color.FromRgb(0, 180, 216)),
                < 25.0 => ("Normal weight", Color.FromRgb(0, 245, 160)),
                < 30.0 => ("Overweight", Color.FromRgb(255, 184, 0)),
                _      => ("Obese", Color.FromRgb(255, 80, 80))
            };

            BmiCategory.Text       = label;
            BmiText.Foreground     = new SolidColorBrush(color);
            BmiCategory.Foreground = new SolidColorBrush(color);
        }

        private void UpdateAvatar(string name)
        {
            AvatarInitials.Text = string.IsNullOrWhiteSpace(name)
                ? "?"
                : name.Trim()[0].ToString().ToUpper();
        }

        private static string GoalEmoji(string goal) => goal switch
        {
            "Weight Loss"       => "🔥",
            "Muscle Gain"       => "💪",
            "Improve Endurance" => "🏃",
            _                   => "⚖️"
        };

        private static void SelectComboByContent(ComboBox combo, string value)
        {
            foreach (ComboBoxItem item in combo.Items)
                if (item.Content?.ToString() == value) { combo.SelectedItem = item; return; }
        }

        private static void ShowError(string msg)
            => MessageBox.Show(msg, "Gymbro", MessageBoxButton.OK, MessageBoxImage.Warning);

        // ── Weight log ───────────────────────────────────────────────
        private async void LoadWeightAsync()
        {
            var today   = await App.WeightService.GetTodayAsync();
            var entries = await App.WeightService.GetRecentAsync(30);

            if (today != null)
            {
                TodayWeightBadge.Text       = $"Today: {today.WeightKg:F1} kg";
                TodayWeightBadge.Visibility = Visibility.Visible;
            }

            UpdateWeightTrend(entries);

            // Draw chart after layout pass so canvas has actual size
            WeightChart.Loaded -= WeightChart_Loaded; // unsubscribe to avoid double-fire
            WeightChart.Loaded += WeightChart_Loaded;
            _chartEntries = entries;

            // If canvas is already loaded, draw immediately
            if (WeightChart.IsLoaded)
                DrawWeightChart(entries);
        }

        private List<WeightEntry> _chartEntries = new();

        private void WeightChart_Loaded(object sender, RoutedEventArgs e)
            => DrawWeightChart(_chartEntries);

        private async void LogWeight_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(WeightLogBox.Text, out double kg) || kg < 20 || kg > 350)
            {
                ShowError("Please enter a valid weight between 20 and 350 kg.");
                return;
            }

            await App.WeightService.LogWeightAsync(kg);
            WeightLogBox.Text = "";
            LoadWeightAsync();
        }

        private void UpdateWeightTrend(List<WeightEntry> entries)
        {
            if (entries.Count < 2)
            {
                WeightTrendText.Text = "Log your weight daily to see your trend";
                return;
            }

            double first = entries.First().WeightKg;
            double last  = entries.Last().WeightKg;
            double diff  = last - first;
            string arrow = diff < 0 ? "↓" : diff > 0 ? "↑" : "→";
            string sign  = diff > 0 ? "+" : "";
            int span     = (entries.Last().Date - entries.First().Date).Days;

            WeightTrendText.Text = $"{arrow} {sign}{diff:F1} kg over {span} days  •  Latest: {last:F1} kg";
        }

        private void DrawWeightChart(List<WeightEntry> entries)
        {
            WeightChart.Children.Clear();
            if (entries.Count < 1) return;

            double w = WeightChart.ActualWidth;
            double h = WeightChart.ActualHeight;
            if (w < 10 || h < 10) return;

            const double pad = 12;
            double chartW = w - pad * 2;
            double chartH = h - pad * 2;

            double minKg = entries.Min(e => e.WeightKg) - 1;
            double maxKg = entries.Max(e => e.WeightKg) + 1;
            double range = maxKg - minKg;
            if (range < 1) range = 1;

            Brush lineBrush = Application.Current.Resources["AccentGreenBrush"] is SolidColorBrush sb
                ? new SolidColorBrush(sb.Color)
                : new SolidColorBrush(Color.FromRgb(0, 245, 160));

            // Compute pixel positions
            var points = new List<Point>();
            double totalSpan = entries.Count == 1
                ? 1
                : (entries.Last().Date - entries.First().Date).TotalDays;

            for (int i = 0; i < entries.Count; i++)
            {
                double xFraction = entries.Count == 1
                    ? 0.5
                    : (entries[i].Date - entries[0].Date).TotalDays / totalSpan;
                double yFraction = 1 - (entries[i].WeightKg - minKg) / range;

                double px = pad + xFraction * chartW;
                double py = pad + yFraction * chartH;
                points.Add(new Point(px, py));
            }

            // Draw gradient fill under the line
            if (points.Count > 1)
            {
                var geo = new StreamGeometry();
                using (var ctx = geo.Open())
                {
                    ctx.BeginFigure(new Point(points[0].X, h), true, true);
                    ctx.LineTo(points[0], false, false);
                    for (int i = 1; i < points.Count; i++)
                        ctx.LineTo(points[i], true, false);
                    ctx.LineTo(new Point(points[^1].X, h), false, false);
                }
                geo.Freeze();

                Color lineColor = ((SolidColorBrush)lineBrush).Color;
                var fill = new Path
                {
                    Data = geo,
                    Fill = new LinearGradientBrush(
                        Color.FromArgb(60, lineColor.R, lineColor.G, lineColor.B),
                        Color.FromArgb(0,  lineColor.R, lineColor.G, lineColor.B),
                        new Point(0, 0), new Point(0, 1))
                };
                WeightChart.Children.Add(fill);
            }

            // Draw line segments
            for (int i = 0; i < points.Count - 1; i++)
            {
                WeightChart.Children.Add(new Line
                {
                    X1              = points[i].X,
                    Y1              = points[i].Y,
                    X2              = points[i + 1].X,
                    Y2              = points[i + 1].Y,
                    Stroke          = lineBrush,
                    StrokeThickness = 2,
                    StrokeLineJoin  = PenLineJoin.Round
                });
            }

            // Draw dots + value labels
            foreach (var pt in points)
            {
                var dot = new Ellipse
                {
                    Width  = 7, Height = 7,
                    Fill   = lineBrush
                };
                Canvas.SetLeft(dot, pt.X - 3.5);
                Canvas.SetTop(dot,  pt.Y - 3.5);
                WeightChart.Children.Add(dot);
            }

            // Y-axis grid lines & labels (min, mid, max)
            var gridVals = new[] { minKg + 0.5, (minKg + maxKg) / 2.0, maxKg - 0.5 };
            foreach (var val in gridVals)
            {
                double yFrac = 1 - (val - minKg) / range;
                double py    = pad + yFrac * chartH;

                var grid = new Line
                {
                    X1              = pad,
                    Y1              = py,
                    X2              = w - pad,
                    Y2              = py,
                    Stroke          = new SolidColorBrush(Color.FromArgb(40, 126, 132, 148)),
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 4, 4 }
                };
                WeightChart.Children.Add(grid);

                var lbl = new TextBlock
                {
                    Text       = $"{val:F0}",
                    FontSize   = 9,
                    Foreground = new SolidColorBrush(Color.FromArgb(130, 126, 132, 148))
                };
                Canvas.SetLeft(lbl, 2);
                Canvas.SetTop(lbl,  py - 7);
                WeightChart.Children.Add(lbl);
            }
        }
    }
}
