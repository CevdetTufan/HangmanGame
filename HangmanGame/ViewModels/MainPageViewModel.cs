using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Maui.Views;
using HangmanGame.Views;
using HangmanGame.Utils;

namespace HangmanGame.ViewModels;

public class MainPageViewModel : INotifyPropertyChanged
{
	private bool _isExitPopupOpen = false;

	public event PropertyChangedEventHandler? PropertyChanged;
	void OnPropertyChanged([CallerMemberName] string? name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	
	public string StartGameText => "Oyuna Başla";

	public ICommand StartGameCommand { get; }

	public MainPageViewModel()
	{
		// Dili sabit olarak Türkçe ayarla
		AppState.SelectedLang = "tr";

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

	private async void OnStartGame()
	{
		AppState.WordsPlayedCount = 0;
		await Shell.Current.GoToAsync("//GamePage", new Dictionary<string, object>
		{
			{ "NewGame", true }
		});
	}
}
