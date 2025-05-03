using System.Linq;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;


namespace WordRiddleFresh
{
    /// <summary>
    /// This class manages the themes of the game
    /// </summary>
    public class Theme
    {
        // Light‐mode brushes
        public readonly IBrush LIGHT_MODE_BACKGROUND =
            new SolidColorBrush(Color.FromArgb(255, 211, 211, 211)).ToImmutable();
        public readonly IBrush WINDOW_LIGHT_MODE_BACKGROUND =
            new SolidColorBrush(Color.FromArgb(255, 254, 254, 251)).ToImmutable();
        public readonly IBrush LIGHT_MODE_WRONG =
            new SolidColorBrush(Color.FromArgb(255, 255, 99, 71)).ToImmutable();
        public readonly IBrush LIGHT_MODE_IN_WORD =
            new SolidColorBrush(Color.FromArgb(255, 245, 232, 86)).ToImmutable();
        public readonly IBrush LIGHT_MODE_IN_SPOT =
            new SolidColorBrush(Color.FromArgb(255, 153, 205, 50)).ToImmutable();

        public readonly IBrush DARK_MODE_BACKGROUND =
            new SolidColorBrush(Color.FromArgb(255, 100, 100, 100)).ToImmutable();
        public readonly IBrush WINDOW_DARK_MODE_BACKGROUND =
            new SolidColorBrush(Color.FromArgb(255, 41, 41, 41)).ToImmutable();
        public readonly IBrush DARK_MODE_WRONG =
            new SolidColorBrush(Color.FromArgb(255, 197, 68, 45)).ToImmutable();
        public readonly IBrush DARK_MODE_IN_WORD =
            new SolidColorBrush(Color.FromArgb(255, 202, 189, 62)).ToImmutable();
        public readonly IBrush DARK_MODE_IN_SPOT =
            new SolidColorBrush(Color.FromArgb(255, 147, 202, 37)).ToImmutable();

        private IBrush background;
        private IBrush foreground;
        private int currentTheme;

        public Theme()
        {
        }

        public void setTheme(GameWindow window)
        {
            if (window.database == null) return;

            currentTheme = window.database.theme;
            background = currentTheme == 0 ? LIGHT_MODE_BACKGROUND : DARK_MODE_BACKGROUND;

            // This will apply the background to the entire GameWindow
            window.Background = currentTheme == 0 ? WINDOW_LIGHT_MODE_BACKGROUND : WINDOW_DARK_MODE_BACKGROUND;
        }


        /// <summary>
        /// Apply theme to the main GameWindow
        /// </summary>
        public void setTheme(GamePage window)
        {
            if (window.database == null) return;
            currentTheme = window.database.theme;
            background = currentTheme == 0 ? LIGHT_MODE_BACKGROUND : DARK_MODE_BACKGROUND;
            foreground = currentTheme == 0 ? Brushes.Black : Brushes.White;

            // Window background
            window.Background = currentTheme == 0 ? WINDOW_LIGHT_MODE_BACKGROUND : WINDOW_DARK_MODE_BACKGROUND;

            if (window.txtTimer != null)
                window.txtTimer.Foreground = foreground;


            if (window.uiBoard != null)
            {
                foreach (var cell in window.uiBoard.Cast<TextBox>())
                {
                    if (cell != null)
                    {
                        cell.Background = Brushes.Transparent;
                        cell.Foreground = foreground;
                    }
                }
            }

            if (window.uiKeyboard != null)
            {
                var rows = new[]
{
    window.FindControl<WrapPanel>("firstRow"),
    window.FindControl<WrapPanel>("secondRow"),
    window.FindControl<WrapPanel>("thirdRow")
};

                foreach (var row in rows)
                {
                    if (row == null) continue;

                    foreach (var btn in row.Children.OfType<Button>())
                    {
                        btn.Background = background;
                        btn.Foreground = foreground;
                    }
                }


            }

            foreach (var btn in new[]
            {
        window.btnNewGame, window.btnPopup, window.btnHint,
        window.btnChangePlayer, window.btnSettings,
        window.btnDebug, window.btnStart, window.btnNormal, window.btnTimed
    })
            {
                if (btn != null)
                {
                    btn.Background = background;
                    btn.Foreground = foreground;
                }
            }

            if (window.btnLightDarkMode != null)
            {
                window.btnLightDarkMode.Background = currentTheme == 0 ? Brushes.Transparent : DARK_MODE_BACKGROUND;
            }

            foreach (var txt in new[]
            {
        window.txtMessage, window.txtGameInfo,
        window.txtTimer, window.txtHints,
        window.txtRemainingHints, window.txtGamemode
    })
            {
                if (txt != null)
                {
                    txt.Background = Brushes.Transparent;
                    txt.Foreground = foreground;
                }
            }

            try
            {
                window.UpdateCharacterColors();
                window.UpdateUsedLetterBoard();
            }
            catch
            {
                // Ignore theme update errors if game isn’t fully initialized yet
            }
        }

        /// <summary>
        /// Apply theme to the Leaderboard window
        /// </summary>
        public void setTheme(Leaderboard window)
        {
            currentTheme = window.database.theme;
            background = currentTheme == 0 ? LIGHT_MODE_BACKGROUND : DARK_MODE_BACKGROUND;
            foreground = currentTheme == 0 ? Brushes.Black : Brushes.White;

            var tc = window.FindControl<TabControl>("tabControl");
            var dg1 = window.FindControl<DataGrid>("dataLeaderboard");
            var dg2 = window.FindControl<DataGrid>("dataLeaderboardTimed");
            var txt = window.FindControl<TextBlock>("txtMessage");

            // Color each tab
            foreach (var tab in window.tabControl.Items.OfType<TabItem>())
            {
                tab.Background = Brushes.Transparent;
                tab.Foreground = foreground;
            }

            // Color main elements
            window.Background = background;
            tc.Background = Brushes.Transparent;
            tc.Foreground = foreground;
            dg1.Background = Brushes.Transparent;
            dg1.Foreground = foreground;
            dg2.Background = Brushes.Transparent;
            dg2.Foreground = foreground;
        }

        /// <summary>
        /// Apply theme to the EditUsername window
        /// </summary>
        public void setTheme(EditWindow window)
        {
            currentTheme = window.database.theme;
            background = currentTheme == 0 ? LIGHT_MODE_BACKGROUND : DARK_MODE_BACKGROUND;
            foreground = currentTheme == 0 ? Brushes.Black : Brushes.White;

            window.Background = currentTheme == 0 ? WINDOW_LIGHT_MODE_BACKGROUND : WINDOW_DARK_MODE_BACKGROUND;
            window.txtNameMessage.Background = Brushes.Transparent;
            window.txtNameMessage.Foreground = foreground;
            window.txtNewUsername.Background = Brushes.Transparent;
            window.txtNewUsername.Foreground = foreground;
            window.txtMessage.Background = Brushes.Transparent;
            window.txtMessage.Foreground = foreground;
            window.btnSubmit.Background = Brushes.Transparent;
            window.btnSubmit.Foreground = foreground;
        }
    }
}
