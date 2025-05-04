using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http.Json;
using WordRiddleShared;



/// <summary>
/// This class manages the database connection and operations
/// </summary>

public class DBController
{
    private static string ConnectionString = @"User Id=ADMIN;Password=Josher152003;Data Source=(DESCRIPTION=(RETRY_COUNT=20)(RETRY_DELAY=3)(ADDRESS=(PROTOCOL=TCPS)(HOST=adb.us-ashburn-1.oraclecloud.com)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=gd119454c26aea7_joshwordledb_high.adb.oraclecloud.com))(SECURITY=(SSL_SERVER_DN_MATCH=YES)));";

    public List<string> usernames = new List<string>(); // List of all usernames

    public string username;    // The current player's username
    public string timeElapsed; // The current player's time in game
    public int guesses;        // The current player's number of guesses
    public int hints;          // The current player's number of hints used
    public int won;            // The current player won a game
    public int wonTimed;       // Player won a timed game
    public int theme;          // The current player's theme
    public string timeTimed;   // Player's time in timed mode
    public int guessesTimed;   // Player's guesses in timed mode
    public int scoreNormal;    // Player's score for normal mode
    public int scoreTimed;     // Player's score for timed mode
    public int viewedInstructions; // Player viewed instructions (0 = no, 1 = normal, 2 = timed)
    public int currentTheme;


    /// <summary>
    /// Constructor
    /// </summary>
    public DBController()
    {
    }

    /// <summary>
    /// Grab all the usernames
    /// </summary>
    public async void grabUsernames()
    {
        usernames = await FetchUsernamesFromAPI();
    }

    public async Task<List<string>> FetchUsernamesFromAPI()
    {
        try
        {
            using var client = new HttpClient();
            var usernames = await client.GetFromJsonAsync<List<string>>("https://wordriddleapi-production.up.railway.app/api/user/usernames");
            return usernames ?? new List<string>();
        }
        catch (Exception ex)
        {
            Console.WriteLine("API error: " + ex.Message);
            return new List<string>();
        }
    }


    /// <summary>
    /// Add a user
    /// </summary>
    /// <param name="username"></param>
    /// <param name="hints"></param>
    /// <param name="time"></param>
    /// <param name="guesses"></param>
    /// <param name="rankPercentage"></param>
    /// <param name="won"></param>
    public async void addUser(string username, int hints = 0, string time = "00:00", 
        int guesses = 0, int won = 0, int theme = 0, int score = 0, int viewedInstructions = 0)
    {
        var user = new
        {
            username = username,
            hints = hints,
            time = time,
            guesses = guesses,
            won = won,
            theme = theme,
            score = score,
            viewedInstructions = viewedInstructions
        };

        using var client = new HttpClient();
        await client.PostAsJsonAsync("https://wordriddleapi-production.up.railway.app/api/user/adduser", user);
    }

    /// <summary>
    /// Get the info of a user
    /// </summary>
    /// <param name="user"></param>
    public async Task<bool> grabUserInfo(string username)
    {
        var userInfo = await FetchUserInfoFromAPI(username);

        if (userInfo != null)
        {
            this.username = userInfo.username;
            this.guesses = userInfo.guesses;
            this.won = userInfo.won;
            this.theme = userInfo.theme;
            this.hints = userInfo.hints;
            this.viewedInstructions = userInfo.viewedInstructions;
            this.timeElapsed = userInfo.timeElapsed;
            this.scoreNormal = userInfo.score;
            return true;
        }

        return false;
    }



