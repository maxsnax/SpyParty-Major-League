using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text.Json.Nodes;
using System.Web;
using Microsoft.Diagnostics.Utilities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static SML.Models.Replays;
using SML.Models;

namespace SML {
    public class DatabaseHelper : DbContext {
        //public DatabaseHelper() : base("name=SML_db-connection") { }

        //// Define the DbSet for ReplayData
        //public DbSet<Replays.ReplayData> Replays { get; set; }


        private static readonly string connectionString = "Server=tcp:smldbserver.database.windows.net,1433;Initial Catalog=SML_db;Persist Security Info=False;User ID=maxsnax;Password=$yMMetry21;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        

        public static SqlConnection GetConnection() {
            return new SqlConnection(connectionString);
        }


        public bool TestConnection() {
            using SqlConnection connection = new SqlConnection(connectionString);
            try {
                connection.Open(); // Attempt to open the connection
                using (SqlCommand command = new SqlCommand("SELECT 1", connection)) {
                    command.ExecuteScalar(); // Execute a simple query
                }
                return true; // Connection is successful
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"Connection test failed: {ex.Message}");
                return false; // Connection failed
            }
        }


        // =======================================================================================
        // Fetching season and division information
        // =======================================================================================
        // Get the ID and names of all seasons in the SQL_db
        //public List<Tuple<int, string>> LoadSeasons() {
        //    List<Tuple<int, string>> seasons = new List<Tuple<int, string>>();

