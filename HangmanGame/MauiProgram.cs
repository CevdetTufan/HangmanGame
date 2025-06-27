using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using SQLitePCL;
using Plugin.Maui.Audio;
using HangmanGame.Data;
using HangmanGame.ViewModels;
using HangmanGame.Views;

namespace HangmanGame
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			Batteries.Init();

			var builder = MauiApp.CreateBuilder();
			builder
				.UseMauiApp<App>()
				.UseSkiaSharp()
				.UseMauiCommunityToolkit()
				.AddAudio()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
					fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
					fonts.AddFont("MaterialSymbols.ttf", "MaterialIcons");
				});

#if DEBUG
			builder.Logging.AddDebug();
#endif
			builder.Services.AddLocalization();

			builder.Services.AddSingleton<WordRepository>();
			builder.Services.AddTransient<GameViewModel>();
			builder.Services.AddTransient<GamePage>();
			builder.Services.AddTransient<MainPageViewModel>();
			builder.Services.AddTransient<MainPage>();

			return builder.Build();
		}
	}
}
