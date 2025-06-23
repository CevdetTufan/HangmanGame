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

	private IDispatcherTimer? _animationTimer;
	private List<SceneObject> _clouds = new();
	private List<SceneObject> _birds = new();
	private Random _random = new();
	private bool _sceneInitialized = false;
	private double _time = 0;
	private GameViewModel _vm;

	// Hareketli nesneler için basit bir yardımcı sınıf
	private class SceneObject
	{
		public SKPoint Position { get; set; }
		public float Size { get; set; }
		public float Speed { get; set; }
	}

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
		_vm = new GameViewModel(audioManager);
		BindingContext = _vm;

		this.Appearing += OnPageAppearing;
		this.Disappearing += OnPageDisappearing;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		_ = _vm.StartGameAudioAsync();
		double screenWidth = Application.Current?.MainPage?.Width ?? 360;
		double screenHeight = Application.Current?.MainPage?.Height ?? 640;
		int totalLetters = _vm.KeyboardLetters.Count;
		int maxPerRow = 10; // Maksimum bir satırda gösterilecek tuş sayısı
		double buttonSize = Math.Min(
			(screenWidth - 32) / maxPerRow,
			(screenHeight * 0.28) / Math.Ceiling((double)totalLetters / maxPerRow)
		);
		Device.BeginInvokeOnMainThread(() =>
		{
			var keybButtons = KeyboardLayout.Children.OfType<Button>();
			foreach (var btn in keybButtons)
			{
				btn.WidthRequest = buttonSize;
				btn.HeightRequest = buttonSize;
				btn.FontSize = buttonSize * 0.45;
			}
		});
	}

	private void ResetKeyboard()
	{
		var keybButtons = KeyboardLayout.Children.OfType<Button>();

		Color originalColor;
		if (Application.Current?.RequestedTheme == AppTheme.Light)
		{
			originalColor = Colors.White;
		}
		else
		{
			originalColor = Color.FromArgb("#FF2C2C2C");
		}

		double screenWidth = Application.Current?.MainPage?.Width ?? 360;
		double screenHeight = Application.Current?.MainPage?.Height ?? 640;
		int totalLetters = _vm.KeyboardLetters.Count;
		int maxPerRow = 10;
		double buttonSize = Math.Min(
			(screenWidth - 32) / maxPerRow,
			(screenHeight * 0.28) / Math.Ceiling((double)totalLetters / maxPerRow)
		);

		foreach (var btn in keybButtons)
		{
			btn.IsEnabled = true;
			btn.BackgroundColor = originalColor;
			btn.WidthRequest = buttonSize;
			btn.HeightRequest = buttonSize;
			btn.FontSize = buttonSize * 0.45;
		}
	}

	private void OnPageAppearing(object? sender, EventArgs e)
	{
		_vm.GameOver += Vm_GameOver;
		_vm.StepChanged += Vm_StepChanged;
		_vm.NewGameStarted += Vm_NewGameStarted;

		if (_animationTimer == null)
		{
			_animationTimer = Dispatcher.CreateTimer();
			_animationTimer.Interval = TimeSpan.FromMilliseconds(33); // ~30fps
			_animationTimer.Tick += AnimationTimer_Tick;
		}
		_animationTimer.Start();
	}

	private void OnPageDisappearing(object? sender, EventArgs e)
	{
		_animationTimer?.Stop();

		_vm.StopBackgroundMusic();

		_vm.GameOver -= Vm_GameOver;
		_vm.StepChanged -= Vm_StepChanged;
		_vm.NewGameStarted -= Vm_NewGameStarted;
	}

	private void AnimationTimer_Tick(object? sender, EventArgs e)
	{
		if (!_sceneInitialized) return;

		_time += 0.05; // Çim animasyonu için zamanı ilerlet

		// Bulutları ve kuşları hareket ettir
		UpdateSceneObjectPositions(_clouds, (float)CanvasView.Width);
		UpdateSceneObjectPositions(_birds, (float)CanvasView.Width);

		CanvasView.InvalidateSurface();
	}

	private void UpdateSceneObjectPositions(List<SceneObject> objects, float width)
	{
		foreach (var obj in objects)
		{
			var newX = obj.Position.X + obj.Speed;

			// Yöne göre ekran dışına çıkma kontrolü ve döngü
			if (obj.Speed > 0 && newX > width + obj.Size * 2)
			{
				newX = -obj.Size * 2; // Sağdan çıktı, soldan gir
			}
			else if (obj.Speed < 0 && newX < -obj.Size * 2)
			{
				newX = width + obj.Size * 2; // Soldan çıktı, sağdan gir
			}
			obj.Position = new SKPoint(newX, obj.Position.Y);
		}
	}

	private void InitializeSceneObjects(int width, int height)
	{
		_clouds.Clear();
		for (int i = 0; i < 4; i++)
		{
			var speed = (float)(_random.NextDouble() * 0.5 + 0.2);
			_clouds.Add(new SceneObject
			{
				Position = new SKPoint(_random.Next(0, width), _random.Next((int)(height * 0.1), (int)(height * 0.4))),
				Size = _random.Next(20, 35),
				Speed = _random.Next(0, 2) == 0 ? speed : -speed // Yön rastgele
			});
		}

		_birds.Clear();
		for (int i = 0; i < 3; i++)
		{
			var speed = (float)(_random.NextDouble() * 1.5 + 0.8); // Hız çeşitliliği artırıldı
			_birds.Add(new SceneObject
			{
				Position = new SKPoint(_random.Next(0, width), _random.Next((int)(height * 0.1), (int)(height * 0.3))),
				Size = 7,
				Speed = _random.Next(0, 2) == 0 ? speed : -speed // Yön rastgele
			});
		}
		_sceneInitialized = true;
	}

	private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		var info = e.Info;
		canvas.Clear(); // Arka planı temizle

		if (!_sceneInitialized)
		{
			InitializeSceneObjects(info.Width, info.Height);
		}

		// --- Arka Plan Sahnesi ---
		bool isDarkTheme = Application.Current?.RequestedTheme == AppTheme.Dark;

		float baseY = info.Height; // Tam alt kenar
		float centerX = info.Width * 0.32f;

		// 1. Gökyüzü Gradient'ı
		using (var bgPaint = new SKPaint())
		{
			SKColor startColor, endColor;
			if (isDarkTheme)
			{
				startColor = SKColor.Parse("#2C3E50"); // Gece mavisi
				endColor = SKColor.Parse("#465A70");
			}
			else
			{
				startColor = SKColor.Parse("#87CEEB"); // Gündüz mavisi
				endColor = SKColor.Parse("#E0F8FF");
			}

			bgPaint.Shader = SKShader.CreateLinearGradient(
				new SKPoint(info.Rect.MidX, info.Rect.Top),
				new SKPoint(info.Rect.MidX, info.Rect.Bottom),
				new[] { startColor, endColor },
				SKShaderTileMode.Clamp);

			canvas.DrawRect(info.Rect, bgPaint);
		}

		// 2. Güneş / Ay
		using (var sunMoonPaint = new SKPaint { IsAntialias = true })
		{
			if (isDarkTheme)
			{
				sunMoonPaint.Color = SKColors.WhiteSmoke; // Ay
				canvas.DrawCircle(info.Width * 0.8f, info.Height * 0.2f, 30, sunMoonPaint);
			}
			else
			{
				sunMoonPaint.Color = SKColors.Gold; // Güneş
				canvas.DrawCircle(info.Width * 0.8f, info.Height * 0.2f, 40, sunMoonPaint);
			}
		}

		// 3. Bulutlar
		using (var cloudPaint = new SKPaint { IsAntialias = true, Color = SKColors.White.WithAlpha(200) })
		{
			foreach (var cloud in _clouds)
			{
				canvas.DrawCircle(cloud.Position.X, cloud.Position.Y, cloud.Size, cloudPaint);
				canvas.DrawCircle(cloud.Position.X + cloud.Size * 0.8f, cloud.Position.Y, cloud.Size * 1.2f, cloudPaint);
				canvas.DrawCircle(cloud.Position.X + cloud.Size * 1.6f, cloud.Position.Y, cloud.Size, cloudPaint);
			}
		}

		// 4. Uzaktaki Ağaçlar
		using (var treePaint = new SKPaint { IsAntialias = true, Color = isDarkTheme ? SKColor.Parse("#5D6D7E") : SKColor.Parse("#95A5A6") })
		{
			var path1 = new SKPath();
			path1.MoveTo(info.Width * 0.1f, baseY);
			path1.LineTo(info.Width * 0.15f, baseY - 80);
			path1.LineTo(info.Width * 0.2f, baseY);
			path1.Close();
			canvas.DrawPath(path1, treePaint);

			var path2 = new SKPath();
			path2.MoveTo(info.Width * 0.7f, baseY);
			path2.LineTo(info.Width * 0.75f, baseY - 100);
			path2.LineTo(info.Width * 0.8f, baseY);
			path2.Close();
			canvas.DrawPath(path2, treePaint);
		}
		
		// 5. Kuşlar
		using (var birdPaint = new SKPaint { IsAntialias = true, Color = isDarkTheme ? SKColors.LightGray : SKColors.Black, Style = SKPaintStyle.Stroke, StrokeWidth = 3 })
		{
			foreach (var bird in _birds)
			{
				var birdPath = new SKPath();
				float dir = Math.Sign(bird.Speed); // Yöne göre "V" şeklini ayarla
				birdPath.MoveTo(-bird.Size * dir, 0);
				birdPath.LineTo(0, -bird.Size);
				birdPath.LineTo(bird.Size * dir, 0);

				canvas.Save();
				canvas.Translate(bird.Position);
				canvas.DrawPath(birdPath, birdPaint);
				canvas.Restore();
			}
		}

		// --- Ön Plan (İskele ve Adam) ---

		// Zeminin ortasından geçen elips gölgesi
		using (var shadowPaint = new SKPaint
		{
			Style = SKPaintStyle.Fill,
			Color = SKColors.Black.WithAlpha(50),
			IsAntialias = true,
			MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 8)
		})
		{
			float platformWidth = info.Width * 0.2f;
			float platformHeight = info.Height * 0.03f;
			float halfWidth = platformWidth / 2f;
			float halfHeight = platformHeight / 2f;
			var shadowRect = new SKRect(centerX - halfWidth, baseY - halfHeight, centerX + halfWidth, baseY + halfHeight);
			canvas.DrawOval(shadowRect, shadowPaint);
		}

		// İskelet ve Zemin
		using (var paint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			StrokeWidth = info.Width * 0.007f, // Daha ince çizgi
			Color = isDarkTheme ? SKColors.LightGray : SKColors.SaddleBrown,
			IsAntialias = true
		})
		{
			float platformWidth = info.Width * 0.2f;
			float platformHeight = info.Height * 0.03f;
			float poleHeight = info.Height * 0.7f;
			float hookY = baseY - poleHeight;
			float hookX = centerX + info.Width * 0.18f;

			// Platform (zemin)
			canvas.DrawLine(centerX - platformWidth / 2f, baseY, centerX + platformWidth / 2f, baseY, paint);

			// Dikey direk
			canvas.DrawLine(centerX, baseY, centerX, hookY, paint);

			// Üst kiriş
			canvas.DrawLine(centerX, hookY, hookX, hookY, paint);

			// -- Adamın Çizimi ve İp --
			float manHookX = hookX + (float)_manOffsetX;
			float manHookY = hookY + (float)_manOffsetY;

			float headRadius = info.Height * 0.05f;
			float bodyLength = info.Height * 0.13f;
			float armLength = info.Width * 0.08f;
			float legLength = info.Height * 0.12f;

			// İp
			canvas.DrawLine(hookX, hookY, manHookX, manHookY + headRadius * 1.6f, paint);

			// Vücut parçaları
			int step = (BindingContext as GameViewModel)?.CurrentStep ?? 0;
			if (step >= 1) canvas.DrawCircle(manHookX, manHookY + headRadius * 2.6f, headRadius, paint); // Kafa
			if (step >= 2) canvas.DrawLine(manHookX, manHookY + headRadius * 3.7f, manHookX, manHookY + headRadius * 3.7f + bodyLength, paint); // Gövde
			if (step >= 3) canvas.DrawLine(manHookX, manHookY + headRadius * 4.2f, manHookX - armLength, manHookY + headRadius * 4.2f + armLength, paint); // Sol kol
			if (step >= 4) canvas.DrawLine(manHookX, manHookY + headRadius * 4.2f, manHookX + armLength, manHookY + headRadius * 4.2f + armLength, paint); // Sağ kol
			if (step >= 5) canvas.DrawLine(manHookX, manHookY + headRadius * 3.7f + bodyLength, manHookX - legLength * 0.6f, manHookY + headRadius * 3.7f + bodyLength + legLength, paint); // Sol bacak
			if (step >= 6) canvas.DrawLine(manHookX, manHookY + headRadius * 3.7f + bodyLength, manHookX + legLength * 0.6f, manHookY + headRadius * 3.7f + bodyLength + legLength, paint); // Sağ bacak
		}

		// 6. Zemin Çimleri (En son, platformun üstüne çizilir)
		using (var grassPaint = new SKPaint { IsAntialias = true, Color = SKColors.Green.WithAlpha(180), StrokeWidth = 2 })
		{
			// Çim çizgilerini canvas'ın tam kenarından kenarına kadar çiz
			for (int i = 0; i < info.Width; i += 4)
			{
				var grassHeight = (float)(_random.NextDouble() * 10 + 5);
				// Zamanla değişen bir sinüs dalgası ile sallanma efekti
				var sway = (float)Math.Sin(_time + i * 0.1) * 3;
				canvas.DrawLine(i, baseY, i + sway, baseY - grassHeight, grassPaint);
			}
		}

		// 3D zemin efekti (platformun altına, kenardan kenara)
		using (var groundPaint = new SKPaint
		{
			Style = SKPaintStyle.Fill,
			Color = SKColor.Parse("#8D5524").WithAlpha(180), // Toprak kahverengisi
			IsAntialias = true,
			MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 12)
		})
		{
			float groundHeight = info.Height * 0.045f;
			var groundRect = new SKRect(0, baseY - groundHeight / 2, info.Width, baseY + groundHeight / 2);
			canvas.DrawOval(groundRect, groundPaint);
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

			// Ekran boyutuna göre tuş boyutunu tekrar uygula (gerekirse)
			double screenWidth = Application.Current?.MainPage?.Width ?? 360;
			double screenHeight = Application.Current?.MainPage?.Height ?? 640;
			int totalLetters = _vm.KeyboardLetters.Count;
			int maxPerRow = 10;
			double buttonSize = Math.Min(
				(screenWidth - 32) / maxPerRow,
				(screenHeight * 0.28) / Math.Ceiling((double)totalLetters / maxPerRow)
			);
			btn.WidthRequest = buttonSize;
			btn.HeightRequest = buttonSize;
			btn.FontSize = buttonSize * 0.45;
		}
	}

	#region ViewModel Event Handlers

	private async void Vm_GameOver(object? sender, (bool Win, string Answer) args)
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
	}

	private async void Vm_StepChanged(object? sender, EventArgs e)
	{
		CanvasView.InvalidateSurface();

		if (_vm.CurrentStep > 0 && _vm.CurrentStep < 6)
		{
			await PlayPartAddedAnimation();
		}
		else if (_vm.CurrentStep == 6)
		{
			await PlayAdvancedHangAnimation();
		}
	}

	private void Vm_NewGameStarted(object? sender, EventArgs e)
	{
		ResetKeyboard();
		_sceneInitialized = false; // Animasyonlu nesneleri yeniden başlat
	}

	#endregion
}