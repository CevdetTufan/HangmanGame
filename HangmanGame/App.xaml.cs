using HangmanGame.Utils;

namespace HangmanGame
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            var savedLangCode = Preferences.Get("selected_language_code", "en");
            CultureSelector.SetCulture(savedLangCode);
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}