        //    using (SqlConnection connection = new SqlConnection(connectionString)) {
        //        try {
        //            connection.Open();
        //            using (SqlCommand command = new SqlCommand("SELECT season_ID, season_name FROM dbo.Season", connection)) {
        //                using (SqlDataReader reader = command.ExecuteReader()) {
        //                    while (reader.Read()) {
        //                        int seasonID = reader.GetInt32(0); // season_ID
        //                        string seasonName = reader.GetString(1); // season_name
        //                        seasons.Add(new Tuple<int, string>(seasonID, seasonName));
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex) {
        //            System.Diagnostics.Debug.WriteLine($"Error loading seasons: {ex.Message}");
        //        }
        //    }

        //    return seasons;
        //}


        //public List<Division> GetSeasonDivisions(int seasonID, SqlConnection connection) {
        //    List<Division> divisionList = new List<Division>();

        //    using SqlCommand command = new SqlCommand(
        //        "SELECT division_id, division_name, load_order FROM Division " +
        //        "WHERE season_id = @seasonID",
        //        connection);

        //    command.Parameters.AddWithValue("@seasonID", seasonID);

        //    System.Diagnostics.Debug.WriteLine($"Fetching all divisions for seasonID:{seasonID}");

        //    try {
        //        using (SqlDataReader reader = command.ExecuteReader()) {
        //            while (reader.Read()) {
        //                divisionList.Add(new Division {
        //                    SeasonID = seasonID,
        //                    DivisionID = reader.GetInt32(0),
        //                    DivisionName = reader.GetString(1),
        //                    LoadOrder = reader.GetInt32(2),
        //                });
        //            }
        //        }
        //    }
        //    catch (Exception ex) {
        //        System.Diagnostics.Debug.WriteLine($"Error fetching divisions for seasonID: {seasonID}\n{ex.Message}");
        //        throw;
        //    }

        //    return divisionList;
        //}


        // =======================================================================================
        // Fetch player data based on their division, name, season
        // =======================================================================================
        


        public List<Player> GetPlayerByName(string playerName, SqlConnection connection, SqlTransaction transaction) {
            playerName = playerName.Replace("/steam", "");
            
            System.Diagnostics.Debug.WriteLine($"SQL GetPlayerByName() - playerName: {playerName}");

            using SqlCommand command = new SqlCommand(
                "SELECT player_id, player_name, forfeit, division_id, season_id, username FROM Player " +
                "WHERE player_name = @playerName",
                connection, transaction);

            command.Parameters.AddWithValue("@playerName", playerName);

            List<Player> playerData = new List<Player>();

            System.Diagnostics.Debug.WriteLine($"Executing SQL Query for Player: {playerName}");
            try {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        int playerId = reader.GetInt32(0);
                        string name = reader.GetString(1);
                        int forfeit = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                        int divisionId = reader.IsDBNull(3) ? -1 : reader.GetInt32(3);
                        int seasonId = reader.IsDBNull(4) ? -1 : reader.GetInt32(4);
                        string username = reader.GetString(5);

                        System.Diagnostics.Debug.WriteLine($"Returning Player: ID={playerId}, Name={name}, Forfeit={forfeit}, Division={divisionId}, Season={seasonId}");
                        playerData.Add(new Player {
                            PlayerID = playerId,
                            Name = name,
                            Forfeit = forfeit,
                            Division = divisionId,
                            Season = seasonId,
                            Username = username
                        });
                    }

                    return playerData;
                }
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"Failed GetPlayerByName: {ex.Message}");
            }

            return null;
        }


        //public Player GetPlayerBySeason(int seasonID, string playerName, SqlConnection connection, SqlTransaction transaction) {
        //    playerName = playerName.Replace("/steam", "");

        //    System.Diagnostics.Debug.WriteLine($"SQL GetPlayerBySeason()");
        //    System.Diagnostics.Debug.WriteLine($"seasonID:{seasonID}\nplayerName:{playerName}");

        //    System.Diagnostics.Debug.WriteLine($"Creating SqlCommand command");
        //    using SqlCommand command = new SqlCommand(
        //        "SELECT player_id, player_name, forfeit, division_id, season_id, username FROM Player " +
        //        "WHERE player_name = @playerName AND season_id = @seasonID",
        //        connection, transaction);

        //    System.Diagnostics.Debug.WriteLine($"command.Parameters.AddWithValue @playerName, @seasonID");
        //    command.Parameters.AddWithValue("@playerName", playerName);
        //    command.Parameters.AddWithValue("@seasonID", seasonID);

        //    System.Diagnostics.Debug.WriteLine($"using SqlDataReader");
        //    try {
        //        using (SqlDataReader reader = command.ExecuteReader()) {
        //            if (reader.Read()) {
        //                int playerId = reader.GetInt32(0);
        //                string name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
        //                int forfeit = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
        //                int divisionId = reader.IsDBNull(3) ? -1 : reader.GetInt32(3);
        //                int seasonId = reader.IsDBNull(4) ? -1 : reader.GetInt32(4);
        //                string username = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);


        //                System.Diagnostics.Debug.WriteLine($"Returning Player: ID={playerId}, Name={name}, Forfeit={forfeit}, Division={divisionId}, Season={seasonId}");
        //                return new Player {
        //                    PlayerID = playerId,
        //                    Name = name,
        //                    Forfeit = forfeit,
        //                    Division = divisionId,
        //                    Season = seasonId,
        //                    Username = username
        //                };
        //            }
        //        }
        //    } catch (Exception ex) {
        //        System.Diagnostics.Debug.WriteLine($"Failed GetPlayerBySeason");
        //        System.Diagnostics.Debug.WriteLine($"{ex.Message}");
        //    }


        //    System.Diagnostics.Debug.WriteLine($"return null");
        //    return null;
        //}


        public List<Player> GetPlayersBySeason(string seasonName, SqlConnection connection, SqlTransaction transaction) {
            List<Player> players = new List<Player>();

            using (SqlCommand command = new SqlCommand(
                "SELECT p.player_id, p.player_name, p.division_id, p.forfeit, p.username" +
                "FROM Player p " +
                "JOIN Division d ON p.division_id = d.division_id " +
                "JOIN Season s ON d.season_id = s.season_id " +
                "WHERE s.season_name = @seasonName", connection, transaction)) {
                command.Parameters.AddWithValue("@seasonName", seasonName);

                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        players.Add(new Player {
                            PlayerID = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Season = reader.GetInt32(2),
                            Division = reader.GetInt32(3),
                            Forfeit = reader.GetInt32(4),
                            Username = reader.GetString(1)
                        });
                    }
                }
            }

            return players;
        }


        public int GetPlayerIdByName(string playerName, SqlConnection connection, SqlTransaction transaction) {
            playerName = playerName.Replace("/steam", "");

            using SqlCommand command = new SqlCommand("SELECT player_id FROM Player WHERE player_name = @playerName", connection, transaction);
            command.Parameters.AddWithValue("@playerName", playerName);
            object result = command.ExecuteScalar();
            return (result != null) ? Convert.ToInt32(result) : 0;
        }


        public int GetSeasonIdByName(string seasonName, SqlConnection connection, SqlTransaction transaction) {
            using SqlCommand command = new SqlCommand("SELECT season_id FROM Season WHERE season_name = @seasonName", connection, transaction);
            command.Parameters.AddWithValue("@season_name", seasonName);
            object season_id = command.ExecuteScalar();
            return (season_id != null) ? Convert.ToInt32(season_id) : 0;
        }


        public string GetSeasonNameById(int seasonID, SqlConnection connection, SqlTransaction transaction) {
            using SqlCommand command = new SqlCommand("SELECT season_name FROM Season WHERE season_ID = @seasonID", connection, transaction);
            command.Parameters.AddWithValue("@seasonID", seasonID);
            object season_name = command.ExecuteScalar();
            return season_name?.ToString();
        }


        public void UpdatePlayerUsernames(SqlConnection connection, SqlTransaction transaction) {
            string query = @"
                UPDATE p
                SET p.username = r.spy_username
                FROM Player p
                JOIN Replay r 
                    ON r.spy_displayname LIKE p.player_name + '%'
                WHERE p.username IS NULL OR p.username = '';
            ";

            using (SqlCommand command = new SqlCommand(query, connection, transaction)) {
                int rowsAffected = command.ExecuteNonQuery();
                System.Diagnostics.Debug.WriteLine($"Updated {rowsAffected} player usernames.");
            }
        }


        // =======================================================================================
        //  Upload replay data and check if games are already stored/matches already been played
        // =======================================================================================
        //public bool ReplayExists(string replayUuid, SqlConnection connection, SqlTransaction transaction) {
        //    using SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Replay WHERE uuid = @uuid", connection, transaction);
        //    command.Parameters.AddWithValue("@uuid", replayUuid);
        //    int count = (int)command.ExecuteScalar();
        //    return count > 0;
        //}


        //public void UploadGame(ReplayData replay, int matchID, SqlConnection connection, SqlTransaction transaction) {
        //    System.Diagnostics.Debug.WriteLine($"SQL UploadGame()");

        //    try {
        //        using SqlCommand command = new SqlCommand(
        //            "INSERT INTO Replay (spy_username, sniper_username, result, level, selected_missions, picked_missions, completed_missions, sequence_number, start_time, duration, game_type, uuid, map_variant, spy_displayname, sniper_displayname, guest_count, clock_seconds, match_id) " +
        //            "VALUES (@spy_username, @sniper_username, @result, @level, @selected_missions, @picked_missions, @completed_missions, @sequence_number, @start_time, @duration, @game_type, @uuid, @map_variant, @spy_displayname, @sniper_displayname, @guest_count, @clock_seconds, @match_id)", connection, transaction);
        //        command.Parameters.AddWithValue("@spy_username", (string)replay.spy_username);
        //        command.Parameters.AddWithValue("@sniper_username", (string)replay.sniper_username);
        //        command.Parameters.AddWithValue("@result", (string)replay.result);
        //        command.Parameters.AddWithValue("@level", (string)replay.level);
        //        command.Parameters.AddWithValue("@selected_missions", JsonConvert.SerializeObject(replay.selected_missions)); // Store JSON array
        //        command.Parameters.AddWithValue("@picked_missions", JsonConvert.SerializeObject(replay.picked_missions));
        //        command.Parameters.AddWithValue("@completed_missions", JsonConvert.SerializeObject(replay.completed_missions));
        //        command.Parameters.AddWithValue("@sequence_number", (int)replay.sequence_number);
        //        command.Parameters.AddWithValue("@start_time", (DateTime)replay.start_time);
        //        command.Parameters.AddWithValue("@duration", (int)replay.duration);
        //        command.Parameters.AddWithValue("@game_type", (string)replay.game_type);
        //        command.Parameters.AddWithValue("@uuid", (string)replay.uuid);
        //        command.Parameters.AddWithValue("@map_variant", (object)replay.map_variant ?? DBNull.Value);
        //        command.Parameters.AddWithValue("@spy_displayname", (string)replay.spy_displayname);
        //        command.Parameters.AddWithValue("@sniper_displayname", (string)replay.sniper_displayname);
        //        command.Parameters.AddWithValue("@guest_count", (int)replay.guest_count);
        //        command.Parameters.AddWithValue("@clock_seconds", (int)replay.clock_seconds);
        //        command.Parameters.AddWithValue("@match_id", matchID);

        //        command.ExecuteNonQuery();
        //    }
        //    // Unique Constraint Violation (Duplicate UUID)
        //    catch (SqlException ex) when (ex.Number == 2627) {
        //        string errorMessage = $"Upload failed: A replay with the same UUID ({replay.uuid}) already exists.";
        //        System.Diagnostics.Debug.WriteLine(errorMessage);

        //        // Throw a custom exception to be handled by the calling function
        //        throw new InvalidOperationException(errorMessage);
        //    }
        //    catch (Exception ex) {
        //        System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
        //        throw;
        //    }
        //}


        public int CreateMatch(Player playerOne, Player playerTwo, SqlConnection connection, SqlTransaction transaction) {
            System.Diagnostics.Debug.WriteLine($"SQL CreateMatch()");

            try {
                int divisionID = playerOne.Division == playerTwo.Division ? playerOne.Division : -1;
                int? winner = null; // Nullable int

                // Handle if one or both players have forfeited the season
                int forfeit = playerOne.Forfeit + playerTwo.Forfeit;

                System.Diagnostics.Debug.WriteLine($"Creating match for {playerOne.Name} vs {playerTwo.Name}");
                System.Diagnostics.Debug.WriteLine($"{playerOne.Name}\n:ID={playerOne.PlayerID}:Div={playerOne.Division}:Forfeit={playerOne.Forfeit}:Username{playerOne.Username}\n");
                System.Diagnostics.Debug.WriteLine($"{playerTwo.Name}\n:ID={playerTwo.PlayerID}:Div={playerTwo.Division}:Forfeit={playerTwo.Forfeit}:Username{playerTwo.Username}\n");

                if (playerOne.Results.Points_Won > playerTwo.Results.Points_Won && forfeit != 0) {
                    winner = playerOne.PlayerID;
                }
                else if (playerOne.Results.Points_Won < playerTwo.Results.Points_Won && forfeit != 0) {
                    winner = playerTwo.PlayerID;
                }
                else if (forfeit == 1) {
                    winner = playerTwo.PlayerID;
                }
                else if (forfeit == 2) {
                    winner = playerOne.PlayerID;
                }
                // winner remains NULL if it's a tie or both players forfeited

                using SqlCommand command = new SqlCommand(
                    "INSERT INTO Match (division_id, player_one_id, player_one_score, player_one_name, player_two_id, player_two_score, player_two_name, winner, forfeit) " +
                    "OUTPUT INSERTED.match_id VALUES (@division_id, @player_one_id, @player_one_score, @player_one_name, @player_two_id, @player_two_score, @player_two_name, @winner, @forfeit)",
                    connection, transaction
                );

                command.Parameters.AddWithValue("@division_id", divisionID);
                command.Parameters.AddWithValue("@player_one_id", playerOne.PlayerID);
                command.Parameters.AddWithValue("@player_one_score", playerOne.Results.Points_Won);
                command.Parameters.AddWithValue("@player_one_name", playerOne.Name);
                command.Parameters.AddWithValue("@player_two_id", playerTwo.PlayerID);
                command.Parameters.AddWithValue("@player_two_score", playerTwo.Results.Points_Won);
                command.Parameters.AddWithValue("@player_two_name", playerTwo.Name);
                command.Parameters.AddWithValue("@winner", winner ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@forfeit", forfeit);

                return (int)command.ExecuteScalar();
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }


        //public List<int> GetMatchWinners(Player playerOne, Player playerTwo, SqlConnection connection, SqlTransaction transaction) {
        //    List<int> winners = new List<int>();

        //    try {
        //        using SqlCommand command = new SqlCommand(
        //            "SELECT winner FROM Match " +
        //            "WHERE ((player_one = @playerOne AND player_two = @playerTwo) " +
        //            "OR (player_one = @playerTwo AND player_two = @playerOne)) " +
        //            "AND division_id = @divisionId " +
        //            "ORDER BY match_date DESC;",
        //            connection, transaction
        //        );

        //        command.Parameters.AddWithValue("@playerOne", playerOne.Name);
        //        command.Parameters.AddWithValue("@playerTwo", playerTwo.Name);
        //        command.Parameters.AddWithValue("@divisionId", playerOne.Division);

        //        using SqlDataReader reader = command.ExecuteReader();
        //        while (reader.Read()) {
        //            if (reader["winner"] != DBNull.Value) {
        //                winners.Add(Convert.ToInt32(reader["winner"]));
        //            }
        //        }
        //    }
        //    catch (Exception ex) {
        //        System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
        //    }

        //    return winners;
        //}


        // =======================================================================================
        //  Updating player stats
        // =======================================================================================
        //public Results GetPlayerResultsByMatch(Player player, Match match, SqlConnection connection, SqlTransaction transaction) {
        //    System.Diagnostics.Debug.WriteLine($"UpdatePlayerStats: {player.Name}");

        //    if (player.Username == null) {
        //        System.Diagnostics.Debug.WriteLine($"PlayerID = 0, invalid ID");
        //        throw new ArgumentNullException(nameof(player), "Null match sent in UpdatePlayerStats");
        //    } else if (match.ID == 0) {
        //        throw new ArgumentNullException("Null match sent in UpdatePlayerStats");
        //    }

        //    System.Diagnostics.Debug.WriteLine($"Name:{player.Name}\n-User:{player.Username}\n{player.PlayerID}\n");

        //    try {
        //        string query = @"
        //            SELECT
        //                SUM(CASE WHEN result = 'Spy Shot' AND spy_username = @playerUsername THEN 1 ELSE 0 END) AS spy_spyShotCount,
        //                SUM(CASE WHEN result = 'Civilian Shot' AND spy_username = @playerUsername THEN 1 ELSE 0 END) AS spy_civilianShotCount,
        //                SUM(CASE WHEN result = 'Missions Win' AND spy_username = @playerUsername THEN 1 ELSE 0 END) AS spy_missionsWinCount,
        //                SUM(CASE WHEN result = 'Time Out' AND spy_username = @playerUsername THEN 1 ELSE 0 END) AS spy_timeOutCount,
        //                SUM(CASE WHEN result = 'Spy Shot' AND sniper_username = @playerUsername THEN 1 ELSE 0 END) AS sniper_spyShotCount,
        //                SUM(CASE WHEN result = 'Civilian Shot' AND sniper_username = @playerUsername THEN 1 ELSE 0 END) AS sniper_civilianShotCount,
        //                SUM(CASE WHEN result = 'Missions Win' AND sniper_username = @playerUsername THEN 1 ELSE 0 END) AS sniper_missionsWinCount,
        //                SUM(CASE WHEN result = 'Time Out' AND sniper_username = @playerUsername THEN 1 ELSE 0 END) AS sniper_timeOutCount
        //            FROM Replay
        //            WHERE match_id = @matchID;
        //            ";

        //        using SqlCommand command = new SqlCommand(query, connection, transaction);
        //        command.Parameters.AddWithValue("@playerUsername", player.Username);
        //        command.Parameters.AddWithValue("@matchID", match.ID);

        //        Results result = new Results();
        //        using SqlDataReader reader = command.ExecuteReader();

        //        if (reader.Read()) {
        //            result.Spy_SpyShot = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
        //            result.Spy_CivilianShot = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
        //            result.Spy_MissionsWin = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
        //            result.Spy_TimeOut = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
        //            result.Sniper_SpyShot = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
        //            result.Sniper_CivilianShot = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);
        //            result.Sniper_MissionsWin = reader.IsDBNull(6) ? 0 : reader.GetInt32(6);
        //            result.Sniper_TimeOut = reader.IsDBNull(7) ? 0 : reader.GetInt32(7);
        //        }

        //        System.Diagnostics.Debug.WriteLine($"Returning {player.Name} result: {result.ToString()}");

        //        return result;
        //    }
        //    catch (Exception ex) {
        //        System.Diagnostics.Debug.WriteLine($"Exception in GetPlayerStatsByMatch: {ex.Message}");
        //        throw;
        //    }
        //}


        //public void UpdatePlayerStatsByMatch(Player player, Results result, SqlConnection connection, SqlTransaction transaction) {
        //    System.Diagnostics.Debug.WriteLine($"UpdatePlayerStats: {player.Name}");

        //    try {
        //        string query = @"
        //            UPDATE Player
        //            SET 
        //                win = win + @Win,
        //                loss = loss + @Loss,
        //                tie = tie + @Tie,
        //                spy_shot = spy_shot + @Spy_SpyShot,
        //                spy_civilian_shot = spy_civilian_shot + @Spy_CivilianShot,
        //                spy_mission_win = spy_mission_win + @Spy_MissionsWin,
        //                spy_timeout = spy_timeout + @Spy_TimeOut,
        //                sniper_shot = sniper_shot + @Sniper_SpyShot,
        //                sniper_civilian_shot = sniper_civilian_shot + @Sniper_CivilianShot,
        //                sniper_mission_win = sniper_mission_win + @Sniper_MissionsWin,
        //                sniper_timeout = sniper_timeout + @Sniper_TimeOut 
        //            WHERE player_id = @playerID AND season_ID = @seasonID;
        //        ";

        //        using SqlCommand command = new SqlCommand(query, connection, transaction);
        //        command.Parameters.AddWithValue("@Win", result.Win);
        //        command.Parameters.AddWithValue("@Loss", result.Loss);
        //        command.Parameters.AddWithValue("@Tie", result.Tie);
        //        command.Parameters.AddWithValue("@Spy_SpyShot", result.Spy_SpyShot);
        //        command.Parameters.AddWithValue("@Spy_CivilianShot", result.Spy_CivilianShot);
        //        command.Parameters.AddWithValue("@Spy_MissionsWin", result.Spy_MissionsWin);
        //        command.Parameters.AddWithValue("@Spy_TimeOut", result.Spy_TimeOut);
        //        command.Parameters.AddWithValue("@Sniper_SpyShot", result.Sniper_SpyShot);
        //        command.Parameters.AddWithValue("@Sniper_CivilianShot", result.Sniper_CivilianShot);
        //        command.Parameters.AddWithValue("@Sniper_MissionsWin", result.Sniper_MissionsWin);
        //        command.Parameters.AddWithValue("@Sniper_TimeOut", result.Sniper_TimeOut);
        //        command.Parameters.AddWithValue("@Points_Won", result.Points_Won);
        //        command.Parameters.AddWithValue("@Points_Lost", result.Points_Lost);
        //        command.Parameters.AddWithValue("@playerID", player.PlayerID);
        //        command.Parameters.AddWithValue("@seasonID", player.Season);

        //        int rowsAffected = command.ExecuteNonQuery();
        //        System.Diagnostics.Debug.WriteLine($"Updated {rowsAffected} rows for Player: {player.Username} in Season: {player.Season}");
        //    }
        //    catch (Exception ex) {
        //        System.Diagnostics.Debug.WriteLine($"Exception in UpdatePlayerStatsByMatch: {ex.Message}");
        //        throw;
        //    }
        //}


        private Results ExecuteStatsQuery(string query, string playerUsername, SqlConnection connection, SqlTransaction transaction) {
            Results result = new Results();

            using SqlCommand command = new SqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@playerUsername", playerUsername);

            using SqlDataReader reader = command.ExecuteReader();
            if (reader.Read()) {
                result.Spy_SpyShot = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                result.Spy_CivilianShot = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                result.Spy_MissionsWin = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                result.Spy_TimeOut = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
            }

            return result;
        }


        private Results GetSpyStats(Player player, SqlConnection connection, SqlTransaction transaction) {
            string query = @"
            SELECT 
                SUM(CASE WHEN result = 'Spy Shot' THEN 1 ELSE 0 END) AS spy_spyShotCount,
                SUM(CASE WHEN result = 'Civilian Shot' THEN 1 ELSE 0 END) AS spy_civilianShotCount,
                SUM(CASE WHEN result = 'Missions Win' THEN 1 ELSE 0 END) AS spy_missionsWinCount,
                SUM(CASE WHEN result = 'Time Out' THEN 1 ELSE 0 END) AS spy_timeOutCount
            FROM Replay
            WHERE spy_username = @playerUsername;";

            return ExecuteStatsQuery(query, player.Username, connection, transaction);
        }


        private Results GetSniperStats(Player player, SqlConnection connection, SqlTransaction transaction) {
            string query = @"
        SELECT 
            SUM(CASE WHEN result = 'Spy Shot' THEN 1 ELSE 0 END) AS sniper_spyShotCount,
            SUM(CASE WHEN result = 'Civilian Shot' THEN 1 ELSE 0 END) AS sniper_civilianShotCount,
            SUM(CASE WHEN result = 'Missions Win' THEN 1 ELSE 0 END) AS sniper_missionsWinCount,
            SUM(CASE WHEN result = 'Time Out' THEN 1 ELSE 0 END) AS sniper_timeOutCount
        FROM Replay
        WHERE sniper_username = @playerUsername;";

            return ExecuteStatsQuery(query, player.Username, connection, transaction);
        }


    }
}
