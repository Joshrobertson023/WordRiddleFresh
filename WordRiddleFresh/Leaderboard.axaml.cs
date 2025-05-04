using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using WordRiddleShared;

namespace WordRiddleFresh
{

    public class LeaderboardViewModel
    {
        public ObservableCollection<LeaderboardEntry> NormalLeaderboard { get; } = new();
        public ObservableCollection<LeaderboardEntry> TimedLeaderboard { get; } = new();

        public LeaderboardViewModel(DBController database)
        {
            LoadData(database);
        }

        public LeaderboardViewModel() { }

        public async void LoadData(DBController database)
        {
            // Fetch and populate normal leaderboard
            var normalData = await database.grabLeaderboard();
            foreach (var entry in normalData)
            {
                entry.Username = Capitalize(entry.Username);
                NormalLeaderboard.Add(entry);
            }

            // Fetch and populate timed leaderboard
            var timedData = await database.grabTimedLeaderboard();
            foreach (var entry in timedData)
            {
                entry.Username = Capitalize(entry.Username);
                TimedLeaderboard.Add(entry);
            }
        }

        // Utility method to capitalize the first letter
        private static string Capitalize(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name;
            return char.ToUpper(name[0]) + name.Substring(1);
        }
    }

    public partial class Leaderboard : Window
    {
        public DBController database;
        private DataTable dt;
        private DataTable dtT;
        Theme theme;

        public Leaderboard(DBController database)
        {
            InitializeComponent();

            this.database = database;

            theme = new Theme();
            theme.setTheme(this);

            var viewModel = new LeaderboardViewModel();
            viewModel.LoadData(database);
            DataContext = viewModel;
        }

        public Leaderboard() : this(new DBController()) { }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void OnWindowOpened(object? sender, EventArgs e)
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            await Task.Run(() =>
            {
                DataContext = new LeaderboardViewModel(new DBController());
            });
        }
    }
}