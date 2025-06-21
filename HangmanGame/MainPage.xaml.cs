using HangmanGame.ViewModels;

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
