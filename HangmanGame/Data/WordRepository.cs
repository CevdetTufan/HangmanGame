using HangmanGame.Models.HangmanGame.Models;
using Microsoft.Data.Sqlite;

namespace HangmanGame.Data
{
	public class WordRepository
	{
		private readonly string _dbPath;

		public WordRepository()
		{
			_dbPath = Path.Combine(FileSystem.AppDataDirectory, "Words.db");
			EnsureDatabase();
		}

		public async Task<WordEntry?> GetNextWordAsync(string lang, string level)
		{
			await using var conn = new SqliteConnection($"Data Source={_dbPath}");
			await conn.OpenAsync();

			var cmd = conn.CreateCommand();
			cmd.CommandText = @"
                SELECT Id, Level, Lang, Word, Meaning, IsUsed
                  FROM Words
                 WHERE Lang = $lang
                   AND Level = $level
                   AND IsUsed = 0
              ORDER BY RANDOM()
                 LIMIT 1";
			cmd.Parameters.AddWithValue("$lang", lang);
			cmd.Parameters.AddWithValue("$level", level);

			await using var reader = await cmd.ExecuteReaderAsync();
			if (await reader.ReadAsync())
			{
				var entry = new WordEntry
				{
					Id = reader.GetInt32(0),
					Level = reader.GetString(1),
					Lang = reader.GetString(2),
					Word = reader.GetString(3),
					Meaning = reader.GetString(4),
					IsUsed = reader.GetInt32(5)
				};

				await MarkWordUsedAsync(entry.Id);
				
				return entry;
			}

			return null;
		}

		//get dummy word for testing purposes
		public async Task<WordEntry> GetDummyWordAsync()
		{
			return new WordEntry
			{
				Id = 1,
				Level = "easy",
				Lang = "en-US",
				Word = "apple",
				Meaning = "a round fruit with red or green skin",
				IsUsed = 0
			};

		}

		public async Task MarkWordUsedAsync(int id)
		{
			await using var conn = new SqliteConnection($"Data Source={_dbPath}");
			await conn.OpenAsync();

			var cmd = conn.CreateCommand();
			cmd.CommandText = @"
                UPDATE Words
                   SET IsUsed = 1
                 WHERE Id = $id";
			cmd.Parameters.AddWithValue("$id", id);

			await cmd.ExecuteNonQueryAsync();
		}

		public async Task ResetUsedAsync(string lang)
		{
			await using var conn = new SqliteConnection($"Data Source={_dbPath}");
			await conn.OpenAsync();

			var cmd = conn.CreateCommand();
			cmd.CommandText = @"
                UPDATE Words
                   SET IsUsed = 0
                 WHERE Lang = $lang";
			cmd.Parameters.AddWithValue("$lang", lang);

			await cmd.ExecuteNonQueryAsync();
		}

		public async Task<int> CountRemainingAsync(string lang, string level)
		{
			await using var conn = new SqliteConnection($"Data Source={_dbPath}");
			await conn.OpenAsync();

			var cmd = conn.CreateCommand();
			cmd.CommandText = @"
                SELECT COUNT(*) 
                  FROM Words
                 WHERE Lang = $lang
                   AND Level = $level
                   AND IsUsed = 0";
			cmd.Parameters.AddWithValue("$lang", lang);
			cmd.Parameters.AddWithValue("$level", level);

			var result = await cmd.ExecuteScalarAsync();
			return Convert.ToInt32(result);
		}

		private void EnsureDatabase()
		{
			using var conn = new SqliteConnection($"Data Source={_dbPath}");
			conn.Open();

			using var cmd = conn.CreateCommand();
			cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Words (
                Id       INTEGER PRIMARY KEY AUTOINCREMENT,
                Level    TEXT    NOT NULL,
                Lang     TEXT    NOT NULL,
                Word     TEXT    NOT NULL,
                Meaning  TEXT    NOT NULL,
                IsUsed   INTEGER NOT NULL DEFAULT 0
            );";
			cmd.ExecuteNonQuery();

			cmd.CommandText = @"
					INSERT OR IGNORE INTO Words (Level, Lang, Word, Meaning) VALUES
					  -- English (US)
					  ('easy','en-US','apple','a round fruit with red or green skin'),
					  ('easy','en-US','bridge','a structure built over water'),
					  -- Türkçe
					  ('easy','tr','elma','yuvarlak, kırmızı veya yeşil kabuklu meyve'),
					  ('easy','tr','köprü','su veya yol üzerine inşa edilen yapı'),
					  -- Deutsch
					  ('easy','de','haus','ein gebäude, in dem man wohnt'),
					  ('easy','de','brücke','ein bauwerk über wasser'),
					  -- Français
					  ('easy','fr','pomme','un fruit rond à peau rouge ou verte'),
					  ('easy','fr','pont','une structure construite au-dessus de l’eau'),
					  -- Español
					  ('easy','es','manzana','una fruta redonda con piel roja o verde'),
					  ('easy','es','puente','una estructura construida sobre agua'),
					  -- Italiano
					  ('easy','it','mela','un frutto rotondo con buccia rossa o verde'),
					  ('easy','it','ponte','una struttura costruita sull’acqua'),
					  -- Українська
					  ('easy','uk','яблуко','круглий фрукт із червоною або зеленою шкіркою'),
					  ('easy','uk','міст','споруда, побудована над водою');
				 ";

			cmd.ExecuteNonQuery();
		}
	}
}
