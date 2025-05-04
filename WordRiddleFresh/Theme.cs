using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Avalonia.Platform;
using Avalonia;
using System;


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
        private string buttonTheme;

        public Theme()
        {
        }

        private Style CreateHoverStyle()
        {
            var hoverBackground = currentTheme == 0
                ? new SolidColorBrush(Color.Parse("#dedede")).ToImmutable()  // ✅ Matches XAML
                : new SolidColorBrush(Color.Parse("#2d2d2d")).ToImmutable();

            var hoverForeground = currentTheme == 0
                ? Brushes.Black
                : Brushes.White;

            return new Style(x => x.OfType<Button>().Class(":pointerover"))
            {
                Setters =
        {
            new Setter(Button.BackgroundProperty, hoverBackground),
            new Setter(Button.ForegroundProperty, hoverForeground)
        }
            };
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
            buttonTheme = currentTheme == 0 ? "light" : "dark";

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
                        btn.Classes.Remove("light");
                        btn.Classes.Remove("dark");
                        btn.Classes.Add(buttonTheme);

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
                    btn.Classes.Remove("light");
                    btn.Classes.Remove("dark");
                    btn.Classes.Add(buttonTheme);

                }
            }

            window.btnLightDarkMode.Background = Brushes.Transparent;
            window.btnLightDarkMode.Classes.Remove("light");
            window.btnLightDarkMode.Classes.Remove("dark");
            window.btnLightDarkMode.Classes.Add(buttonTheme);

            var img = window.FindControl<Image>("imgThemeIcon");
            if (img != null)
            {
                var iconUri = currentTheme == 0
                    ? "avares://WordRiddleFresh/Assets/light-mode.png"
                    : "avares://WordRiddleFresh/Assets/dark-mode.png";


                var uri = new Uri(currentTheme == 0
    ? "avares://WordRiddleFresh/Assets/light-mode.png"
    : "avares://WordRiddleFresh/Assets/dark-mode.png");

                using var stream = AssetLoader.Open(uri);
                img.Source = new Avalonia.Media.Imaging.Bitmap(stream);
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


            // Remove any previous hover styles
            var existingHoverStyles = window.Styles
                .Where(s => s is Style style && style.Selector?.ToString()?.Contains(":pointerover") == true)
                .ToList();

            foreach (var style in existingHoverStyles)
            {
                window.Styles.Remove(style);
            }

            // Now add the correct hover style for the current theme
            window.Styles.Add(CreateHoverStyle());
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

            // Update tab style
            foreach (var tab in tc.Items.OfType<TabItem>())
            {
                tab.Background = Brushes.Transparent;
                tab.Foreground = foreground;
            }

            // Set shared DataGrid column header style
            Style MakeHeaderStyle(IBrush foreground) => new(x => x.OfType<DataGridColumnHeader>())
            {
                Setters =
                    {
                        new Setter(DataGridColumnHeader.BackgroundProperty, Brushes.Transparent),
                        new Setter(DataGridColumnHeader.ForegroundProperty, foreground)
                    }
            };

            dg1.Styles.Add(MakeHeaderStyle(foreground));
            dg2.Styles.Add(MakeHeaderStyle(foreground));



            // Backgrounds
            window.Background = currentTheme == 0 ? WINDOW_LIGHT_MODE_BACKGROUND : WINDOW_DARK_MODE_BACKGROUND;
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
            buttonTheme = currentTheme == 0 ? "light" : "dark";

            window.Background = currentTheme == 0 ? WINDOW_LIGHT_MODE_BACKGROUND : WINDOW_DARK_MODE_BACKGROUND;

            // Apply styles to text-related controls
            window.txtNameMessage.Background = Brushes.Transparent;
            window.txtNameMessage.Foreground = foreground;

            window.txtMessage.Background = Brushes.Transparent;
            window.txtMessage.Foreground = foreground;

            // Apply button style and classes
            window.btnSubmit.Background = background;
            window.btnSubmit.Foreground = foreground;
            window.btnSubmit.Classes.Remove("light");
            window.btnSubmit.Classes.Remove("dark");
            window.btnSubmit.Classes.Add(buttonTheme);

            // Remove existing hover styles (to prevent stacking)
            var existingHoverStyles = window.Styles
                .Where(s => s is Style style && style.Selector?.ToString()?.Contains(":pointerover") == true)
                .ToList();

            foreach (var style in existingHoverStyles)
            {
                window.Styles.Remove(style);
            }

            // Add the correct hover style for this window
            window.Styles.Add(CreateHoverStyle());
        }

    }
}
