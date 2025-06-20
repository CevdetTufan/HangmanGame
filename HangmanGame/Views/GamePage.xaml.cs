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

		  ((GameViewModel)BindingContext).StepChanged += (s, e) =>
        {
            CanvasView.InvalidateSurface();
        };
	}

	private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		var info = e.Info;
		canvas.Clear(SKColors.White);

		var paint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			StrokeWidth = 5,
			Color = SKColors.Black,
			IsAntialias = true
		};

		if (BindingContext is not GameViewModel vm)
			return;

		int step = vm.CurrentStep;

		float centerX = info.Width / 2f;
		float baseY = info.Height * 0.95f;
		float poleHeight = info.Height * 0.7f;
		float hookY = baseY - poleHeight;
		float hookX = centerX + 100;
		float headRadius = 25f;

		// ??? ZEMÝN VE DÝKEY DÝREK (her zaman) ???
		canvas.DrawLine(centerX - 100, baseY, centerX + 100, baseY, paint);
		canvas.DrawLine(centerX, baseY, centerX, hookY, paint);

		// ??? ÜST KÝRÝÞ VE ÝP (adým baðýmsýz bir kere) ???
		canvas.DrawLine(centerX, hookY, hookX, hookY, paint);
		canvas.DrawLine(hookX, hookY, hookX, hookY + 40, paint);

		// ??? KALAN PARÇALAR ADIMA GÖRE ???

		// 1: kafa
		if (step >= 1)
			canvas.DrawCircle(hookX, hookY + 65, headRadius, paint);

		// 2: gövde
		if (step >= 2)
			canvas.DrawLine(hookX, hookY + 90, hookX, hookY + 160, paint);

		// 3: sol kol
		if (step >= 3)
			canvas.DrawLine(hookX, hookY + 110, hookX - 30, hookY + 140, paint);

		// 4: sað kol
		if (step >= 4)
			canvas.DrawLine(hookX, hookY + 110, hookX + 30, hookY + 140, paint);

		// 5: sol bacak
		if (step >= 5)
			canvas.DrawLine(hookX, hookY + 160, hookX - 30, hookY + 210, paint);

		// 6: sað bacak
		if (step >= 6)
			canvas.DrawLine(hookX, hookY + 160, hookX + 30, hookY + 210, paint);
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