using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;

namespace WordRiddleFresh
{
    public partial class EditWindow : Window
    {
        public DBController database;

        private MenuItem miEditUsername, miThemeToggle;

        // XAML controls
        //public TextBox txtNewUsername;
        //public TextBlock txtMessage;
        //public TextBlock txtNameMessage;
        //public Button btnSubmit;

        public EditWindow(DBController database)
        {
            InitializeComponent();
            this.database = database;
            WireUpControls();

            // Apply theme
            var theme = new Theme();
            theme.setTheme(this);

            // Preload existing usernames and focus input
            database.grabUsernames();
            this.Opened += (_, _) => txtNewUsername.Focus();
        }

        public EditWindow() { }

        private void InitializeComponent()
            => AvaloniaXamlLoader.Load(this);

        private void WireUpControls()
        {
            txtNewUsername = this.FindControl<TextBox>("txtNewUsername");
            txtMessage = this.FindControl<TextBlock>("txtMessage");
            txtNameMessage = this.FindControl<TextBlock>("txtNameMessage");
            btnSubmit = this.FindControl<Button>("btnSubmit");

            // … your existing FindControl<Button> and FindControl<TextBox> calls …

            // now grab the MenuItems by name:
            miEditUsername = this.FindControl<MenuItem>("miEditUsername");
            miThemeToggle = this.FindControl<MenuItem>("miThemeToggle");
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            var entry = txtNewUsername.Text?.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(entry))
            {
                txtMessage.Text = "Please enter a username.";
                return;
            }

            if (database.usernames.Contains(entry))
            {
                txtMessage.Text = "Username already exists.";
            }
            else
            {
                database.editUsername(entry);
                database.grabUsernames();
                Close();
            }
        }

        private void TxtNewUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Submit_Click(btnSubmit, new RoutedEventArgs());
        }

    }
}
