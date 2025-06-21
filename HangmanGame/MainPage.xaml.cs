using CommunityToolkit.Maui.Views;
using HangmanGame.ViewModels;
using HangmanGame.Views;

namespace HangmanGame
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = new MainPageViewModel();
        }
    }
}
