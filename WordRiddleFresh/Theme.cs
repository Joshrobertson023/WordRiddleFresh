using System.Linq;
using Avalonia.Controls;
using Avalonia.Media;

namespace WordRiddleFresh
{
    /// <summary>
    /// This class manages the themes of the game
    /// </summary>
    public class Theme
    {
        // Light‐mode brushes
        public readonly IBrush LIGHT_MODE_BACKGROUND =
            new SolidColorBrush(Color.FromArgb(255, 254, 254, 251)).ToImmutable();
        public readonly IBrush LIGHT_MODE_WRONG =
            new SolidColorBrush(Color.FromArgb(255, 255, 99, 71)).ToImmutable();
        public readonly IBrush LIGHT_MODE_IN_WORD =
            new SolidColorBrush(Color.FromArgb(255, 245, 232, 86)).ToImmutable();
        public readonly IBrush LIGHT_MODE_IN_SPOT =
            new SolidColorBrush(Color.FromArgb(255, 153, 205, 50)).ToImmutable();

        public readonly IBrush DARK_MODE_BACKGROUND =
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

        /// <summary>
        /// Apply theme to the main GameWindow
        /// </summary>
        public void setTheme(GameWindow window)
        {
            //currentTheme = window.database.theme;
            //background = currentTheme == 0 ? LIGHT_MODE_BACKGROUND : DARK_MODE_BACKGROUND;
            //foreground = currentTheme == 0 ? Brushes.Black : Brushes.White;

            //// Window background
            //window.Background = background;

            //// Reset and color each board cell
            //foreach (var cell in window.uiBoard.Cast<TextBox>())
            //{
            //    cell.Background = Brushes.Transparent;
            //    cell.Foreground = foreground;
            //}

            //// Color on‐screen keyboard
            //foreach (var row in window.uiKeyboard.Children.OfType<WrapPanel>())
            //{
            //    foreach (var btn in row.Children.OfType<Button>())
            //    {
            //        btn.Background = background;
            //        btn.Foreground = foreground;
            //    }
            //}

            //// Color control buttons
            //foreach (var btn in new[]
            //{
            //    window.btnNewGame, window.btnPopup, window.btnHint,
            //    window.btnChangePlayer, window.btnSettings,
            //    window.btnDebug, window.btnGamemode, window.btnStart
            //})
            //{
            //    btn.Background = background;
            //    btn.Foreground = foreground;
            //}

            //// Color status textblocks
            //foreach (var txt in new[]
            //{
            //    window.txtMessage, window.txtGameInfo,
            //    window.txtTimer, window.txtHints,
            //    window.txtRemainingHints
            //})
            //{
            //    txt.Background = background;
            //    txt.Foreground = foreground;
            //}

            //// Re‐apply cell and keyboard highlight colors
            //window.UpdateCharacterColors();
            //window.UpdateUsedLetterBoard();
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
                tab.Background = background;
                tab.Foreground = foreground;
            }

            // Color main elements
            window.Background = background;
            tc.Background = background;
            tc.Foreground = foreground;
            dg1.Background = background;
            dg1.Foreground = foreground;
            dg2.Background = background;
            dg2.Foreground = foreground;
        }

        /// <summary>
        /// Apply theme to the EditUsername window
        /// </summary>
        public void setTheme(EditWindow window)
        {
            //currentTheme = window.database.theme;
            //background = currentTheme == 0 ? LIGHT_MODE_BACKGROUND : DARK_MODE_BACKGROUND;
            //foreground = currentTheme == 0 ? Brushes.Black : Brushes.White;

            //window.Background = background;
            //window.txtNameMessage.Background = background;
            //window.txtNameMessage.Foreground = foreground;
            //window.txtNewUsername.Background = background;
            //window.txtNewUsername.Foreground = foreground;
            //window.txtMessage.Background = background;
            //window.txtMessage.Foreground = foreground;
            //window.btnSubmit.Background = background;
            //window.btnSubmit.Foreground = foreground;
        }
    }
}
