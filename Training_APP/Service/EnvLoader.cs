using System;
using System.IO;

namespace Training_APP.Service
{
    /// <summary>
    /// Loads key=value pairs from a .env file next to the running executable
    /// and injects them into the current process's environment variables.
    /// </summary>
    public static class EnvLoader
    {
        public static void Load(string? path = null)
        {
            path ??= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");

            if (!File.Exists(path)) return;

            foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith('#')) continue;

                var idx = line.IndexOf('=');
                if (idx < 1) continue;

                var key   = line[..idx].Trim();
                var value = line[(idx + 1)..].Trim()
                                             .Trim('"')
                                             .Trim('\'');

                // Only set if not already overridden by the real environment
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
                    Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}
