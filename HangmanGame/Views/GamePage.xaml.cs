using HangmanGame.ViewModels;
using Microsoft.Maui.Layouts;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace HangmanGame.Views;

public partial class GamePage : ContentPage
{
	public GamePage()
	{
		InitializeComponent();

		var vm = (GameViewModel)BindingContext;
		vm.StepChanged += async (s, e) =>
		{
			CanvasView.InvalidateSurface();

			if (vm.CurrentStep == 6)
				await PlayHangAnimation();
		};
	}

	private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		var info = e.Info;
		canvas.Clear(SKColors.White);

		// 1) Zeminin ortasýndan geçen elips gölgesi
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
			float halfWidth = 100f;   // platform yarý geniþliði
			float halfHeight = 6f;     // gölge yarý yüksekliði

			// Elips dikdörtgeni: top = baseY - halfHeight, bottom = baseY + halfHeight
			var shadowRect = new SKRect(
				centerX - halfWidth,
				baseY - halfHeight,
				centerX + halfWidth,
				baseY + halfHeight
			);
			canvas.DrawOval(shadowRect, shadowPaint);
		}

		// 2) Platform ve iskeletin geri kalaný
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

			// Üst kiriþ ve ip
			canvas.DrawLine(centerX, hookY, hookX, hookY, paint);
			canvas.DrawLine(hookX, hookY, hookX, hookY + 40, paint);

			// Vücut parçalarý (CurrentStep’e göre)
			int step = (BindingContext as GameViewModel)?.CurrentStep ?? 0;
			if (step >= 1) canvas.DrawCircle(hookX, hookY + 65, 25, paint);                              // kafa
			if (step >= 2) canvas.DrawLine(hookX, hookY + 90, hookX, hookY + 160, paint);              // gövde
			if (step >= 3) canvas.DrawLine(hookX, hookY + 110, hookX - 30, hookY + 140, paint);         // sol kol
			if (step >= 4) canvas.DrawLine(hookX, hookY + 110, hookX + 30, hookY + 140, paint);         // sað kol
			if (step >= 5) canvas.DrawLine(hookX, hookY + 160, hookX - 30, hookY + 210, paint);         // sol bacak
			if (step >= 6) canvas.DrawLine(hookX, hookY + 160, hookX + 30, hookY + 210, paint);         // sað bacak
		}
	}

	private async Task PlayHangAnimation()
	{
		// CanvasView: SKCanvasView, VisualElement olduðu için RotateTo kullanabiliriz
		// Birkaç kez sola-saða sallanýp duruyor.
		await CanvasView.RotateTo(10, 300, Easing.SinInOut);
		await CanvasView.RotateTo(-10, 300, Easing.SinInOut);
		await CanvasView.RotateTo(8, 300, Easing.SinInOut);
		await CanvasView.RotateTo(-8, 300, Easing.SinInOut);
		await CanvasView.RotateTo(0, 300, Easing.SinInOut);
	}


	private void OnToggleMusicClicked(object sender, EventArgs e)
	{
		// Þimdilik geçici
		Console.WriteLine("?? Müzik aç/kapa týklandý.");
	}

	private async void OnExitClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//MainPage");
	}

	private void ImageButton_Clicked(object sender, EventArgs e)
	{

	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		if (BindingContext is GameViewModel vm)
		{
			vm.RefreshKeyboard();
			BuildKeyboardLayout(vm.KeyboardRows);
		}
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

	private void BuildKeyboardLayout(List<List<string>> keyboardRows)
	{
		KeyboardLayout.Children.Clear();

		var displayWidth = DeviceDisplay.MainDisplayInfo.Width
						   / DeviceDisplay.MainDisplayInfo.Density;
		double spacing = 6;
		double horizontalPadding = 20;
		double availableWidth = displayWidth - horizontalPadding * 2;

		var flexLayout = new FlexLayout
		{
			Wrap = FlexWrap.Wrap,
			Direction = FlexDirection.Row,
			JustifyContent = FlexJustify.Center,
			AlignItems = FlexAlignItems.Center,
			Margin = new Thickness(0, 5),
		};

		var allKeys = keyboardRows.SelectMany(r => r).ToList();
		int referenceKeyCount = 10;
		double totalSpacing = (referenceKeyCount + 1) * spacing;
		double keyWidth = (availableWidth - totalSpacing) / referenceKeyCount;
		double keyHeight = 48;

		foreach (var key in allKeys)
		{
			var button = new Button
			{
				Text = key,
				FontSize = 18,
				WidthRequest = keyWidth,
				HeightRequest = keyHeight,
				CornerRadius = 6,
				BackgroundColor = Colors.White,
				BorderColor = Colors.Black,
				BorderWidth = 1,
				Margin = new Thickness(spacing / 2),
				TextColor = Colors.Black,
			};

			button.Clicked += (s, e) =>
			{
				if (BindingContext is GameViewModel vm)
					vm.GuessCommand.Execute(key);

				button.IsEnabled = false;

				if (BindingContext is GameViewModel vm2)
				{
					bool correct = vm2.IsCorrectLetter(key);
					button.BackgroundColor = correct
						? Colors.LightGreen
						: Colors.Pink;
				}
			};

			flexLayout.Children.Add(button);
		}

		KeyboardLayout.Children.Add(flexLayout);
	}
}