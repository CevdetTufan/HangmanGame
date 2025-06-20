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

		float x = info.Width / 4;
		float baseY = info.Height * 0.95f;
		float poleHeight = info.Height * 0.7f;
		float headRadius = 25;

		// Platform her zaman �izilsin
		canvas.DrawLine(x - 40, baseY, x + 100, baseY, paint); // zemin
		canvas.DrawLine(x, baseY, x, baseY - poleHeight, paint); // dikey direk
		canvas.DrawLine(x, baseY - poleHeight, x + 100, baseY - poleHeight, paint); // �st yatay
		canvas.DrawLine(x + 100, baseY - poleHeight, x + 100, baseY - poleHeight + 40, paint); // ip

		// Di�erleri step'e g�re �izilsin
		if (step >= 1)
			canvas.DrawCircle(x + 100, baseY - poleHeight + 65, headRadius, paint); // kafa

		if (step >= 2)
			canvas.DrawLine(x + 100, baseY - poleHeight + 90, x + 100, baseY - poleHeight + 160, paint); // g�vde

		if (step >= 3)
		{
			canvas.DrawLine(x + 100, baseY - poleHeight + 100, x + 70, baseY - poleHeight + 130, paint); // sol kol
			canvas.DrawLine(x + 100, baseY - poleHeight + 100, x + 130, baseY - poleHeight + 130, paint); // sa� kol
		}

		if (step >= 4)
		{
			canvas.DrawLine(x + 100, baseY - poleHeight + 160, x + 70, baseY - poleHeight + 210, paint); // sol bacak
			canvas.DrawLine(x + 100, baseY - poleHeight + 160, x + 130, baseY - poleHeight + 210, paint); // sa� bacak
		}
	}

	private void OnToggleMusicClicked(object sender, EventArgs e)
	{
		// �imdilik ge�ici
		Console.WriteLine("?? M�zik a�/kapa t�kland�.");
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

	private void BuildKeyboardLayout(List<List<string>> keyboardRows)
	{
		KeyboardLayout.Children.Clear();

		var displayWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
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
			Padding = new Thickness(0),
			AlignContent = FlexAlignContent.Start
		};

		// T�m harfleri tek boyutlu dizi olarak al
		var allKeys = keyboardRows.SelectMany(r => r).ToList();

		// Ortalama 10 harfe g�re boyut hesapla
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
				Command = (BindingContext as GameViewModel)?.GuessCommand,
				CommandParameter = key
			};

			flexLayout.Children.Add(button);
		}

		KeyboardLayout.Children.Add(flexLayout);
	}




}