using CommunityToolkit.Maui.Views;
using HangmanGame.ViewModels;
using HangmanGame.Views;

namespace HangmanGame
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }

        private async void OnSocialIconClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.CommandParameter is string url)
            {
                try
                {
                    Uri uri = new(url);
                    await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
                }
                catch (Exception ex)
                {
                    // Hata yönetimi: URL açılamazsa ne olacağı burada ele alınabilir.
                    // Örneğin, kullanıcıya bir uyarı gösterilebilir.
                    Console.WriteLine($"Could not open URL: {ex.Message}");
                }
            }
        }
    }
}
