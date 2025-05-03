using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using WordRiddleFresh;

namespace WordRiddleFresh
{
    public partial class GameWindow : Window
    {
        public DBController database;
        private GamePage gamePage;


        public GameWindow(DBController db)
        {
            InitializeComponent();
            database = db;

            new Theme().setTheme(this);

            gamePage = new GamePage(this, db);
            this.FindControl<ContentControl>("MainContent").Content = gamePage;

        }

        public GameWindow() { }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void ShowMainGameContent()
        {
            //MainContent.Content = gamePage;
            MainContent.Content = gamePage;


        }

        public void ShowSettingsPage()
        {
            MainContent.Content = new SettingsPage(this);
        }
    }
}
