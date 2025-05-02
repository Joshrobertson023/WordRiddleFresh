using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace WordRiddleFresh
{
    public partial class Leaderboard : Window
    {
        public DBController database;

        private readonly DataTable dt = new();
        private readonly DataTable dtT = new();

        public Leaderboard(DBController database)
        {
            this.database = database;
            InitializeComponent();
        }

        private void InitializeComponent()
            => AvaloniaXamlLoader.Load(this);

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                var normalList = this.FindControl<ItemsControl>("normalList");
                var timedList = this.FindControl<ItemsControl>("timedList");

                await Task.Run(() =>
                {
                    if (database.username == "josher152003")
                    {
                        database.grabAllUsers(dt);
                        database.grabTimedLeaderboard(dtT);
                    }
                    else
                    {
                        database.grabLeaderboard(dt);
                        database.grabTimedLeaderboard(dtT);
                    }
                });

                var normal = new List<string>();
                int rank = 1;
                foreach (DataRow row in dt.Rows)
                {
                    var name = row["username"].ToString();
                    name = char.ToUpper(name[0]) + name[1..];
                    var time = row["time"];
                    var guesses = row["guesses"];
                    var hints = row.Table.Columns.Contains("hints") ? row["hints"] : 0;
                    var score = row.Table.Columns.Contains("score") ? row["score"] : 0;

                    normal.Add($"{rank,-5} {name,-13} {time,-8} {guesses,-8} {hints,-6} {score,-5}");
                    rank++;
                }

                var timed = new List<string>();
                rank = 1;
                foreach (DataRow row in dtT.Rows)
                {
                    var name = row["username"].ToString();
                    name = char.ToUpper(name[0]) + name[1..];
                    var time = row["time"];
                    var guesses = row["guesses"];
                    var score = row["score"];

                    timed.Add($"{rank,-5} {name,-13} {time,-8} {guesses,-8} {score,-5}");
                    rank++;
                }

                normalList.ItemsSource = normal;
                timedList.ItemsSource = timed;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}");
            }
        }
    }
}
