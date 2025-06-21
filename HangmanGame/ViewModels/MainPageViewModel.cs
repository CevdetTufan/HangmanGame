using HangmanGame.Models;
using HangmanGame.Resources.Localization;
using HangmanGame.Utils;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Maui.Views;
using HangmanGame.Views;

namespace HangmanGame.ViewModels;

public class MainPageViewModel : INotifyPropertyChanged
{
	private bool _isExitPopupOpen = false;

	public event PropertyChangedEventHandler? PropertyChanged;
	void OnPropertyChanged([CallerMemberName] string? name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

	public string LanguageSelectText => Strings.SelectLanguage;
	public string StartGameText => Strings.StartGame;

	public ObservableCollection<LanguageOption> Languages { get; }

	private LanguageOption _selectedLang;
	public LanguageOption SelectedLang
	{
		get => _selectedLang;
		set
		{
			if (_selectedLang != value)
			{
				_selectedLang = value;
				OnPropertyChanged(nameof(SelectedLang));
				CultureSelector.SetCulture(value.Code);
				UpdateLocalizedStrings();
				AppState.SelectedLang = value.Code;
				Preferences.Set("selected_language_code", value.Code);
			}
		}
	}

	public ICommand StartGameCommand { get; }

	public MainPageViewModel()
	{
		Languages = CultureSelector.GetAvailableLanguages().ToObservableCollection();

		var currentCode = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
		_selectedLang = Languages.FirstOrDefault(l => l.Code == currentCode) ?? Languages[0];
		AppState.SelectedLang = _selectedLang.Code;

		StartGameCommand = new Command(OnStartGame);
	}

	public async Task<bool> AttemptExit()
	{
		if (_isExitPopupOpen) return false;

		try
		{
			_isExitPopupOpen = true;
			var popup = new ConfirmationPopup();
			await Shell.Current.CurrentPage.ShowPopupAsync(popup);

			if (popup.Confirmed)
			{
				Application.Current.Quit();
				return true;
			}
		}
		finally
		{
			_isExitPopupOpen = false;
		}
		return false;
	}

	private void UpdateLocalizedStrings()
	{
		OnPropertyChanged(nameof(StartGameText));
		OnPropertyChanged(nameof(LanguageSelectText));
	}

	private async void OnStartGame()
	{
		AppState.SelectedLang = SelectedLang.Code;
		AppState.WordsPlayedCount = 0;
		await Shell.Current.GoToAsync("//GamePage");
	}
}
