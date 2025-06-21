using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using SQLitePCL;

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
			return builder.Build();
		}
	}
}
