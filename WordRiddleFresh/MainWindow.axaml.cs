using System.Threading.Tasks;
using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Data;

namespace WordRiddleFresh
{
    public partial class MainWindow : Window
    {
        private string username;
        private bool createUsername;
        public DBController database;

        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            InitializeComponent();

            this.Opened += (_, _) => txtUsername.Focus();

            //try
            //{
                database = new DBController();
                database.resetDeveloper();

                database.grabUsernames();

                createUsername = true;
                txtUsername.Focus();
            //}
            //catch (Exception ex)
            //{
            //    this.FindControl<TextBlock>("txtMessage").Text = ex.Message;
            //}
        }

        private async void Play_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
                if (string.IsNullOrWhiteSpace(txtUsername.Text))
                    throw new Exception("Please enter a username.");

                username = txtUsername.Text.Trim().ToLower();
                txtMessage.Text = "Checking username...";
                btnPlay.IsEnabled = false;
                btnCreateUser.IsEnabled = false;

                if (createUsername)
                {
                    if (database.usernames.Contains(username))
                    {
                        txtMessage.Text = "Username already exists.\nTo enter an existing username, click the button below.";
                        btnPlay.IsEnabled = true;
                        btnCreateUser.IsEnabled = true;
                        return;
                    }

                    txtMessage.Text = "Creating user...";
                    database.addUser(username);

                    // Wait for the server to process the new user
                    int retries = 0;
                    while (!database.usernames.Contains(username) && retries++ < 10)
                    {
                        await Task.Delay(300);
                        database.grabUsernames();
                    }

                    database.username = username; // Manually set username since we just created it
                }
                else
                {
                    if (!database.usernames.Contains(username))
                    {
                        txtMessage.Text = "Username does not exist.\nPlease go back to create a new username.";
                        btnPlay.IsEnabled = true;
                        btnCreateUser.IsEnabled = true;
                        return;
                    }

                    txtMessage.Text = "Loading user data...";
                    btnPlay.IsEnabled = false;
                    btnCreateUser.IsEnabled = false;

                    database.username = username;

                    await database.grabUserInfo(username);
                    await database.grabTimedUserInfo(username);

                    if (string.IsNullOrWhiteSpace(database.username))
                    {
                        txtMessage.Text = "Could not load user info.";
                        btnPlay.IsEnabled = true;
                        btnCreateUser.IsEnabled = true;
                        return;
                    }
            }

            var gameWindow = new GameWindow(database);
                gameWindow.Show();
                this.Close();
            //}
            //catch (Exception ex)
            //{
            //    txtMessage.Text = ex.Message;
            //    btnPlay.IsEnabled = true;
            //    btnCreateUser.IsEnabled = true;
            //}
        }


        private void TxtUsername_KeyDown(object sender, KeyEventArgs e)
        {
            var btnPlay = this.FindControl<Button>("btnPlay");
            if (e.Key == Key.Enter && btnPlay.IsEnabled)
            {
                Play_Click(sender, new RoutedEventArgs());
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                CreateUser_Click(sender, new RoutedEventArgs());
            }
        }

        private void CreateUser_Click(object sender, RoutedEventArgs e)
        {
            createUsername = !createUsername;

            txtMessage.Text = "";

            if (createUsername)
            {
                btnCreateUser.Content = "Enter Existing Username";
                txtUsernameMsg.Text = "Create a new username:";
            }
            else
            {
                btnCreateUser.Content = "Back";
                txtUsernameMsg.Text = "Enter your username:";
            }

            txtUsername.Focus();
        }
    }
}