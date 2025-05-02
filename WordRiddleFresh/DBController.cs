using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;

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

    /// <summary>
    /// Constructor
    /// </summary>
    public DBController()
    {
    }

    /// <summary>
    /// Grab all the usernames
    /// </summary>
    public void grabUsernames()
    {
        usernames.Clear();
        DataTable dt = new DataTable();
        string query = @"SELECT username FROM WORDLELEADERBOARD";
        using OracleConnection conn = new OracleConnection(ConnectionString);
        using OracleDataAdapter da = new OracleDataAdapter(query, conn);
        da.Fill(dt);
        foreach (DataRow row in dt.Rows)
            usernames.Add(row.Field<string>("username"));
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
    public void addUser(string username, int hints = 0, string time = "00:00", 
        int guesses = 0, int won = 0, int theme = 0, int score = 0)
    {
        username = username.Trim().ToLower();
        using OracleConnection conn = new OracleConnection(ConnectionString);
        conn.Open();
        string query = @"    
                    BEGIN
                      INSERT INTO WORDLELEADERBOARD (username, hints, time, guesses, won, theme, score)
                      VALUES (:username, :hints, :time, :guesses, :won, :theme, :score);
                      INSERT INTO TIMEDLEADERBOARD (username, time, guesses, won, score)
                      VALUES (:username, :time, :guesses, :won, :score);
                    END;";
        using OracleCommand cmd = new OracleCommand(query, conn);
        cmd.BindByName = true;
        cmd.Parameters.Add(new OracleParameter("username", username));
        cmd.Parameters.Add(new OracleParameter("hints", hints));
        cmd.Parameters.Add(new OracleParameter("time", time));
        cmd.Parameters.Add(new OracleParameter("guesses", guesses));
        cmd.Parameters.Add(new OracleParameter("won", won));
        cmd.Parameters.Add(new OracleParameter("theme", theme));
        cmd.Parameters.Add(new OracleParameter("score", score));
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Get the info of a user
    /// </summary>
    /// <param name="user"></param>
    public void grabUserInfo(string user)
    {
        DataTable dt = new DataTable();
        using OracleConnection conn = new OracleConnection(ConnectionString);
        conn.Open();
        string query = @"SELECT time, username, guesses, hints, won, theme 
                         FROM WORDLELEADERBOARD WHERE username = :username";
        using OracleCommand cmd = new OracleCommand(query, conn);
        cmd.BindByName = true;
        cmd.Parameters.Add(new OracleParameter("username", user));
        using OracleDataAdapter da = new OracleDataAdapter(cmd);
        da.Fill(dt);

        username = dt.Rows[0].Field<string>("username");
        timeElapsed = dt.Rows[0].Field<string>("time");
        guesses = Convert.ToInt32(dt.Rows[0]["guesses"]);
        hints = Convert.ToInt32(dt.Rows[0]["hints"]);
        won = Convert.ToInt32(dt.Rows[0]["won"]);
        theme = Convert.ToInt32(dt.Rows[0]["theme"]);

        grabNormalScore();
    }

    /// <summary>
    /// Get the user's score for normal mode
    /// </summary>
    private void grabNormalScore()
    {
        DataTable dt = new DataTable();
        using OracleConnection conn = new OracleConnection(ConnectionString);
        conn.Open();
        string query = @"SELECT score FROM WORDLELEADERBOARD WHERE username = :username";
        using OracleCommand cmd = new OracleCommand(query, conn);
        cmd.BindByName = true;
        cmd.Parameters.Add(new OracleParameter("username", username));
        using OracleDataAdapter da = new OracleDataAdapter(cmd);
        da.Fill(dt);
        scoreNormal = Convert.ToInt32(dt.Rows[0]["score"]);
    }

    /// <summary>
    /// Get the user's info for timed mode
    /// </summary>
    public void grabTimedUserInfo()
    {
        DataTable dt = new DataTable();
        using OracleConnection conn = new OracleConnection(ConnectionString);
        conn.Open();
        string query = @"SELECT time, guesses, won FROM TIMEDLEADERBOARD WHERE username = :username";
        using OracleCommand cmd = new OracleCommand(query, conn);
        cmd.BindByName = true;
        cmd.Parameters.Add(new OracleParameter("username", this.username));
        using OracleDataAdapter da = new OracleDataAdapter(cmd);
        da.Fill(dt);

        if (dt.Rows.Count > 0)
        {
            timeTimed = dt.Rows[0].Field<string>("time");
            guesses = Convert.ToInt32(dt.Rows[0]["guesses"]);
            won = Convert.ToInt32(dt.Rows[0]["won"]);
        }
    }

    /// <summary>
    /// Get the user's score for timed mode
    /// </summary>
    public void grabTimedScore()
    {
        DataTable dt = new DataTable();
        using OracleConnection conn = new OracleConnection(ConnectionString);
        conn.Open();
        string query = @"SELECT score FROM TIMEDLEADERBOARD WHERE username = :username";
        using OracleCommand cmd = new OracleCommand(query, conn);
        cmd.BindByName = true;
        cmd.Parameters.Add(new OracleParameter("username", username));
        using OracleDataAdapter da = new OracleDataAdapter(cmd);
        da.Fill(dt);
        scoreTimed = Convert.ToInt32(dt.Rows[0]["score"]);
    }

    /// <summary>
    /// Update a user's info
    /// </summary>
    /// <param name="timeElapsed"></param>
    /// <param name="guesses"></param>
    /// <param name="rankPercentage"></param>
    public void updateUserInfo(string timeElapsed, int guesses, int score)
    {
        this.timeElapsed = timeElapsed;
        this.guesses = guesses;
        using OracleConnection conn = new OracleConnection(ConnectionString);
        conn.Open();
        string query = @"UPDATE WORDLELEADERBOARD SET time = :timeElapsed, guesses = :guesses, score = :score 
                         WHERE username = :username";
        using OracleCommand cmd = new OracleCommand(query, conn);
        cmd.BindByName = true;
        cmd.Parameters.Add(new OracleParameter("timeElapsed", timeElapsed));
        cmd.Parameters.Add(new OracleParameter("guesses", guesses));
        cmd.Parameters.Add(new OracleParameter("score", score));
        cmd.Parameters.Add(new OracleParameter("username", username));
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Update a user's info for timed mode
    /// </summary>
    /// <param name="time"></param>
    /// <param name="guesses"></param>
    /// <param name="score"></param>
    public void updateUserInfoTimed(string time, int guesses, int score)
    {
        this.timeTimed = time;
        this.guessesTimed = guesses;
        using OracleConnection conn = new OracleConnection(ConnectionString);
        conn.Open();
        string query = @"UPDATE TIMEDLEADERBOARD SET time = :time, guesses = :guesses, score = :score 
                         WHERE username = :username";
        using OracleCommand cmd = new OracleCommand(query, conn);
        cmd.BindByName = true;
        cmd.Parameters.Add(new OracleParameter("time", timeTimed));
        cmd.Parameters.Add(new OracleParameter("guesses", guessesTimed));
        cmd.Parameters.Add(new OracleParameter("score", score));
        cmd.Parameters.Add(new OracleParameter("username", username));
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Edit a user's username
    /// </summary>
    /// <param name="newUsername"></param>
    public void editUsername(string newUsername)
    {
        using OracleConnection conn = new OracleConnection(ConnectionString);
        conn.Open();
        string query = @"BEGIN
                            UPDATE WORDLELEADERBOARD SET username = :newUsername WHERE username = :username;
                            UPDATE TIMEDLEADERBOARD SET username = :newUsername WHERE username = :username;
                         END;";
        using OracleCommand cmd = new OracleCommand(query, conn);
        cmd.BindByName = true;
        cmd.Parameters.Add(new OracleParameter("newUsername", newUsername));
        cmd.Parameters.Add(new OracleParameter("username", username));
        cmd.ExecuteNonQuery();
        username = newUsername;
    }

    /// <summary>
    /// Toggle a user's theme
    /// </summary>
    /// <param name="newUsername"></param>
    public void setTheme()
    {
        using OracleConnection conn = new OracleConnection(ConnectionString);
        conn.Open();
        string query = @"UPDATE WORDLELEADERBOARD 
                        SET theme = CASE 
                                        WHEN theme = 1 THEN 0 
                                        ELSE 1 
                                    END 
                        WHERE username = :username";
        using OracleCommand cmd = new OracleCommand(query, conn);
        cmd.BindByName = true;
        cmd.Parameters.Add(new OracleParameter("username", username));
        cmd.ExecuteNonQuery();
        if (theme == 0) theme = 1;
        else theme = 0;
    }

    /// <summary>
    /// Set the user having a normal game won
    /// </summary>
    public void setWin()
    {
        won = 1;
        using OracleConnection conn = new OracleConnection(ConnectionString);
        conn.Open();
        string query = @"UPDATE WORDLELEADERBOARD SET won = :won WHERE username = :username";
        using OracleCommand cmd = new OracleCommand(query, conn);
        cmd.BindByName = true;
        cmd.Parameters.Add(new OracleParameter("won", won));
        cmd.Parameters.Add(new OracleParameter("username", username));
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Set the user having a timed game won
    /// </summary>
    public void setWinTimed()
    {
        wonTimed = 1;
        using OracleConnection conn = new OracleConnection(ConnectionString);
        conn.Open();
        string query = @"UPDATE TIMEDLEADERBOARD SET won = :won WHERE username = :username";
        using OracleCommand cmd = new OracleCommand(query, conn);
        cmd.BindByName = true;
        cmd.Parameters.Add(new OracleParameter("won", wonTimed));
        cmd.Parameters.Add(new OracleParameter("username", username));
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Set the number of hints used
    /// </summary>
    /// <param name="hints"></param>
    public void setHints(int hints)
    {
        using OracleConnection conn = new OracleConnection(ConnectionString);
        conn.Open();
        string query = @"UPDATE WORDLELEADERBOARD SET hints = :hints WHERE username = :username";
        using OracleCommand cmd = new OracleCommand(query, conn);
        cmd.Parameters.Add(new OracleParameter("username", username));
        cmd.Parameters.Add(new OracleParameter("hints", hints));
        cmd.BindByName = true;
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Grab the leaderboard for normal mode
    /// </summary>
    /// <param name="dt"></param>
    public void grabLeaderboard(DataTable dt)
    {
        string query = @"SELECT RANK() OVER (ORDER BY score) AS rank,
                        username, time, guesses, hints, score
                        FROM WORDLELEADERBOARD WHERE won = 1 
                        AND username NOT LIKE 'josher152003'";
        using OracleConnection conn = new OracleConnection(ConnectionString);
        using OracleDataAdapter da = new OracleDataAdapter(query, conn);
        da.Fill(dt);
    }

    /// <summary>
    /// Grab the leaderboard for timed mode
    /// </summary>
    /// <param name="dt"></param>
    public void grabTimedLeaderboard(DataTable dt)
    {
        string query = @"SELECT RANK() OVER (ORDER BY score) AS rank,
                        username, time, guesses, score
                        FROM TIMEDLEADERBOARD WHERE won = 1";
        using OracleConnection conn = new OracleConnection(ConnectionString);
        using OracleDataAdapter da = new OracleDataAdapter(query, conn);
        da.Fill(dt);
    }

    /// <summary>
    /// Reset the developer's info for debugging
    /// </summary>
    /// <param name="newUsername"></param>
    public void resetDeveloper()
    {
        using OracleConnection conn = new OracleConnection(ConnectionString);
        conn.Open();
        string query = @"BEGIN
                            DELETE FROM WORDLELEADERBOARD WHERE username = 'josher152003';
                            DELETE FROM TIMEDLEADERBOARD WHERE username = 'josher152003';
                         END;";
        using OracleCommand cmd = new OracleCommand(query, conn);
        cmd.BindByName = true;
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Grab all users
    /// </summary>
    /// <param name="dt"></param>
    public void grabAllUsers(DataTable dt)
    {
        string query = @"SELECT username, time, guesses, hints FROM WORDLELEADERBOARD";
        using OracleConnection conn = new OracleConnection(ConnectionString);
        using OracleDataAdapter da = new OracleDataAdapter(query, conn);
        da.Fill(dt);
    }

    /// <summary>
    /// Alter the database
    /// </summary>
    /// <param name="dt"></param>
    public void editDatabase(DataTable dt)
    {
        using OracleConnection conn = new OracleConnection(ConnectionString);
        conn.Open();
        string query = @"ALTER TABLE TIMEDLEADERBOARD ADD score INT";
        using OracleCommand cmd = new OracleCommand(query, conn);
        cmd.BindByName = true;
        cmd.ExecuteNonQuery();
    }
}
