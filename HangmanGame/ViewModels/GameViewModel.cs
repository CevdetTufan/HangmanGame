using HangmanGame.Data;
using HangmanGame.Models.HangmanGame.Models;
using HangmanGame.Utils;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Plugin.Maui.Audio;

namespace HangmanGame.ViewModels
{
	public class GameViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;
		public event EventHandler<(bool Win, string Answer)>? GameOver;
		public event EventHandler? NewGameStarted;
		void OnPropertyChanged([CallerMemberName] string? name = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

		// UI metinleri
		//public string GameTitleText => Resources.Localization.Strings.GameTitle;
		public string HintText => _currentWord?.Meaning ?? string.Empty;

		public string WordDisplay =>
			string.Join(" ", _currentWord?.Word.ToUpperInvariant()
				.Select(c => _guessedLetters.Contains(c) ? c.ToString() : "_")
				?? new[] { string.Empty });

		public string HangmanImage => $"hangman_{6 - RemainingTries}.png";
		public string Score => _totalScore.ToString();
		public string TriesText => RemainingTries.ToString();

		private bool _isMusicOn = true;
		public bool IsMusicOn
		{
			get => _isMusicOn;
			set
			{
				if (SetProperty(ref _isMusicOn, value))
				{
					OnPropertyChanged(nameof(SoundIcon));
				}
			}
		}

		public string SoundIcon => IsMusicOn ? "\ue050" : "\ue04f"; // Material Icons: volume_up / volume_off

		// Dil-dinamik klavye satırları
		public List<List<string>> KeyboardRows { get; private set; } = new();
		public List<string> KeyboardLetters { get; private set; } = new();

		private WordEntry? _currentWord;

		public string CurrentAnswer =>
				_currentWord?.Word.ToUpperInvariant() ?? string.Empty;

		public bool IsCorrectLetter(string letter) =>
			CurrentAnswer.Contains(letter.ToUpperInvariant());

		private readonly WordRepository _repo;
		private readonly HashSet<char> _guessedLetters = new();

		private int RemainingTries = 6;
		private int _totalScore = 0;
		private int _roundScore = 0;

		private readonly IAudioManager _audioManager;
		private IAudioPlayer? _backgroundMusicPlayer;
		private IAudioPlayer? _correctSoundPlayer;
		private IAudioPlayer? _wrongSoundPlayer;

		public ICommand GuessCommand { get; }
		public ICommand ToggleMusicCommand { get; }

		public GameViewModel(IAudioManager audioManager)
		{
			_audioManager = audioManager;
			_repo = new WordRepository();
			GuessCommand = new Command<string>(OnGuess);
			ToggleMusicCommand = new Command(OnToggleMusic);
			_totalScore = Preferences.Get("TotalScore", 0);
		}

		public void RefreshKeyboard()
		{
			BuildKeyboard(); 
			OnPropertyChanged(nameof(KeyboardLetters));
		}


		private void BuildKeyboard()
		{
			KeyboardRows = new List<List<string>>
			{
				new() { "A", "B", "C", "Ç", "D", "E", "F", "G" },
				new() { "Ğ", "H", "I", "İ", "J", "K", "L", "M" },
				new() { "N", "O", "Ö", "P", "R", "S", "Ş", "T" },
				new() { "U", "Ü", "V", "Y", "Z" }
			};
			OnPropertyChanged(nameof(KeyboardRows));
		}


		public async Task ResetAndLoadNewWordAsync()
		{
			await _repo.InitializeAsync();
			
			// Yeni kelime yükle
			var word = await _repo.GetRandomWordAsync();
			if (word != null)
			{
				_currentWord = word;
				_guessedLetters.Clear();
				RemainingTries = 6;
				_roundScore = _currentWord.Word.Length * 10;
				CurrentStep = 0;
				
				// Klavyeyi yeniden oluştur
				BuildKeyboard();
				
				OnPropertyChanged(nameof(HintText));
				OnPropertyChanged(nameof(WordDisplay));
				OnPropertyChanged(nameof(Score));
				OnPropertyChanged(nameof(TriesText));
				OnPropertyChanged(nameof(KeyboardLetters));

				NewGameStarted?.Invoke(this, EventArgs.Empty);
			}
			else
			{
				// Kelime kalmadıysa özel bir olay tetikle
				GameOver?.Invoke(this, (true, "TEBRİKLER! BÜTÜN KELİMELERİ BİLDİNİZ!"));
			}
		}

		public async Task ResetAllWordsAndLoadNewWordAsync()
		{
			await _repo.InitializeAsync();
			
			// Tüm kelimeleri sıfırla
			await _repo.ResetAllWords();
			
			// Yeni kelime yükle
			var word = await _repo.GetRandomWordAsync();
			if (word != null)
			{
				_currentWord = word;
				_guessedLetters.Clear();
				RemainingTries = 6;
				_roundScore = _currentWord.Word.Length * 10;
				CurrentStep = 0;
				
				// Klavyeyi yeniden oluştur
				BuildKeyboard();
				
				OnPropertyChanged(nameof(HintText));
				OnPropertyChanged(nameof(WordDisplay));
				OnPropertyChanged(nameof(Score));
				OnPropertyChanged(nameof(TriesText));
				OnPropertyChanged(nameof(KeyboardLetters));

				NewGameStarted?.Invoke(this, EventArgs.Empty);
			}
		}

		private void OnGuess(string letter)
		{
			char c = char.ToUpperInvariant(letter[0]);

			if (_guessedLetters.Contains(c))
				return;

			_guessedLetters.Add(c);

			if (!_currentWord!.Word.ToUpperInvariant().Contains(c))
			{
				RemainingTries--;
				_roundScore = Math.Max(0, _roundScore - 10);
				if (IsMusicOn) _wrongSoundPlayer?.Play();
				IncreaseStep();
			}
			else
			{
				if (IsMusicOn) _correctSoundPlayer?.Play();
			}

			OnPropertyChanged(nameof(WordDisplay));
			OnPropertyChanged(nameof(HangmanImage));  
			OnPropertyChanged(nameof(TriesText));

			if (RemainingTries <= 0)
			{
				GameOver?.Invoke(this, (false, _currentWord!.Word));
			}
			else if (_currentWord.Word.ToUpperInvariant().All(ch => _guessedLetters.Contains(ch)))
			{
				_totalScore += _roundScore;
				Preferences.Set("TotalScore", _totalScore);
				OnPropertyChanged(nameof(Score));

				AppState.WordsPlayedCount++;
				GameOver?.Invoke(this, (true, _currentWord.Word));
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

		private void OnToggleMusic(object? obj)
		{
			IsMusicOn = !IsMusicOn;

			if (IsMusicOn)
			{
				_backgroundMusicPlayer?.Play();
			}
			else
			{
				_backgroundMusicPlayer?.Pause();
			}
		}
	}
}
