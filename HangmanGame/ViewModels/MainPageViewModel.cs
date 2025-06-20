using HangmanGame.Models;
using HangmanGame.Utils;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace HangmanGame.ViewModels;

public class MainPageViewModel : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;
	void OnPropertyChanged([CallerMemberName] string? name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

	public string LanguageSelectText => Resources.Localization.Strings.SelectLanguage;
	public string StartGameText => Resources.Localization.Strings.StartGame;

	private LanguageOption _selectedLang = new() { DisplayName = string.Empty, Code = string.Empty };
	public LanguageOption SelectedLang
	{
		get => _selectedLang;
		set
		{
			if (_selectedLang?.Code == value?.Code) return;

			_selectedLang = value ?? CultureSelector.GetDefaultLanguage();

			OnPropertyChanged();
			CultureSelector.SetCulture(_selectedLang.Code);

			// Notify changes  
			OnPropertyChanged(nameof(LanguageSelectText));
			OnPropertyChanged(nameof(StartGameText));
		}
	}

	public ObservableCollection<LanguageOption> Languages { get; }

	public ICommand StartGameCommand { get; }

	public MainPageViewModel()
	{
		Languages = CultureSelector.GetAvailableLanguages().ToObservableCollection();
		SelectedLang = Languages.First(x => x.Code == "en");
		StartGameCommand = new Command(OnStartGame);
	}

	private async void OnStartGame()
	{
		AppState.SelectedLang = SelectedLang.Code;
		AppState.WordsPlayedCount = 0;
		await Shell.Current.GoToAsync("//GamePage");
	}
}
