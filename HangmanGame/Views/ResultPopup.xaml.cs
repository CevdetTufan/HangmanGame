using CommunityToolkit.Maui.Views;

namespace HangmanGame.Views
{
	public partial class ResultPopup : Popup
	{
		// Kullanıcının seçimini saklayacak property
		public bool? PlayAgain { get; private set; }

		public ResultPopup(bool win, string answer)
		{
			InitializeComponent();

			// Default: null (ne seçti bilinmiyor)
			PlayAgain = null;

			// Popup herhangi bir şekilde kapanırsa, eğer PlayAgain null ise yeni oyun başlatılsın
			this.Closed += (s, e) =>
			{
				if (PlayAgain == null)
				{
					PlayAgain = true;
				}
			};

			// Özel durum: Tüm kelimeler tamamlandı
			if (answer == "TEBRİKLER! BÜTÜN KELİMELERİ BİLDİNİZ!")
			{
				ResultIconLabel.Text = "🏆";
				MessageLabel.Text = "Tebrikler! Bütün kelimeleri bildiniz!";
				AnswerLabel.IsVisible = false;
			}
			else if (win)
			{
				ResultIconLabel.Text = "🎉";
				MessageLabel.Text = "Tebrikler! Kazandınız";
				AnswerLabel.IsVisible = false;
			}
			else
			{
				ResultIconLabel.Text = "😢";
				MessageLabel.Text = "Üzgünüm, kaybettiniz";
				AnswerLabel.Text = $"Doğru kelime: {answer}";
				AnswerLabel.IsVisible = true;
			}
		}

		private async void OnPlayAgainClicked(object sender, EventArgs e)
		{
			// Popup içinden başka popup göstermek yerine, doğrudan onaylı olarak kapat
			PlayAgain = true;           // "Yeni Oyun" dedi
			await CloseAsync();         // Popup'u kapat
		}

		private async void OnExitClicked(object sender, EventArgs e)
		{
			PlayAgain = false;          // "Çıkış" dedi
			await CloseAsync();         // Popup'u kapat
		}
	}
}
