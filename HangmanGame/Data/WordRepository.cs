using HangmanGame.Models.HangmanGame.Models;
using HangmanGame.Utils;
using Microsoft.Data.Sqlite;

namespace HangmanGame.Data
{
	public class WordRepository
	{
		private readonly string _dbPath;
		private bool _isInitialized = false;
		private static readonly SemaphoreSlim _dbSemaphore = new SemaphoreSlim(1, 1);

		public WordRepository()
		{
			_dbPath = Path.Combine(FileSystem.AppDataDirectory, "Words.db");
		}

		public async Task InitializeAsync()
		{
			if (_isInitialized) return;

			await _dbSemaphore.WaitAsync();
			try
			{
				if (_isInitialized) return;

				await using var conn = new SqliteConnection($"Data Source={_dbPath}");
				await conn.OpenAsync();

				await using var cmd = conn.CreateCommand();
				cmd.CommandText = @"
	            CREATE TABLE IF NOT EXISTS Words (
	                Id       INTEGER PRIMARY KEY AUTOINCREMENT,
	                Level    TEXT    NOT NULL,
	                Lang     TEXT    NOT NULL,
	                Word     TEXT    NOT NULL,
	                Meaning  TEXT    NOT NULL,
	                IsUsed   INTEGER NOT NULL DEFAULT 0
	            );";
				await cmd.ExecuteNonQueryAsync();

				cmd.CommandText = @"
						INSERT OR IGNORE INTO Words (Level, Lang, Word, Meaning) VALUES
						  ('easy','en','apple','a round fruit with red or green skin'),
						  ('easy','en','bridge','a structure built over water'),
						  ('easy','tr','elma','yuvarlak, kırmızı veya yeşil kabuklu meyve'),
						  ('easy','tr','köprü','su veya yol üzerine inşa edilen yapı'),
						  ('easy','de','haus','ein gebäude, in dem man wohnt'),
						  ('easy','de','brücke','ein bauwerk über wasser'),
						  ('easy','fr','pomme','un fruit rond à peau rouge ou verte'),
						  ('easy','fr','pont','une structure construite au-dessus de l''eau'),
						  ('easy','es','manzana','una fruta redonda con piel roja o verde'),
						  ('easy','es','puente','una estructura construida sobre agua'),
						  ('easy','it','mela','un frutto rotondo con buccia rossa o verde'),
						  ('easy','it','ponte','una struttura construita sull''acqua'),
						  ('easy','uk','яблуко','круглий фрукт із червоною або зеленою шкіркою'),
						  ('easy','uk','міст','споруда, побудована над водою');
					 ";
				await cmd.ExecuteNonQueryAsync();

				_isInitialized = true;
			}
			finally
			{
				_dbSemaphore.Release();
			}
		}

		public async Task<WordEntry?> GetNextWordAsync(string lang, string level)
		{
			await InitializeAsync();

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
				
				// Mark word as used directly within the same transaction
				var updateCmd = conn.CreateCommand();
				updateCmd.CommandText = "UPDATE Words SET IsUsed = 1 WHERE Id = $id";
				updateCmd.Parameters.AddWithValue("$id", entry.Id);
				await updateCmd.ExecuteNonQueryAsync();

				return entry;
			}

			return null;
		}

		public async Task MarkWordUsedAsync(int id)
		{
			await InitializeAsync();

			await _dbSemaphore.WaitAsync();
			try
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
			finally
			{
				_dbSemaphore.Release();
			}
		}

		public async Task ResetUsedAsync(string lang)
		{
			await InitializeAsync();
			
			await _dbSemaphore.WaitAsync();
			try
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
			finally
			{
				_dbSemaphore.Release();
			}
		}

		public async Task<int> CountRemainingAsync(string lang, string level)
		{
			await InitializeAsync();

			await _dbSemaphore.WaitAsync();
			try
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
			finally
			{
				_dbSemaphore.Release();
			}
		}

		public async Task<WordEntry?> GetRandomWordAsync()
		{
			await InitializeAsync();

			await using var conn = new SqliteConnection($"Data Source={_dbPath}");
			await conn.OpenAsync();

			var cmd = conn.CreateCommand();
			cmd.CommandText = @"
                SELECT Id, Level, Lang, Word, Meaning, IsUsed
                  FROM Words
                 WHERE Lang = $lang
                   AND IsUsed = 0
              ORDER BY RANDOM()
                 LIMIT 1";
			cmd.Parameters.AddWithValue("$lang", AppState.SelectedLang);

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
				
				// Mark word as used directly within the same transaction
				var updateCmd = conn.CreateCommand();
				updateCmd.CommandText = "UPDATE Words SET IsUsed = 1 WHERE Id = $id";
				updateCmd.Parameters.AddWithValue("$id", entry.Id);
				await updateCmd.ExecuteNonQueryAsync();

				return entry;
			}

			// Eğer kelime kalmadıysa null döndür
			return null;
		}

		public async Task ResetAllWords()
		{
			await InitializeAsync();
			
			await _dbSemaphore.WaitAsync();
			try
			{
				await using var conn = new SqliteConnection($"Data Source={_dbPath}");
				await conn.OpenAsync();

				var cmd = conn.CreateCommand();
				cmd.CommandText = @"
	                UPDATE Words
	                   SET IsUsed = 0
	                 WHERE Lang = $lang";
				cmd.Parameters.AddWithValue("$lang", AppState.SelectedLang);

				await cmd.ExecuteNonQueryAsync();
			}
			finally
			{
				_dbSemaphore.Release();
			}
		}
	}
}
