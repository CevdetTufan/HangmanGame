using HangmanGame.Data;
using HangmanGame.Models.HangmanGame.Models;
using HangmanGame.Utils;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace HangmanGame.ViewModels
{
	public class GameViewModel : INotifyPropertyChanged
	{

		public ImageSource HangmanImageSource { get; private set; }

		public event PropertyChangedEventHandler? PropertyChanged;
		void OnPropertyChanged([CallerMemberName] string? name = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

		// UI metinleri
		public string GameTitleText => Resources.Localization.Strings.GameTitle;
		public string HintText => _currentWord?.Meaning ?? string.Empty;
		public string WordDisplay =>
			string.Join(" ", _currentWord?.Word
								  .Select(c => _guessedLetters.Contains(c) ? c.ToString() : "_")
								  ?? new[] { string.Empty });
		public string HangmanImage => $"hangman_{6 - RemainingTries}.png";
		public string ScoreText =>
			string.Format(Resources.Localization.Strings.ScoreFormat, CurrentScore);
		public string TriesText =>
			string.Format(Resources.Localization.Strings.TriesFormat, RemainingTries);

		// Dil-dinamik klavye satırları
		public List<List<string>> KeyboardRows { get; private set; } = new();

		private WordEntry? _currentWord;
		private readonly WordRepository _repo;
		private readonly HashSet<char> _guessedLetters = new();

		private int RemainingTries = 6;
		private int CurrentScore = 0;

		public ICommand GuessCommand { get; }


		public GameViewModel()
		{
			_repo = new WordRepository();
			GuessCommand = new Command<string>(OnGuess);

			// Başlangıçta seçili dile göre klavye oluştur
			BuildKeyboard(AppState.SelectedLang);

			// İlk kelimeyi yükle
			LoadNextWord();
		}

		private void BuildKeyboard(string code)
		{
			var rows = new List<List<string>>();
			switch (code)
			{
				case "de": // Almanca QWERTZ
					rows.Add(new() { "Q", "W", "E", "R", "T", "Z", "U", "I", "O", "P" });
					rows.Add(new() { "A", "S", "D", "F", "G", "H", "J", "K", "L" });
					rows.Add(new() { "Y", "X", "C", "V", "B", "N", "M" });
					break;
				case "fr": // Fransızca AZERTY
					rows.Add(new() { "A", "Z", "E", "R", "T", "Y", "U", "I", "O", "P" });
					rows.Add(new() { "Q", "S", "D", "F", "G", "H", "J", "K", "L", "M" });
					rows.Add(new() { "W", "X", "C", "V", "B", "N" });
					break;
				case "es": // İspanyolca QWERTY + Ñ
					rows.Add(new() { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "Ñ" });
					rows.Add(new() { "A", "S", "D", "F", "G", "H", "J", "K", "L" });
					rows.Add(new() { "Z", "X", "C", "V", "B", "N", "M" });
					break;
				case "it": // İtalyanca QWERTY
					rows.Add(new() { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" });
					rows.Add(new() { "A", "S", "D", "F", "G", "H", "J", "K", "L" });
					rows.Add(new() { "Z", "X", "C", "V", "B", "N", "M" });
					break;
				case "tr": // Türkçe QÜERTY + ÖÇĞÜŞİ
					rows.Add(new() { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "Ğ", "Ü" });
					rows.Add(new() { "A", "S", "D", "F", "G", "H", "J", "K", "L", "Ş", "İ" });
					rows.Add(new() { "Z", "X", "C", "V", "B", "N", "M", "Ö", "Ç" });
					break;
				case "uk": // Ukraynaca Kiril
					rows.Add(new() { "Й", "Ц", "У", "К", "Е", "Н", "Г", "Ш", "Щ", "З", "Х", "Ї" });
					rows.Add(new() { "Ф", "І", "В", "А", "П", "Р", "О", "Л", "Д", "Ж", "Є" });
					rows.Add(new() { "Я", "Ч", "С", "М", "И", "Т", "Ь", "Б", "Ю" });
					break;
				default:   // İngilizce QWERTY
					rows.Add(new() { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" });
					rows.Add(new() { "A", "S", "D", "F", "G", "H", "J", "K", "L" });
					rows.Add(new() { "Z", "X", "C", "V", "B", "N", "M" });
					break;
			}

			KeyboardRows = rows;
			OnPropertyChanged(nameof(KeyboardRows));
		}

		private async void LoadNextWord()
		{
			var level = GetLevel(AppState.WordsPlayedCount);
			_currentWord = await _repo.GetDummyWordAsync();
			RemainingTries = 6;
			_guessedLetters.Clear();

			OnPropertyChanged(nameof(HintText));
			OnPropertyChanged(nameof(WordDisplay));
			OnPropertyChanged(nameof(HangmanImage));
			OnPropertyChanged(nameof(ScoreText));
			OnPropertyChanged(nameof(TriesText));
		}

		private async void OnGuess(string letter)
		{
			char c = letter[0];
			if (_guessedLetters.Contains(c)) return;
			_guessedLetters.Add(c);

			if (!_currentWord!.Word.Contains(c)) RemainingTries--;
			else CurrentScore += 10;

			// UI güncelle
			OnPropertyChanged(nameof(WordDisplay));
			OnPropertyChanged(nameof(HangmanImage));
			OnPropertyChanged(nameof(ScoreText));
			OnPropertyChanged(nameof(TriesText));

			// Sonuç sayfasına yönlendir
			if (RemainingTries <= 0)
				await Shell.Current.GoToAsync($"//result?win=false&answer={_currentWord.Word}");
			else if (_currentWord.Word.All(ch => _guessedLetters.Contains(ch)))
			{
				AppState.WordsPlayedCount++;
				await Shell.Current.GoToAsync($"//result?win=true&answer={_currentWord.Word}");
			}
		}

		private static string GetLevel(int count)
		{
			if (count < 3) return "easy";
			if (count < 6) return "medium";
			return "hard";
		}

		private int _currentStep;
		public int CurrentStep
		{
			get => _currentStep;
			set
			{
				if (SetProperty(ref _currentStep, value))
					StepChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public event EventHandler? StepChanged;

		// Örnek:
		public void IncreaseStep()
		{
			if (CurrentStep < 8)
				CurrentStep++;
		}

		protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
		{
			if (EqualityComparer<T>.Default.Equals(backingStore, value))
				return false;

			backingStore = value;
			OnPropertyChanged(propertyName);
			return true;
		}
	}
}
