# 🎯 Hangman Game

A modern and user-friendly Hangman game application. Developed using .NET MAUI, this entertaining word guessing game runs on multiple platforms including Android, iOS, macOS, and Windows.

## ✨ Features

### 🎮 Game Features
- **Turkish Word Pool**: Play with Turkish words and their meanings
- **Visual Hangman**: Dynamic hangman drawings created with SkiaSharp
- **Sound Effects**: Audio feedback for correct/incorrect guesses
- **Background Music**: Playable background music during gameplay
- **Audio Control**: Music on/off functionality
- **Scoring System**: Earn points for successful guesses
- **6 Lives**: 6 wrong guess attempts per game

### 🎨 User Interface
- **Modern Design**: Interface designed with Material Design principles
- **Turkish Keyboard**: Custom keyboard layout supporting Turkish characters
- **Responsive Design**: Adaptable to different screen sizes
- **Social Media Links**: Social media icons on the main page

### 💾 Data Management
- **SQLite Database**: Local word database
- **Word Tracking**: Track used words
- **Auto Reset**: Automatic reset when word pool is exhausted
- **Score Persistence**: Permanent storage of total score

## 🛠️ Technologies

- **.NET MAUI 9.0**: Multi-platform application development
- **C#**: Programming language
- **SQLite**: Local database
- **SkiaSharp**: 2D graphics rendering
- **Plugin.Maui.Audio**: Audio management
- **CommunityToolkit.Maui**: UI components
- **MVVM Pattern**: Model-View-ViewModel architecture

## 📱 Supported Platforms

- ✅ Android (API 21+)
- ✅ iOS (15.0+)
- ✅ macOS (15.0+)
- ✅ Windows (10.0.17763.0+)

## 🚀 Installation

### Requirements
- Visual Studio 2022 17.8 or higher
- .NET 9.0 SDK
- MAUI Workload

### Steps
1. Clone the repository:
```bash
git clone https://github.com/yourusername/HangmanGame.git
```

2. Navigate to the project directory:
```bash
cd HangmanGame
```

3. Open `HangmanGame.sln` in Visual Studio

4. Select target platform (Android, iOS, Windows, macOS)

5. Build and run the project (F5)

## 🎯 How to Play

1. **Main Page**: Click "Start Game" button
2. **Word Guessing**: Guess the word based on the hint shown on screen
3. **Letter Selection**: Select letters from the Turkish keyboard
4. **Correct Guess**: Correct letters are revealed in the word
5. **Wrong Guess**: Hangman drawing progresses, lives decrease
6. **Game End**: Win by finding the word, lose after 6 wrong guesses

## 📁 Project Structure

```
HangmanGame/
├── Data/
│   └── WordRepository.cs          # Database operations
├── Models/
│   ├── LanguageOption.cs          # Language options (unused)
│   └── WordEntry.cs               # Word model
├── ViewModels/
│   ├── GameViewModel.cs           # Game logic
│   └── MainPageViewModel.cs       # Main page logic
├── Views/
│   ├── GamePage.xaml              # Game page
│   ├── ConfirmationPopup.xaml     # Confirmation popup
│   └── ResultPopup.xaml           # Result popup
├── Utils/
│   ├── AppState.cs                # Application state
│   └── CollectionExtensions.cs    # Collection extensions
└── Resources/
    ├── Raw/                       # Audio files
    ├── Images/                    # Images
    └── Localization/              # Language files
```

## 🎨 Customization

### Adding New Words
You can add new words in the `InitializeAsync` method of `WordRepository.cs`:

```csharp
cmd.CommandText = @"
    INSERT OR IGNORE INTO Words (Level, Lang, Word, Meaning) VALUES
    ('easy','tr','new_word','meaning of the new word');
";
```

### Audio Files
Add new audio files to the `Resources/Raw/` folder:
- `background.mp3`: Background music
- `correct.mp3`: Correct guess sound
- `wrong.mp3`: Wrong guess sound

## 🤝 Contributing

1. Fork this repository
2. Create a new branch (`git checkout -b feature/new-feature`)
3. Commit your changes (`git commit -am 'Add new feature'`)
4. Push to the branch (`git push origin feature/new-feature`)
5. Create a Pull Request

## 📄 License

This project is licensed under the MIT License. See the `LICENSE` file for details.

## 👨‍💻 Developer

**Wordivio** - [Website](https://www.wordivio.com) | [Facebook](https://www.facebook.com/wordivio) | [Instagram](https://www.instagram.com/wordivio) | [Twitter/X](https://www.x.com/wordivio) | [TikTok](https://www.tiktok.com/@wordivioapp)

## 🎮 Screenshots

*Game screenshots will be added here*

---

⭐ Don't forget to star this project if you liked it!
