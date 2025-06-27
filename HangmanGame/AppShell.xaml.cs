using CommunityToolkit.Maui.Views;
using HangmanGame.Views;
using HangmanGame.ViewModels;

namespace HangmanGame
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        protected override bool OnBackButtonPressed()
        {
            if (Current.CurrentPage is MainPage mainPage && mainPage.BindingContext is MainPageViewModel vm)
            {
                _ = vm.AttemptExit();
                return true; 
            }

            return base.OnBackButtonPressed();
        }
    }
}
