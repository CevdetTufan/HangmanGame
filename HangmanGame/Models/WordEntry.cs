namespace HangmanGame.Models
{
	namespace HangmanGame.Models
	{
		public class WordEntry
		{
			/// <summary>
			/// Primary key in the SQLite “Words” table
			/// </summary>
			public int Id { get; set; }

			/// <summary>
			/// Difficulty level: "easy", "medium", or "hard"
			/// </summary>
			public string Level { get; set; } = string.Empty;

			/// <summary>
			/// Two-letter language code, e.g. "en", "tr", "de", "fr", "es", "it", "uk"
			/// </summary>
			public string Lang { get; set; } = string.Empty;

			/// <summary>
			/// The actual word to guess (in the given language)
			/// </summary>
			public string Word { get; set; } = string.Empty;

			/// <summary>
			/// The hint/meaning shown as the question (in the same language)
			/// </summary>
			public string Meaning { get; set; } = string.Empty;

			/// <summary>
			/// Flag for whether this word has already been shown (0 = unused, 1 = used)
			/// </summary>
			public int IsUsed { get; set; } = 0;
		}
	}
}
