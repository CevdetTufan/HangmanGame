using HangmanGame.Utils;

namespace HangmanGame
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
			CultureSelector.ResetCulture(); 
		}

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}