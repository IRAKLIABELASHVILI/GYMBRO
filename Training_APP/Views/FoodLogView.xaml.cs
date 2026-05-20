using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Training_APP.Views
{
    public partial class FoodLogView : UserControl
    {
        public FoodLogView()
        {
            InitializeComponent();
            LoadFoodAsync();
        }

        public void Refresh() => LoadFoodAsync();

        private async void LoadFoodAsync()
        {
            var entries = await App.NutritionService.GetTodayEntriesAsync();
            FoodList.ItemsSource = entries;
            EmptyFoodText.Visibility = entries.Count == 0
                ? Visibility.Visible : Visibility.Collapsed;
        }

        // ─── Drop zone: click to open file dialog ───────────────────────
        private void DropZone_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
            => OpenAndAnalyzeImage();

        // ─── Drag & drop visual feedback ────────────────────────────────
        private void DropZone_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            DropZoneBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 245, 160));
            DropZoneBorder.Background  = new SolidColorBrush(Color.FromArgb(10, 0, 245, 160));
            DropPrimaryText.Text       = "Release to analyze";
        }

        private void DropZone_DragLeave(object sender, DragEventArgs e)
            => ResetDropZone();

        private void DropZone_Drop(object sender, DragEventArgs e)
        {
            ResetDropZone();
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files?.Length > 0)
                AnalyzeImageAsync(files[0]);
        }

        private void ResetDropZone()
        {
            // Read border/background from the active theme so dark & light mode both look correct
            DropZoneBorder.BorderBrush = Application.Current.Resources["DropZoneBorderBrush"] as Brush
                                         ?? new SolidColorBrush(Color.FromRgb(42, 48, 71));
            DropZoneBorder.Background  = Application.Current.Resources["DropZoneBgBrush"] as Brush
                                         ?? new SolidColorBrush(Color.FromRgb(13, 16, 24));
            DropPrimaryText.Text       = "Drop a food photo here";
        }

        // ─── Browse file dialog ─────────────────────────────────────────
        private void OpenAndAnalyzeImage()
        {
            var dialog = new OpenFileDialog
            {
                Title  = "Select food photo",
                Filter = "Images (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp"
            };
            if (dialog.ShowDialog() == true)
                AnalyzeImageAsync(dialog.FileName);
        }

        // ─── Manual text entry ──────────────────────────────────────────
        private async void AddManual_Click(object sender, RoutedEventArgs e)
        {
            string food = ManualFoodBox.Text.Trim();
            if (string.IsNullOrEmpty(food))
            {
                StatusText.Text = "⚠ Please enter a food name.";
                return;
            }

            if (!int.TryParse(ManualGramsBox.Text.Trim(), out int grams) || grams < 1 || grams > 5000)
            {
                StatusText.Text = "⚠ Enter a valid amount in grams (1–5000).";
                return;
            }

            string meal = ((ComboBoxItem)MealTypeCombo.SelectedItem).Content.ToString()!;

            await AnalyzeAndSaveAsync(food, grams, meal);

            ManualFoodBox.Text  = "";
            ManualGramsBox.Text = "";
        }

        // ─── Core analysis pipeline ─────────────────────────────────────
        private async void AnalyzeImageAsync(string imagePath)
        {
            if (!File.Exists(imagePath)) return;

            string meal = ((ComboBoxItem)MealTypeCombo.SelectedItem).Content.ToString()!;

            SetStatus("🔍 Analyzing photo...", isWorking: true);
            try
            {
                var (foodName, estimatedGrams) =
                    await App.CloudService.AnalyzeFoodImageAsync(imagePath);

                var dialog = new FoodGramsDialog(foodName, estimatedGrams);
                if (dialog.ShowDialog() != true)
                {
                    SetStatus("", isWorking: false);
                    return;
                }

                await AnalyzeAndSaveAsync(dialog.FoodName, dialog.Grams, meal);
            }
            catch (Exception ex)
            {
                SetStatus($"Error: {ex.Message}", isWorking: false);
            }
        }

        private async System.Threading.Tasks.Task AnalyzeAndSaveAsync(
            string foodName, int grams, string meal)
        {
            SetStatus("🔍 Checking food database...", isWorking: true);
            try
            {
                // 1 — Check local cache first (instant, no AI call)
                var entry = await App.FoodCacheService.GetCachedEntryAsync(
                                foodName, grams, meal);

                if (entry != null)
                {
                    // Cache hit — save and done
                    await App.NutritionService.SaveFoodEntryAsync(entry);
                    SetStatus($"✓ {entry.FoodName} added ({entry.Calories} kcal)  ⚡ from cache",
                              isWorking: false);
                    LoadFoodAsync();
                    return;
                }

                // 2 — Not cached — ask AI
                SetStatus("⏳ Asking AI for nutrition data...", isWorking: true);
                entry = await App.CloudService.AnalyzeFoodWithGramsAsync(
                    foodName, grams, meal);

                // Save to food log
                await App.NutritionService.SaveFoodEntryAsync(entry);

                // Save to cache for next time (fire-and-forget — user doesn't wait)
                _ = App.FoodCacheService.SaveToCacheAsync(entry, grams);

                SetStatus($"✓ {entry.FoodName} added ({entry.Calories} kcal)",
                          isWorking: false);
                LoadFoodAsync();
            }
            catch (Exception ex)
            {
                SetStatus($"Error: {ex.Message}", isWorking: false);
            }
        }

        // ─── Delete ─────────────────────────────────────────────────────
        private async void DeleteFood_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                await App.NutritionService.DeleteFoodEntryAsync(id);
                LoadFoodAsync();
            }
        }

        private void SetStatus(string msg, bool isWorking)
        {
            StatusText.Text      = msg;
            DropZoneBorder.IsHitTestVisible = !isWorking;
        }
    }
}
