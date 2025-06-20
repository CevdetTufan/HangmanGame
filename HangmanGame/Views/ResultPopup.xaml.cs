using CommunityToolkit.Maui.Views;

namespace HangmanGame.Views
{
	public partial class ResultPopup : Popup
	{
		// Kullanıcının seçimini saklayacak property
		public bool? PlayAgain { get; set; }

		public ResultPopup(bool win, string answer)
		{
			InitializeComponent();

			// Default: null (ne seçti bilinmiyor)
			PlayAgain = null;

			// Mesajı göster
			MessageLabel.Text = win
				? "Tebrikler! Kazandınız 🎉"
				: $"Üzgünüm, kaybettiniz 😢\nDoğru kelime: {answer}";
		}

		private async void OnPlayAgainClicked(object sender, EventArgs e)
		{
			PlayAgain = true;           // “Yeni Oyun” dedi
			await CloseAsync();         // Popup’u kapat
		}

		private async void OnExitClicked(object sender, EventArgs e)
		{
			PlayAgain = false;          // “Çıkış” dedi
			await CloseAsync();         // Popup’u kapat
		}
	}
}
