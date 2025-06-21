using CommunityToolkit.Maui.Views;
using HangmanGame.ViewModels;
using Plugin.Maui.Audio;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace HangmanGame.Views;

[QueryProperty(nameof(NeedsReset), "NewGame")]
public partial class GamePage : ContentPage
{
	private double _manOffsetX = 0;
	private double _manOffsetY = 0;

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

			// Son adımda büyük sallanma animasyonu oynayacağı için aradaki adımları sars.
			if (vm.CurrentStep > 0 && vm.CurrentStep < 6)
			{
				await PlayPartAddedAnimation();
			}
			else if (vm.CurrentStep == 6)
			{
				await PlayAdvancedHangAnimation();
			}
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
		canvas.Clear(); // Arka planı temizle, temanın rengini alsın

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

			// Üst kiriş
			canvas.DrawLine(centerX, hookY, hookX, hookY, paint);
			
			// -- Adamın Çizimi ve İp --
			// Animasyon için offset'leri uygula
			float manHookX = hookX + (float)_manOffsetX;
			float manHookY = hookY + (float)_manOffsetY;

			// İp (Adamla birlikte hareket edecek şekilde güncellendi)
			canvas.DrawLine(hookX, hookY, manHookX, manHookY + 40, paint);

			// Vücut parçaları (CurrentStep'e göre)
			int step = (BindingContext as GameViewModel)?.CurrentStep ?? 0;
			if (step >= 1) canvas.DrawCircle(manHookX, manHookY + 65, 25, paint);                              // kafa
			if (step >= 2) canvas.DrawLine(manHookX, manHookY + 90, manHookX, manHookY + 160, paint);              // gövde
			if (step >= 3) canvas.DrawLine(manHookX, manHookY + 110, manHookX - 30, manHookY + 140, paint);         // sol kol
			if (step >= 4) canvas.DrawLine(manHookX, manHookY + 110, manHookX + 30, manHookY + 140, paint);         // sağ kol
			if (step >= 5) canvas.DrawLine(manHookX, manHookY + 160, manHookX - 30, manHookY + 210, paint);         // sol bacak
			if (step >= 6) canvas.DrawLine(manHookX, manHookY + 160, manHookX + 30, manHookY + 210, paint);         // sağ bacak
		}
	}

	private async Task PlayAdvancedHangAnimation()
	{
		// 1. Sarsılma (Jolt)
		_manOffsetY = 20;
		CanvasView.InvalidateSurface();
		await Task.Delay(80);
		_manOffsetY = 15;
		CanvasView.InvalidateSurface();
		await Task.Delay(80);
		_manOffsetY = 18;
		CanvasView.InvalidateSurface();
		await Task.Delay(80);
		
		// 2. Yavaşça Sallanma (Sway)
		double amplitude = 15.0; // Sallanma genişliği
		for (int i = 0; i < 120; i++) // Yaklaşık 2 saniye sürer
		{
			_manOffsetX = amplitude * Math.Sin(i * 0.2);
			amplitude *= 0.97; // Sallanmayı yavaşça durdur (damping)
			
			CanvasView.InvalidateSurface();
			await Task.Delay(16); // ~60fps
		}

		_manOffsetX = 0;
		_manOffsetY = 0;
		CanvasView.InvalidateSurface();
	}

	private async Task PlayPartAddedAnimation()
	{
		await CanvasView.TranslateTo(-4, 0, 40, Easing.CubicOut);
		await CanvasView.TranslateTo(4, 0, 40, Easing.CubicOut);
		await CanvasView.TranslateTo(-2, 0, 40, Easing.CubicOut);
		await CanvasView.TranslateTo(2, 0, 40, Easing.CubicOut);
		await CanvasView.TranslateTo(0, 0, 40, Easing.CubicIn);
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