using CommunityToolkit.Maui.Views;

namespace HangmanGame.Views
{
    public partial class ConfirmationPopup : Popup
    {
        public bool Confirmed { get; private set; }

        public ConfirmationPopup()
        {
            InitializeComponent();
        }

        private async void OnNoClicked(object sender, EventArgs e)
        {
            Confirmed = false;
            await CloseAsync();
        }

        private async void OnYesClicked(object sender, EventArgs e)
        {
            Confirmed = true;
            await CloseAsync();
        }
    }
} 