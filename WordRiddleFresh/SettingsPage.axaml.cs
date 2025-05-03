using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace WordRiddleFresh
{
    public partial class SettingsPage : UserControl
    {
        private GameWindow gameWindow;

        public SettingsPage(GameWindow owner)
        {
            try
            {
                gameWindow = owner ?? throw new ArgumentNullException(nameof(owner));
                InitializeComponent();
            }
            catch (Exception ex)
            {
                Console.WriteLine("SettingsPage constructor crash: " + ex.Message);
                throw;
            }
        }


        //public SettingsPage(GameWindow owner)
        //{
        //    gameWindow = owner;
        //    if (gameWindow.database == null)
        //        throw new Exception("gameWindow.database is null");

        //    InitializeComponent();
        //}

        //private void Back_Click(object? sender, RoutedEventArgs e)
        //{
        //    gameWindow.ShowMainGameContent();
        //}

        private async void EditUsername_Click(object? sender, RoutedEventArgs e)
        {
            //var editWindow = new EditWindow(gameWindow.database);
            //editWindow.ShowDialog(gameWindow);
            //gameWindow.UpdateUsernameDisplay(); // optional
        }

        //private void InitializeComponent()
        //{
        //    AvaloniaXamlLoader.Load(this);
        //}
    }
}