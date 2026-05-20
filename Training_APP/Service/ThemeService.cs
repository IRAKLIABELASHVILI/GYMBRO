using System;
using System.IO;
using System.Windows;

namespace Training_APP.Service
{
    public static class ThemeService
    {
        private static readonly string SettingsFile =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                         "Gymbro", "theme.txt");

        public static bool IsDark { get; private set; } = true;

        public static void LoadSaved()
        {
            try
            {
                if (File.Exists(SettingsFile))
                    IsDark = File.ReadAllText(SettingsFile).Trim() != "light";
            }
            catch { }
            Apply(IsDark);
        }

        public static void Toggle()
        {
            Apply(!IsDark);
            Save();
        }

        private static void Apply(bool dark)
        {
            IsDark = dark;
            var mergedDicts = Application.Current.Resources.MergedDictionaries;

            // Remove existing theme dict (first one)
            if (mergedDicts.Count > 0)
                mergedDicts.RemoveAt(0);

            var uri = new Uri(dark
                ? "Themes/DarkTheme.xaml"
                : "Themes/LightTheme.xaml",
                UriKind.Relative);

            mergedDicts.Insert(0, new ResourceDictionary { Source = uri });
        }

        private static void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsFile)!);
                File.WriteAllText(SettingsFile, IsDark ? "dark" : "light");
            }
            catch { }
        }
    }
}