    public async Task<UserInfoDto?> FetchUserInfoFromAPI(string username)
    {
        try
        {
            using var client = new HttpClient();
            string url = $"https://wordriddleapi-production.up.railway.app/api/user/info/{username.ToLower().Trim()}";

            var userInfo = await client.GetFromJsonAsync<UserInfoDto>(url);
            return userInfo;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error fetching user info: " + ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Get the user's info for timed mode
    /// </summary>
    public async Task grabTimedUserInfo(string username)
    {
        var timedInfo = await FetchTimedUserInfoFromAPI(username);

        if (timedInfo != null)
        {
            this.timeTimed = timedInfo.timeTimed;
            this.guesses = timedInfo.guesses;
            this.wonTimed = timedInfo.won;
            Console.WriteLine($"Timed info loaded for {username}: Time={timeTimed}, Guesses={guesses}, Won={won}");
        }
        else
        {
            Console.WriteLine("No timed user info found.");
        }
    }

    public async Task<TimedUserInfoDto?> FetchTimedUserInfoFromAPI(string username)
    {
        try
        {
            using var client = new HttpClient();
            string url = $"https://wordriddleapi-production.up.railway.app/api/user/timed-info/{username.ToLower().Trim()}";
            return await client.GetFromJsonAsync<TimedUserInfoDto>(url);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error fetching timed user info: " + ex.Message);
            return null;
        }
    }


    /// <summary>
    /// Get the user's score for timed mode
    /// </summary>
    public async void grabTimedScore()
    {
        this.scoreTimed = await FetchTimedScoreFromAPI(username);
        Console.WriteLine($"Timed score for {username}: {scoreTimed}");
    }

    public async Task<int> FetchTimedScoreFromAPI(string username)
    {
        try
        {
            using var client = new HttpClient();
            string url = $"https://wordriddleapi-production.up.railway.app/api/user/timed-score/{username.ToLower().Trim()}";
            var scoreDto = await client.GetFromJsonAsync<TimedScoreDto>(url);
            return scoreDto?.scoreTimed ?? 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error fetching timed score: " + ex.Message);
            return 0;
        }
    }



    /// <summary>
    /// Update a user's info
    /// </summary>
    /// <param name="timeElapsed"></param>
    /// <param name="guesses"></param>
    /// <param name="rankPercentage"></param>
    public async void updateUserInfo(string timeElapsed, int guesses, int score)
    {
        await UpdateUserInfoAPI(username, timeElapsed, guesses, score);
    }

    public async Task<bool> UpdateUserInfoAPI(string username, string timeElapsed, int guesses, int score)
    {
        var userInfo = new UpdateUserInfoDto
        {
            username = username,
            timeElapsed = timeElapsed,
            guesses = guesses,
            score = score
        };

        this.timeElapsed = timeElapsed;
        this.guesses = guesses;
        this.scoreNormal = score;

        try
        {
            using var client = new HttpClient();
            var response = await client.PutAsJsonAsync("https://wordriddleapi-production.up.railway.app/api/user/update-info", userInfo);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error updating user info: " + ex.Message);
            return false;
        }
    }


    public async void updateViewedInstructions(int viewedInstructions)
    {
        await UpdateViewedInstructionsAPI(username, viewedInstructions);
    }

    public async Task<bool> UpdateViewedInstructionsAPI(string username, int viewedInstructions)
    {
        var dto = new UpdateViewedInstructionsDto
        {
            username = username,
            viewedInstructions = viewedInstructions
        };

        this.viewedInstructions = viewedInstructions;

        try
        {
            using var client = new HttpClient();
            var response = await client.PutAsJsonAsync("https://wordriddleapi-production.up.railway.app/api/user/update-viewed-instructions", dto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error updating viewedInstructions: " + ex.Message);
            return false;
        }
    }


    /// <summary>
    /// Update a user's info for timed mode
    /// </summary>
    /// <param name="time"></param>
    /// <param name="guesses"></param>
    /// <param name="score"></param>
    public async void updateUserInfoTimed(string time, int guesses, int score)
    {
        await UpdateTimedUserInfoAPI(username, time, guesses, score);
    }

    public async Task<bool> UpdateTimedUserInfoAPI(string username, string time, int guesses, int score)
    {
        var dto = new UpdateTimedUserInfoDto
        {
            username = username,
            time = time,
            guesses = guesses,
            score = score
        };

        this.timeTimed = time;
        this.guessesTimed = guesses;
        this.scoreTimed = score;

        try
        {
            using var client = new HttpClient();
            var response = await client.PutAsJsonAsync("https://wordriddleapi-production.up.railway.app/api/user/update-timed-info", dto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error updating timed user info: " + ex.Message);
            return false;
        }
    }


    /// <summary>
    /// Edit a user's username
    /// </summary>
    /// <param name="newUsername"></param>
    public async void editUsername(string newUsername)
    {
        await EditUsernameAPI(username, newUsername);

    }

    public async Task<bool> EditUsernameAPI(string oldUsername, string newUsername)
    {
        var dto = new EditUsernameDto
        {
            oldUsername = oldUsername,
            newUsername = newUsername
        };

        this.username = newUsername; // ✅ local variable update

        try
        {
            using var client = new HttpClient();
            var response = await client.PutAsJsonAsync("https://wordriddleapi-production.up.railway.app/api/user/edit-username", dto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error updating username: " + ex.Message);
            return false;
        }
    }


    /// <summary>
    /// Toggle a user's theme
    /// </summary>
    /// <param name="newUsername"></param>
    public async Task<bool> setTheme()
    {
        if (string.IsNullOrWhiteSpace(this.username))
            return false;

        bool success = await ToggleThemeAPI(this.username);
        if (!success) return false;

        // Immediately fetch updated theme
        this.theme = await FetchThemeFromAPI(this.username);
        return true;
    }


    public async Task<bool> ToggleThemeAPI(string username)
    {
        try
        {
            using var client = new HttpClient();
            var response = await client.PutAsync($"https://wordriddleapi-production.up.railway.app/api/user/toggle-theme/{username}", null);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
                if (json != null && json.TryGetValue("theme", out int newTheme))
                {
                    Console.WriteLine("Theme updated to: " + newTheme);
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error toggling theme: " + ex.Message);
            return false;
        }
    }



    public async Task<int> FetchThemeFromAPI(string username)
    {
        try
        {
            using var client = new HttpClient();
            var response = await client.GetFromJsonAsync<Dictionary<string, int>>($"https://wordriddleapi-production.up.railway.app/api/user/theme/{username}");
            return response != null && response.TryGetValue("theme", out int theme) ? theme : 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error fetching theme: " + ex.Message);
            return 0;
        }
    }



    /// <summary>
    /// Set the user having a normal game won
    /// </summary>
    public async void setWin()
    {
        await SetWinAPI(this.username);

    }

    public async Task<bool> SetWinAPI(string username)
    {
        this.won = 1; // ✅ update local variable

        try
        {
            using var client = new HttpClient();
            var response = await client.PutAsync($"https://wordriddleapi-production.up.railway.app/api/user/set-win/{username}", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error setting win: " + ex.Message);
            return false;
        }
    }


    /// <summary>
    /// Set the user having a timed game won
    /// </summary>
    public async void setWinTimed()
    {
        await SetWinTimedAPI(this.username);

    }

    public async Task<bool> SetWinTimedAPI(string username)
    {
        this.wonTimed = 1; // ✅ update local variable

        try
        {
            using var client = new HttpClient();
            var response = await client.PutAsync($"https://wordriddleapi-production.up.railway.app/api/user/set-win-timed/{username}", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error setting timed win: " + ex.Message);
            return false;
        }
    }


    /// <summary>
    /// Set the number of hints used
    /// </summary>
    /// <param name="hints"></param>
    public async void setHints(int hints)
    {
        await SetHintsAPI(this.username, hints);

    }

    public async Task<bool> SetHintsAPI(string username, int hints)
    {
        var dto = new SetHintsDto
        {
            username = username,
            hints = hints
        };

        this.hints = hints; // ✅ update local variable

        try
        {
            using var client = new HttpClient();
            var response = await client.PutAsJsonAsync("https://wordriddleapi-production.up.railway.app/api/user/set-hints", dto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error setting hints: " + ex.Message);
            return false;
        }
    }

    public async Task<List<LeaderboardEntry>> grabLeaderboard()
    {
        var leaderboard = await FetchLeaderboardAPI();

        foreach (var entry in leaderboard)
        {
            Console.WriteLine($"{entry.Rank}. {entry.Username} - {entry.Score} pts");
        }

        return leaderboard;
    }


    public async Task<List<LeaderboardEntry>> FetchLeaderboardAPI()
    {
        try
        {
            using var client = new HttpClient();
            var entries = await client.GetFromJsonAsync<List<LeaderboardEntry>>("https://wordriddleapi-production.up.railway.app/api/user/leaderboard");
            return entries ?? new List<LeaderboardEntry>();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error fetching leaderboard: " + ex.Message);
            return new List<LeaderboardEntry>();
        }
    }



    //AND username NOT LIKE 'josher152003'

    public async Task<List<LeaderboardEntry>> grabTimedLeaderboard()
    {
        var leaderboard = await FetchTimedLeaderboardAPI();

        foreach (var entry in leaderboard)
        {
            Console.WriteLine($"{entry.Rank}. {entry.Username} - {entry.Score} pts");
        }

        return leaderboard;
    }


    public async Task<List<LeaderboardEntry>> FetchTimedLeaderboardAPI()
    {
        try
        {
            using var client = new HttpClient();
            var entries = await client.GetFromJsonAsync<List<LeaderboardEntry>>("https://wordriddleapi-production.up.railway.app/api/user/leaderboard-timed");
            return entries ?? new List<LeaderboardEntry>();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error fetching timed leaderboard: " + ex.Message);
            return new List<LeaderboardEntry>();
        }
    }


    /// <summary>
    /// Reset the developer's info for debugging
    /// </summary>
    /// <param name="newUsername"></param>
    public async void resetDeveloper()
    {
        await ResetDeveloperAPI();

    }

    public async Task<bool> ResetDeveloperAPI()
    {
        try
        {
            using var client = new HttpClient();
            var response = await client.DeleteAsync("https://wordriddleapi-production.up.railway.app/api/user/reset-developer");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error resetting developer data: " + ex.Message);
            return false;
        }
    }


    public async void grabAllUsers(DataTable dt)
    {
        await grabAllUsers2(dt);
    }

    public async Task grabAllUsers2(DataTable dt)
    {
        var users = await FetchAllUsersAPI();

        // Define table structure
        dt.Columns.Clear();
        dt.Columns.Add("username", typeof(string));
        dt.Columns.Add("time", typeof(string));
        dt.Columns.Add("guesses", typeof(int));
        dt.Columns.Add("hints", typeof(int));

        // Populate rows
        foreach (var user in users)
        {
            dt.Rows.Add(user.Username, user.Time, user.Guesses, user.Hints);
        }
    }

    public async Task<List<UserSummaryDto>> FetchAllUsersAPI()
    {
        try
        {
            using var client = new HttpClient();
            var users = await client.GetFromJsonAsync<List<UserSummaryDto>>("https://wordriddleapi-production.up.railway.app/api/user/all-users");
            return users ?? new List<UserSummaryDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error fetching all users: " + ex.Message);
            return new List<UserSummaryDto>();
        }
    }
}
