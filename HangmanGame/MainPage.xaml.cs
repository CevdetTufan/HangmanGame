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

        protected override bool OnBackButtonPressed()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var popup = new ConfirmationPopup();
                var result = await this.ShowPopupAsync(popup);

                if (popup.Confirmed)
                {
                    Application.Current.Quit();
                }
            });

            return true;
        }
	}
}
