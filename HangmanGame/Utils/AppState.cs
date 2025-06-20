namespace HangmanGame.Utils;

public static class AppState
{
	public static string SelectedLang { get; set; } = "tr";
	public static int WordsPlayedCount { get; set; } = 0;
	public static int TotalScore { get; set; } = 0;
}