using Microsoft.Win32;

namespace LayerGenForDotNet4
{
    /// <summary>
    /// Simple registry-based settings storage (same approach as original LayerGen).
    /// </summary>
    public static class Settings
    {
        private const string RegPath = @"Software\NaserAlMadi\LayerGenForDotNet4\";

        public static void Set(string key, string value)
        {
            try
            {
                using var reg = Registry.CurrentUser.CreateSubKey(RegPath);
                reg?.SetValue(key, value);
            }
            catch { }
        }

        public static string Get(string key, string defaultValue = "")
        {
            try
            {
                using var reg = Registry.CurrentUser.OpenSubKey(RegPath);
                return reg?.GetValue(key, defaultValue)?.ToString() ?? defaultValue;
            }
            catch { return defaultValue; }
        }

        public static void SetBool(string key, bool value) => Set(key, value ? "1" : "0");

        public static bool GetBool(string key, bool defaultValue = false)
        {
            string v = Get(key, defaultValue ? "1" : "0");
            return v == "1";
        }
    }
}
