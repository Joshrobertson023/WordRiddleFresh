using System.Threading.Tasks;
using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace WordRiddleFresh
{
    public partial class MainWindow : Window
    {
        private string username;
        private bool createUsername;
        public DBController database;

        public MainWindow()
        {
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

                if (createUsername)
                {
                    if (database.usernames.Contains(username))
                    {
                        txtMessage.Text = "Username already exists.\nTo enter an existing username, click the button below.";
                        return;
                    }
                    txtMessage.Text = "Creating user...";
                    btnPlay.IsEnabled = false;
                    btnCreateUser.IsEnabled = false;
                    database.addUser(username);
                }
                else
                {
                    if (!database.usernames.Contains(username))
                    {
                        txtMessage.Text = "Username does not exist.\nPlease go back to create a new username.";
                        return;
                    }
                    txtMessage.Text = "Loading user data...";
                    btnPlay.IsEnabled = false;
                    btnCreateUser.IsEnabled = false;
                }

                await Task.Run(() =>
                {
                    database.grabUserInfo(username);
                    database.grabTimedUserInfo();
                });

                var gameWindow = new GameWindow(database);
                gameWindow.Show();
                this.Close();
            //}
            //catch (Exception ex)
            //{
            //    msg.Text = ex.Message;
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