using HangmanGame.ViewModels;
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

		// Platform her zaman çizilsin
		canvas.DrawLine(x - 40, baseY, x + 100, baseY, paint); // zemin
		canvas.DrawLine(x, baseY, x, baseY - poleHeight, paint); // dikey direk
		canvas.DrawLine(x, baseY - poleHeight, x + 100, baseY - poleHeight, paint); // üst yatay
		canvas.DrawLine(x + 100, baseY - poleHeight, x + 100, baseY - poleHeight + 40, paint); // ip

		// Diðerleri step'e göre çizilsin
		if (step >= 1)
			canvas.DrawCircle(x + 100, baseY - poleHeight + 65, headRadius, paint); // kafa

		if (step >= 2)
			canvas.DrawLine(x + 100, baseY - poleHeight + 90, x + 100, baseY - poleHeight + 160, paint); // gövde

		if (step >= 3)
		{
			canvas.DrawLine(x + 100, baseY - poleHeight + 100, x + 70, baseY - poleHeight + 130, paint); // sol kol
			canvas.DrawLine(x + 100, baseY - poleHeight + 100, x + 130, baseY - poleHeight + 130, paint); // sað kol
		}

		if (step >= 4)
		{
			canvas.DrawLine(x + 100, baseY - poleHeight + 160, x + 70, baseY - poleHeight + 210, paint); // sol bacak
			canvas.DrawLine(x + 100, baseY - poleHeight + 160, x + 130, baseY - poleHeight + 210, paint); // sað bacak
		}
	}

	private void OnToggleMusicClicked(object sender, EventArgs e)
	{
		// Þimdilik geçici
		Console.WriteLine("?? Müzik aç/kapa týklandý.");
	}

	private async void OnExitClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//HomePage");
		// Veya: Application.Current.MainPage = new NavigationPage(new HomePage());
	}

	private void ImageButton_Clicked(object sender, EventArgs e)
	{

	}
}