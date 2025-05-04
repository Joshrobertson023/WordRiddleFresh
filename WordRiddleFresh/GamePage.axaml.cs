// GameWindow.axaml.cs
using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.Media;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace WordRiddleFresh
{
    public partial class GamePage : UserControl
    {
        // core game objects
        private Game game;
        private Theme theme;
        public DBController database;

        // UI timers & state
        private DispatcherTimer timer;
        private DateTime startTime, timedLimit;
        private const int TIMED_LIMIT_MINUTES = 1, TIMED_LIMIT_SECONDS = 0;
        private const string BACKSPACE = "\u21b5", ENTER = "\u232B";
        private static readonly char[] validLetters = "abcdefghijklmnopqrstuvwxyz".ToCharArray();

        // controls (only a few shown -- add the rest)
        public TextBox[,] uiBoard;

        // game meta
        private string player, elapsedTime;
        private int numGuesses, oldScore, score, gameMode, UIkeysPressed;
        private bool debug, won, gameOver, gamePaused, timedWon;

        private Grid gameMainGrid;


        private GameWindow gameWindow;

        public GamePage(GameWindow owner, DBController database)
        {
                InitializeComponent();
            Dispatcher.UIThread.Post(() => this.Focus());
            gameWindow = owner;

                uiBoard = new TextBox[6, 5];
                string[] rows = { "A", "B", "C", "D", "E", "F" };
                for (int r = 0; r < 6; r++)
                    for (int c = 0; c < 5; c++)
                        uiBoard[r, c] = this.FindControl<TextBox>($"{rows[r]}{c + 1}");

                txtMessage = this.FindControl<TextBlock>("txtMessage");
                txtTimer = this.FindControl<TextBlock>("txtTimer");
                txtHints = this.FindControl<TextBlock>("txtHints");
                txtRemainingHints = this.FindControl<TextBlock>("txtRemainingHints");
                txtGameInfo = this.FindControl<TextBlock>("txtGameInfo");
                btnHint = this.FindControl<Button>("btnHint");
                btnStart = this.FindControl<Button>("btnStart");
                btnDebug = this.FindControl<Button>("btnDebug");
                btnPopup = this.FindControl<Button>("btnPopup");
                btnSettings = this.FindControl<Button>("btnSettings");
                btnEnter = this.FindControl<Button>("btnEnter");
                btnBackspace = this.FindControl<Button>("btnBackspace");
                btnLightDarkMode = this.FindControl<Button>("btnLightDarkMode");
                btnNewGame = this.FindControl<Button>("btnNewGame");
                btnChangePlayer = this.FindControl<Button>("btnChangePlayer");
                uiKeyboard = this.FindControl<StackPanel>("uiKeyboard");
                btnNormal = this.FindControl<Button>("btnNormal");
                btnTimed = this.FindControl<Button>("btnTimed");
                instructionPopup = this.FindControl<Popup>("instructionPopup");
                txtGamemode = this.FindControl<TextBlock>("txtGamemode");
                gameMainGrid = this.FindControl<Grid>("GamePageRootGrid");


                btnEnter.Content = ENTER;
                btnBackspace.Content = BACKSPACE;

                btnNormal.IsEnabled = false;

                if (!Design.IsDesignMode)
                {
                    this.database = database;

                    InitializeGame();
                }
        }

        public GamePage() { }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void InitializeGame()
        {
            debug = false;
            player = database.username;
            player = char.ToUpper(player[0]) + player.Substring(1);

            theme = new Theme();
            theme.setTheme(this);

            ShowInstructionPopup();
            StartNewGame();
        }

        private void StartNewGame()
        {
            game = new Game();
            won = false;
            gameOver = false;
            gamePaused = false;
            numGuesses = 0;
            game.hints = 0;

            if (btnHint == null)
            {
                Console.WriteLine("btnHint was not found. Check x:Name in XAML.");
                return;
            }
            btnHint.IsHitTestVisible = true;
            btnHint.Cursor = new Cursor(StandardCursorType.Arrow);
            btnStart.Opacity = 0;
            btnStart.IsHitTestVisible = false;
            btnStart.IsEnabled = false;

            txtTimer.Text = "00:00";
            txtMessage.Text = "";
            txtHints.Text = "";
            txtRemainingHints.Text = "Remaining hints: " + game.maxHints;
            SetDebugInfo();

            UpdateUIBoardCharacters();
            UpdateCharacterColors();
            game.usedLetters.Clear();
            game.usedLettersChars.Clear();
            game.updateUsedLetters();
            UpdateUsedLetterBoard();

            if (database.username == "josher152003")
                btnDebug.IsVisible = true;

            if (gameMode == 1)
            {
                timedWon = false;
                gamePaused = true;
                txtMessage.Text = "";
                game.maxHints = 0;
                btnStart.Opacity = 1;
                btnStart.IsHitTestVisible = true;
                btnStart.IsEnabled = true;
                txtRemainingHints.Text = "Hints disabled";
                btnHint.IsHitTestVisible = false;
                if (timer != null)
                {
                    timer.Stop();
                    timer.Tick -= Timer_Tick;
                    timer = null;
                }
                timedLimit = DateTime.Now.AddMinutes(TIMED_LIMIT_MINUTES).AddSeconds(TIMED_LIMIT_SECONDS);
                txtTimer.Text = $"{TIMED_LIMIT_MINUTES:00}:{TIMED_LIMIT_SECONDS:00}";
                txtTimer.Foreground = database.theme == 0 ? Brushes.Black : Brushes.White;
            }
            else
            {
                SetupStopwatch();
                btnHint.Cursor = new Cursor(StandardCursorType.Hand);
            }
        }

        private void SetupStopwatch()
        {
            startTime = DateTime.Now;
            timedLimit = DateTime.Now.AddMinutes(TIMED_LIMIT_MINUTES).AddSeconds(TIMED_LIMIT_SECONDS);
            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (game == null)
                return;

            if (gameMode == 0)
            {
                txtTimer.Text = (DateTime.Now - startTime).ToString("mm\\:ss");
            }
            else
            {
                var remaining = timedLimit - DateTime.Now;
                if (remaining <= TimeSpan.Zero)
                {
                    timedWon = false;
                    gamePaused = gameOver = true;
                    timer.Stop();
                    txtMessage.Text = $"Time's up! The word was {game.chosenWord}\nPress any key to start a new game.";
                    btnStart.Opacity = 0;
                    btnStart.IsHitTestVisible = false;
                    btnStart.IsEnabled = false;
                    SetHighScore();
                }
                else
                {
                    if (remaining <= TimeSpan.FromSeconds(11))
                    {
                        txtTimer.Foreground = Brushes.Red;
                        txtTimer.FontSize = 18;
                    }
                    txtTimer.Text = remaining.ToString("mm\\:ss");
                }
            }
        }

        private void HandleKeyPress(object? sender, KeyEventArgs e)
        {
            if (game == null)
                return;

            char keyPressed = '\0';
            if (e.Key == Key.Enter) keyPressed = '\r';
            else if (e.Key == Key.Back) keyPressed = '\b';
            else if (e.Key.ToString().Length == 1) keyPressed = e.Key.ToString().ToLower()[0];

            UIkeysPressed = 0;
            //txtType.Text = "";
            IterateGame(keyPressed);
        }

        private void LetterClicked(object sender, RoutedEventArgs e)
        {
            if (game == null)
                return;

            if (sender is Button b && b.Content is string s && char.TryParse(s.ToLower(), out var c))
            {
                UIkeysPressed++;
                IterateGame(c);
            }
        }

        private void EnterClicked(object sender, RoutedEventArgs e)
        {
            if (game == null)
                return;

            if (sender is Button b && b.Content is string s)
            {
                UIkeysPressed++;
                IterateGame('\b');
            }
        }

        private void BackspaceClicked(object sender, RoutedEventArgs e)
        {
            if (game == null)
                return;

            if (sender is Button b && b.Content is string s)
            {
                UIkeysPressed++;
                IterateGame('\r');
            }
        }

        private void IterateGame(char letter)
        {
            if (game == null)
                return;
            try
            {
                if ((gameOver && gamePaused) || gameOver)
                {
                    StartNewGame();
                    return;
                }

                if (letter == '\b')
                {
                    game.deleteLetter();
                    UpdateUIBoardCharacters();
                }
                else if (letter == '\r')
                {
                    if (!game.board.rowIsFull())
                        throw new Exception("Complete the word first.");
                    if (!game.checkWord())
                        throw new Exception("Not a valid word.");

                    won = game.checkGuess();
                    numGuesses++;

                    UpdateCharacterColors();
                    UpdateUIBoardCharacters();
                    game.updateUsedLetters();
                    UpdateUsedLetterBoard();

                    if (won)
                    {
                        if (gameMode == 1)
                            timedWon = true;
                        timer.Stop();
                        SetHighScore();
                        gameOver = true;
                        txtMessage.Text = $"Congratulations! You guessed the word!\nTime: {txtTimer.Text}\nPress any key to start a new game.";
                    }
                    else if (game.board.activeRow < game.board.NUM_ROWS)
                    {
                        txtMessage.Text = "Wrong word. Try again!";
                    }
                    else
                    {
                        timer.Stop();
                        txtMessage.Text = $"Game over! The word was {game.chosenWord}\nPress any key to start a new game.";
                        gameOver = true;
                    }
                }
                else
                {
                    if (!validLetters.Contains(letter))
                        throw new Exception("The key you pressed was not a letter.");
                    if (gamePaused) return;

                    game.addLetter(letter);
                    UpdateUIBoardCharacters();
                }

                if (UIkeysPressed > 5)
                {
                    //txtType.Text = "You can also use your keyboard…";
                    UIkeysPressed = 0;
                }
            }
            catch (Exception ex)
            {
                txtMessage.Text = ex.Message;
            }
        }

        public void UpdateUIBoardCharacters()
        {
            if (game == null)
                return;

            for (int r = 0; r < game.board.NUM_ROWS; r++)
                for (int c = 0; c < game.board.NUM_COLUMNS; c++)
                {
                    if (game.board.board[0, 0] == null)
                        Console.WriteLine("Board cell [0,0] is null");
                    Console.WriteLine($"game: {game != null}");
                    Console.WriteLine($"game.board: {game.board != null}");
                    Console.WriteLine($"game.board.board: {game.board.board != null}");
                    Console.WriteLine($"game.board.board[{r},{c}]: {game.board.board[r, c] != null}");
                    uiBoard[r, c].Text = game.board.board[r, c].character.ToString().ToUpper();

                }
        }

        public void UpdateCharacterColors()
        {
            if (game == null)
                return;

            for (int r = 0; r < game.board.NUM_ROWS; r++)
                for (int c = 0; c < game.board.NUM_COLUMNS; c++)
                {
                    var cell = game.board.board[r, c];
                    var box = uiBoard[r, c];
                    if (cell.isInCorrectPosition)
                        box.Background = database.theme == 0
                            ? theme.LIGHT_MODE_IN_SPOT
                            : theme.DARK_MODE_IN_SPOT;
                    else if (cell.isInWord)
                        box.Background = database.theme == 0
                            ? theme.LIGHT_MODE_IN_WORD
                            : theme.DARK_MODE_IN_WORD;
                    else if (cell.character == ' ')
                        box.Background = Brushes.Transparent;
                    else
                        box.Background = database.theme == 0
                            ? theme.LIGHT_MODE_WRONG
                            : theme.DARK_MODE_WRONG;
                }
        }

        public void UpdateUsedLetterBoard()
        {
            if (game == null)
                return;

            var rows = new[]
            {
        this.FindControl<WrapPanel>("firstRow"),
        this.FindControl<WrapPanel>("secondRow"),
        this.FindControl<WrapPanel>("thirdRow")
    };

            foreach (var row in rows)
            {
                if (row == null)
                {
                    Console.WriteLine("Warning: A row (firstRow, secondRow, or thirdRow) was not found.");
                    continue;
                }

                foreach (Button b in row.Children.OfType<Button>())
                {
                    string content = b.Content?.ToString()?.ToLower() ?? "";
                    if (content.Length != 1) continue; // Skip "Enter", "Back", etc.

                    var ch = content[0];
                    var used = game.usedLetters.FirstOrDefault(u => u.character == ch);

                    if (used != null)
                    {
                        b.Background = used.isInCorrectPosition
                            ? (database.theme == 0 ? theme.LIGHT_MODE_IN_SPOT : theme.DARK_MODE_IN_SPOT)
                            : used.isInWord
                                ? (database.theme == 0 ? theme.LIGHT_MODE_IN_WORD : theme.DARK_MODE_IN_WORD)
                                : (database.theme == 0 ? theme.LIGHT_MODE_WRONG : theme.DARK_MODE_WRONG);
                    }
                    else
                    {
                        // 🟡 This sets background to theme's default even for unused letters
                        b.Background = database.theme == 0 ? theme.LIGHT_MODE_BACKGROUND : theme.DARK_MODE_BACKGROUND;
                    }

                    // Always set the foreground
                    b.Foreground = database.theme == 0 ? Brushes.Black : Brushes.White;
                }
            }
        }




        private void SetDebugInfo()
        {
            if (game == null)
                return;

            txtGameInfo.Text = debug
                ? $"Word to guess: {game.chosenWord}   |   Player: {player}"
                : $"Player: {player}";
        }

        /// <summary>
        /// Check for a new high score and push it to the database
        /// </summary>
        private void SetHighScore()
        {
            if (game == null)
                return;

            int timeScore, guessScore, hintScore, maxTimeScore;
            guessScore = numGuesses * 20;
            elapsedTime = txtTimer.Text;

            if (gameMode == 0)
            {
                // Normal mode
                timeScore = (int.Parse(elapsedTime.Substring(0, 2)) * 60)
                          + int.Parse(elapsedTime.Substring(3, 2));
                hintScore = game.hints * 50;
                score = timeScore + guessScore + hintScore;
                oldScore = database.scoreNormal;
                gameOver = true;

                if (score < oldScore)
                {
                    database.updateUserInfo(elapsedTime, numGuesses, score);
                    database.setWin();
                    database.setHints(game.hints);
                    txtHints.Text =
                        "+-+-+-+- Score +-+-+-+-\n" +
                        $"   Time:    {timeScore}\n" +
                        $"   Guesses: {guessScore}\n" +
                        $"   Hints:   {hintScore}\n" +
                        "   -----------------\n" +
                        $"   Total:   {score}\n" +
                        $"   Previous high score: {oldScore}\n" +
                        "   You got a new high score!\n" +
                        "   Check out the leaderboard!";
                }
                else if (database.won == 0)
                {
                    database.updateUserInfo(elapsedTime, numGuesses, score);
                    database.setWin();
                    database.setHints(game.hints);
                    txtHints.Text =
                        "+-+-+-+- Score +-+-+-+-\n" +
                        $"   Time:    {timeScore}\n" +
                        $"   Guesses: {guessScore}\n" +
                        $"   Hints:   {hintScore}\n" +
                        "   -----------------\n" +
                        $"   Total:   {score}\n" +
                        "   You won your first game!\n" +
                        "   Check out the leaderboard!";
                }
                else
                {
                    txtHints.Text =
                        "+-+-+-+- Score +-+-+-+-\n" +
                        $"   Time:    {timeScore}\n" +
                        $"   Guesses: {guessScore}\n" +
                        $"   Hints:   {hintScore}\n" +
                        "   -----------------\n" +
                        $"   Total:   {score}\n" +
                        $"   Previous high score: {oldScore}\n" +
                        "   (lower is better)";
                }
            }
            else
            {
                // Timed mode
                maxTimeScore = TIMED_LIMIT_MINUTES * 60 + TIMED_LIMIT_SECONDS;
                timeScore = maxTimeScore
                             - ((int.Parse(elapsedTime.Substring(0, 2)) * 60)
                             + int.Parse(elapsedTime.Substring(3, 2)));
                score = timeScore + guessScore;
                oldScore = database.scoreTimed;
                gamePaused = gameOver = true;

                if (timedWon && score < oldScore && score > 0 && txtTimer.Text != "00:00")
                {
                    database.updateUserInfoTimed(elapsedTime, numGuesses, score);
                    database.setWinTimed();
                    database.scoreTimed = score;
                    txtHints.Text =
                        "+-+-+-+- Score +-+-+-+-\n" +
                        $"   Time:    {timeScore}\n" +
                        $"   Guesses: {guessScore}\n" +
                        "   -----------------\n" +
                        $"   Total:   {score}\n" +
                        $"   Previous high score: {oldScore}\n" +
                        "   You got a new high score!";
                }
                else if (timedWon && database.wonTimed == 0 && txtTimer.Text != "00:00")
                {
                    database.updateUserInfoTimed(elapsedTime, numGuesses, score);
                    database.setWinTimed();
                    database.scoreTimed = score;
                    txtHints.Text =
                        "+-+-+-+- Score +-+-+-+-\n" +
                        $"   Time:    {timeScore}\n" +
                        $"   Guesses: {guessScore}\n" +
                        "   -----------------\n" +
                        $"   Total:   {score}\n" +
                        "   You won your first timed game!\n" +
                        "   Check out the leaderboard!";
                }
                else if (timedWon)
                {
                    txtHints.Text =
                        "+-+-+-+- Score +-+-+-+-\n" +
                        $"   Time:    {timeScore}\n" +
                        $"   Guesses: {guessScore}\n" +
                        "   -----------------\n" +
                        $"   Total:   {score}\n" +
                        $"   Previous high score: {oldScore}\n" +
                        "   (lower is better)";
                }
            }
        }

        private async void btnPopup_Click(object sender, RoutedEventArgs e)
        {
            var popup = new Leaderboard(database);
            // modal over this window:
            popup.ShowDialog(gameWindow);
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            timer?.Stop();
            StartNewGame();
        }

        private void Debug_Click(object sender, RoutedEventArgs e)
        {
            debug = !debug;
            SetDebugInfo();
        }

        private void ChangePlayer_Click(object sender, RoutedEventArgs e)
        {
            var main = new MainWindow();
            main.Show();
            gameWindow.Close();
        }

        private void Hint_Click(object sender, RoutedEventArgs e)
        {
            if (game == null)
                return;

            try
            {
                txtHints.Text += game.useHint();
                txtRemainingHints.Text = $"Remaining hints: {game.maxHints - game.hints}";
            }
            catch (Exception ex)
            {
                txtMessage.Text = ex.Message;
            }
        }

        private async void EditUsername_Click(object? sender, RoutedEventArgs e)
        {
            var editWindow = new EditWindow(database);
            await editWindow.ShowDialog(gameWindow);
            player = char.ToUpper(database.username[0]) + database.username.Substring(1);
            SetDebugInfo();
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            gameWindow.ShowSettingsPage();
        }

        private void NormalMode_Click(object sender, RoutedEventArgs e)
        {
            gameMode = 0;
            btnNormal.IsEnabled = false;
            btnTimed.IsEnabled = true;
            StartNewGame();
        }

        private void TimedMode_Click(object sender, RoutedEventArgs e)
        {
            gameMode = 1;
            btnStart.Opacity = 0;
            btnStart.IsHitTestVisible = false;
            btnStart.IsEnabled = false;
            btnNormal.IsEnabled = true;
            btnTimed.IsEnabled = false;
            ShowInstructionPopup();
            timedLimit = DateTime.Now
                       .AddMinutes(TIMED_LIMIT_MINUTES)
                       .AddSeconds(TIMED_LIMIT_SECONDS);
            StartNewGame();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            SetupStopwatch();
            btnStart.Opacity = 0;
            btnStart.IsHitTestVisible = false;
            btnStart.IsEnabled = false;
            txtMessage.Text = "";
            gamePaused = false;
        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            //InstructionsWindow window = new InstructionsWindow(database);
            //window.Show();
        }

        private async void btnLightDarkMode_Click(object sender, RoutedEventArgs e)
        {
            bool success = await database.setTheme();
            if (success)
            {
                theme.setTheme(this); // now uses updated database.theme
            }
        }


        private void ShowInstructionPopup()
        {
            int viewedInstructions = database.viewedInstructions;
            var messageControl = this.FindControl<TextBlock>("popupMessage");

            string normalMessage = "Welcome to Word Riddle!" +
                "\n\nThe goal of the game is to guess a random 5-letter word." +
                "\n\nThe letters will color based on how close you are to the correct word." +
                "\nGreen means the letter is in the correct position. \nYellow means it's in the " +
                "word but in the wrong spot. \nRed means it's not in the word at all." +
                "\n\nYou can click the on-screen keyboard or type on your computer's keyboard." +
                "\n\nOnce you win a round, you will be added to the leaderboard." +
                "\n\nGamemodes:" +
                "\n- Normal: You have unlimited time and a set number of hints" +
                "\n- Timed: You have a time limit and hints are disabled";

            string timedMessage = "Welcome to timed mode!" +
                "\n\nIn this mode you have a time limit to guess the word." +
                "\n\nPress START to begin the game." +
                "\n\nKeep in mind hints are disabled. Try to get on the leaderboard!";

            if (viewedInstructions == 0)
            {
                messageControl.Text = normalMessage;
                database.updateViewedInstructions(1);
            }
            else if (viewedInstructions < 2)
            {
                messageControl.Text = timedMessage;
                database.updateViewedInstructions(2);
            }
            else
            {
                return; // No popup if already viewed
            }

            instructionPopup.IsOpen = true;
        }

        private void CloseInstructionPopup(object? sender, RoutedEventArgs e)
        {
            instructionPopup.IsOpen = false;

            if (gameMode == 0)
            {
                if (timer == null)
                    SetupStopwatch();
            }
        }
    }
}