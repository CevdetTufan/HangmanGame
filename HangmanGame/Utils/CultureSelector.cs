using HangmanGame.Models;

namespace HangmanGame.Utils
{
    public static class CultureSelector
    {
        public static void SetCulture(string cultureCode)
        {
            try
            {
                var culture = new System.Globalization.CultureInfo(cultureCode);
				Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
                System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
                System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting culture: {ex.Message}");
            }
		}

        public static void ResetCulture()
        {
			var defaultCulture = new System.Globalization.CultureInfo(GetDefaultLanguage().Code);
            SetCulture(defaultCulture.Name);
		}

		public static LanguageOption GetDefaultLanguage()
		=> new LanguageOption { DisplayName = "🇺🇸 English", Code = "en" };

        public static List<LanguageOption> GetAvailableLanguages()
        {
            return new List<LanguageOption>
            {
                new() { DisplayName = "🇺🇸 English", Code = "en" },
                //new() { DisplayName = "🇬🇧 English (UK)", Code = "en-GB" },
                //new LanguageOption { DisplayName = "🇺🇸🇬🇧 English", Code = "en" },
				new() { DisplayName = "🇹🇷 Türkçe",      Code = "tr"    },
                new() { DisplayName = "🇮🇹 Italiano",    Code = "it"    },
                new() { DisplayName = "🇩🇪 Deutsch",     Code = "de"    },
                new() { DisplayName = "🇫🇷 Français",    Code = "fr"    },
                new() { DisplayName = "🇪🇸 Español",     Code = "es"    },
                new() { DisplayName = "🇺🇦 Українська",  Code = "uk"    }
            };
		}
	}
}
