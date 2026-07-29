using Celer.Properties;

namespace Celer.Utilities
{
    public class Localization
    {
        public readonly static Dictionary<string, string> languages = new()
        { 
            { "en", "English (UK)" },
            { "pt", "Português (Portugal)" }
        };


        public static string GetCurrentLangugageByValue(string key) {
            return languages.TryGetValue(key, out string? value) ? value : "en";
        }

        public static string GetLanguageValueFromPreference()
        {
            return GetCurrentLangugageByValue(MainConfiguration.Default.Language);
        }

        public static string GetLanguageKeyFromPreference()
        {
            return languages.FirstOrDefault(x => x.Value == GetLanguageValueFromPreference()).Key;
        }

        public static void SetApplicationLanguage(string value)
        {
            var key = languages.FirstOrDefault(x => x.Value == value).Key;
            if (key is not null)
            {
                MainConfiguration.Default.Language = key;
                MainConfiguration.Default.Save();
            }
        }
    }
}
