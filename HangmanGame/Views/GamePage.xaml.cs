using CommunityToolkit.Maui.Views;
using HangmanGame.ViewModels;
using Plugin.Maui.Audio;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace HangmanGame.Views;

[QueryProperty(nameof(NeedsReset), "NewGame")]
public partial class GamePage : ContentPage
{
	public bool NeedsReset { set
		{
			if (value && BindingContext is GameViewModel vm)
			{
				MainThread.BeginInvokeOnMainThread(async () => {
					await vm.ResetAndLoadNewWordAsync();
				});
			}
		} 
	}
	public GamePage(IAudioManager audioManager)
	{
		InitializeComponent();
		BindingContext = new GameViewModel(audioManager);

		var vm = (GameViewModel)BindingContext;
		vm.StepChanged += async (s, e) =>
		{
			CanvasView.InvalidateSurface();

			if (vm.CurrentStep == 6)
				await PlayHangAnimation();
		};

		vm.GameOver += async (_, args) =>
		{
			await Task.Delay(500); // Animasyonun bitmesi için kısa bir bekleme
			
			var popup = new ResultPopup(args.Win, args.Answer);
			await this.ShowPopupAsync(popup);

			if (popup.PlayAgain == true)
			{
				// "Yeni Oyun" dediyse oyunu sıfırla
				var gameViewModel = (GameViewModel)BindingContext;
				
				// Eğer tüm kelimeler tamamlandıysa, kelimeleri sıfırla
				if (args.Answer == "TEBRİKLER! BÜTÜN KELİMELERİ BİLDİNİZ!")
				{
					await gameViewModel.ResetAllWordsAndLoadNewWordAsync();
				}
				else
				{
					await gameViewModel.ResetAndLoadNewWordAsync();
				}
				
				CanvasView.InvalidateSurface();
			}
			else
			{ 
				await Shell.Current.GoToAsync("//MainPage");
			}
		};

		vm.NewGameStarted += (s, e) => ResetKeyboard();
	}

	private void ResetKeyboard()
	{
		var keybButtons = KeyboardLayout
			.Children.OfType<FlexLayout>()
			.SelectMany(flex => flex.Children.OfType<Button>());

		Color originalColor;
		if (Application.Current?.RequestedTheme == AppTheme.Light)
		{
			originalColor = Colors.White;
		}
		else
		{
			originalColor = Color.FromArgb("#FF2C2C2C");
		}


		foreach (var btn in keybButtons)
		{
			btn.IsEnabled = true;
			btn.BackgroundColor = originalColor;
		}
	}

	private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		var info = e.Info;
		canvas.Clear(SKColors.White);

		// 1) Zeminin ortasından geçen elips gölgesi
		using (var shadowPaint = new SKPaint
		{
			Style = SKPaintStyle.Fill,
			Color = SKColors.Black.WithAlpha(50),
			IsAntialias = true,
			MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 8)
		})
		{
			float centerX = info.Width / 2f;
			float baseY = info.Height * 0.95f;
			float halfWidth = 100f;   // platform yarı genişliği
			float halfHeight = 6f;     // gölge yarı yüksekliği

			// Elips dikdörtgeni: top = baseY - halfHeight, bottom = baseY + halfHeight
			var shadowRect = new SKRect(
				centerX - halfWidth,
				baseY - halfHeight,
				centerX + halfWidth,
				baseY + halfHeight
			);
			canvas.DrawOval(shadowRect, shadowPaint);
		}

		// 2) Platform ve iskeletin geri kalanı
		using (var paint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			StrokeWidth = 5,
			Color = SKColors.Black,
			IsAntialias = true
		})
		{
			float centerX = info.Width / 2f;
			float baseY = info.Height * 0.95f;
			float poleHeight = info.Height * 0.7f;
			float hookY = baseY - poleHeight;
			float hookX = centerX + 100;

			// Platform (zemin)
			canvas.DrawLine(centerX - 100, baseY, centerX + 100, baseY, paint);

			// Dikey direk
			canvas.DrawLine(centerX, baseY, centerX, hookY, paint);

			// Üst kiriş ve ip
			canvas.DrawLine(centerX, hookY, hookX, hookY, paint);
			canvas.DrawLine(hookX, hookY, hookX, hookY + 40, paint);

			// Vücut parçaları (CurrentStep'e göre)
			int step = (BindingContext as GameViewModel)?.CurrentStep ?? 0;
			if (step >= 1) canvas.DrawCircle(hookX, hookY + 65, 25, paint);                              // kafa
			if (step >= 2) canvas.DrawLine(hookX, hookY + 90, hookX, hookY + 160, paint);              // gövde
			if (step >= 3) canvas.DrawLine(hookX, hookY + 110, hookX - 30, hookY + 140, paint);         // sol kol
			if (step >= 4) canvas.DrawLine(hookX, hookY + 110, hookX + 30, hookY + 140, paint);         // sağ kol
			if (step >= 5) canvas.DrawLine(hookX, hookY + 160, hookX - 30, hookY + 210, paint);         // sol bacak
			if (step >= 6) canvas.DrawLine(hookX, hookY + 160, hookX + 30, hookY + 210, paint);         // sağ bacak
		}
	}

	private async Task PlayHangAnimation()
	{
		// CanvasView: SKCanvasView, VisualElement olduğu için RotateTo kullanabiliriz
		// Birkaç kez sola-sağa sallanıp duruyor.
		await CanvasView.RotateTo(10, 300, Easing.SinInOut);
		await CanvasView.RotateTo(-10, 300, Easing.SinInOut);
		await CanvasView.RotateTo(8, 300, Easing.SinInOut);
		await CanvasView.RotateTo(-8, 300, Easing.SinInOut);
		await CanvasView.RotateTo(0, 300, Easing.SinInOut);
	}


	private void OnToggleMusicClicked(object sender, EventArgs e)
	{
		// Şimdilik geçici
		Console.WriteLine("?? Müzik aç/kapa tıklandı.");
	}

	private async void OnExitClicked(object sender, EventArgs e)
	{
		var popup = new ConfirmationPopup();
		await this.ShowPopupAsync(popup);

		if (popup.Confirmed)
		{
			await Shell.Current.GoToAsync("//MainPage");
		}
	}

	private async void ImageButton_Clicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//MainPage");
	}

	private void OnKeyClicked(object sender, EventArgs e)
	{
		if (sender is Button btn
			&& BindingContext is GameViewModel vm
			&& btn.CommandParameter is string letter)
		{
			// first execute the guess
			vm.GuessCommand.Execute(letter);

			// now disable
			btn.IsEnabled = false;

			// color-feedback
			var correct = vm.IsCorrectLetter(letter);
			btn.BackgroundColor = correct
				? Colors.LightGreen
				: Colors.Pink;
		}
	}
}