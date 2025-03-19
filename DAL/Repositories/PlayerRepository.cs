using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.WebPages;
using Microsoft.Graph.Models.Security;
using SML.Models;
using Match = SML.Models.Match;

namespace SML.DAL.Repositories {
    public class PlayerRepository : BaseRepository {


        private readonly SqlConnection _connection;
        private SqlTransaction _transaction;

        public PlayerRepository(SqlConnection connection) {
            _connection = connection;
        }

        public void SetTransaction(SqlTransaction transaction) {
            _transaction = transaction;
        }


        public DataTable GetPlayerData() {
            string query = @"
            SELECT 
                p.player_name, 
                d.division_name, 
                s.season_name, 
                p.win, p.tie, p.loss
            FROM Player p
            LEFT JOIN Division d ON p.division_id = d.division_id
            LEFT JOIN Season s ON p.season_id = s.season_id";


            DataTable table = new DataTable();
            using (SqlDataAdapter adapter = new SqlDataAdapter(query, _connection)) {
                adapter.Fill(table);
            }
            return table;
        }

        public List<Player> GetPlayerByName(string playerName) {
            playerName = playerName.Replace("/steam", "");

            System.Diagnostics.Debug.WriteLine($"SQL GetPlayerByName() - playerName: {playerName}");

            string query = "SELECT player_id, player_name, forfeit, division_id, season_id, username FROM Player " +
                "WHERE player_name = @playerName";

            using SqlCommand command = new SqlCommand(query, _connection);

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


        public void UpdatePlayerUsername(Player player) {
            System.Diagnostics.Debug.WriteLine($"UpdatePlayerUsername {player.Name}");

            try {
                string query = @"
                    UPDATE p
                    SET username = r.spy_username
                    FROM Player p
                    JOIN Replay r 
                        ON r.spy_displayname LIKE p.player_name + '%'
                    WHERE (p.username IS NULL OR p.username = '') 
                        AND p.player_name = @playerName;
                ";


                using SqlCommand command = new SqlCommand(query, _connection, _transaction);
                command.Parameters.AddWithValue("@playerName", player.Name);

                int rowsAffected = command.ExecuteNonQuery();
                System.Diagnostics.Debug.WriteLine($"Updated {rowsAffected} player usernames.");
            }
            catch (Exception ex) {
                throw ex;
            }
        }

        public Player GetPlayerByNameAndSeason(string playerName, int seasonId) {
            System.Diagnostics.Debug.WriteLine($"GetPlayerByNameAndSeason {playerName}:{seasonId}");

            try {
                playerName = playerName.Replace("/steam", "");

                string query = "SELECT player_id, player_name, forfeit, division_id, season_id, username FROM Player " +
                    "WHERE player_name = @playerName AND season_id = @seasonID";
                using SqlCommand command = new SqlCommand(query, _connection, _transaction);

                command.Parameters.AddWithValue("@playerName", playerName);
                command.Parameters.AddWithValue("@seasonID", seasonId);

                using SqlDataReader reader = command.ExecuteReader();
                if (reader.Read()) {
                    int player_id = reader.GetInt32(0);
                    string player_name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                    int forfeit = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                    int division_id = reader.IsDBNull(3) ? -1 : reader.GetInt32(3);
                    int season_id = reader.IsDBNull(4) ? -1 : reader.GetInt32(4);
                    string username = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);

                    return new Player {
                        PlayerID = player_id,
                        Name = player_name,
                        Forfeit = forfeit,
                        Division = division_id,
                        Season = season_id,
                        Username = username
                    };
                } else {
                    return null;
                }
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"Failed GetPlayerBySeason");
                throw ex;
            }
        }


        public Results UpdatePlayerStats(Player player) {
            System.Diagnostics.Debug.WriteLine($"UpdatePlayerStats: {player.Name}");
            System.Diagnostics.Debug.WriteLine($"{player}");

            if (player.PlayerID <= 0) {
                System.Diagnostics.Debug.WriteLine($"PlayerID = 0, invalid ID");
                throw new ArgumentNullException(nameof(player), "Null player sent in UpdatePlayerStats");
            } else if (player.Username == null) {
                System.Diagnostics.Debug.WriteLine($"PlayerUsername is null, invalid for replay search");
                throw new ArgumentNullException(nameof(player), "PlayerUsername is null, invalid for replay search");
            }

            try {
                string query = @"
                    WITH ReplayStats AS (
                        SELECT
                            @playerID AS player_id,
                            SUM(CASE WHEN result = 'Spy Shot' AND spy_username = @playerUsername THEN 1 ELSE 0 END) AS spy_spyShotCount,
                            SUM(CASE WHEN result = 'Civilian Shot' AND spy_username = @playerUsername THEN 1 ELSE 0 END) AS spy_civilianShotCount,
                            SUM(CASE WHEN result = 'Missions Win' AND spy_username = @playerUsername THEN 1 ELSE 0 END) AS spy_missionsWinCount,
                            SUM(CASE WHEN result = 'Time Out' AND spy_username = @playerUsername THEN 1 ELSE 0 END) AS spy_timeOutCount,
                            SUM(CASE WHEN result = 'Spy Shot' AND sniper_username = @playerUsername THEN 1 ELSE 0 END) AS sniper_spyShotCount,
                            SUM(CASE WHEN result = 'Civilian Shot' AND sniper_username = @playerUsername THEN 1 ELSE 0 END) AS sniper_civilianShotCount,
                            SUM(CASE WHEN result = 'Missions Win' AND sniper_username = @playerUsername THEN 1 ELSE 0 END) AS sniper_missionsWinCount,
                            SUM(CASE WHEN result = 'Time Out' AND sniper_username = @playerUsername THEN 1 ELSE 0 END) AS sniper_timeOutCount
                        FROM Replay
                    ),
                    MatchStats AS (
                        SELECT
                            @playerID AS player_id,
                            SUM(CASE WHEN winner = @playerID THEN 1 ELSE 0 END) AS wins,
                            SUM(CASE WHEN winner IS NOT NULL AND winner <> @playerID THEN 1 ELSE 0 END) AS losses,
                            SUM(CASE WHEN winner = 0 AND (player_one_id = @playerID OR player_two_id = @playerID) THEN 1 ELSE 0 END) AS ties
                        FROM Match
                        WHERE player_one_id = @playerID OR player_two_id = @playerID
                    )
                    UPDATE Player
                    SET 
                        spy_shot = rs.spy_spyShotCount,
                        spy_civilian_shot = rs.spy_civilianShotCount,
                        spy_mission_win = rs.spy_missionsWinCount,
                        spy_timeout = rs.spy_timeOutCount,
                        sniper_shot = rs.sniper_spyShotCount,
                        sniper_civilian_shot = rs.sniper_civilianShotCount,
                        sniper_mission_win = rs.sniper_missionsWinCount,
                        sniper_timeout = rs.sniper_timeOutCount,
                        win = ms.wins,
                        loss = ms.losses,
                        tie = ms.ties
                    FROM Player ps
                    JOIN ReplayStats rs ON ps.player_id = rs.player_id
                    JOIN MatchStats ms ON ps.player_id = ms.player_id
                    WHERE ps.player_id = @playerID;
                ";

                using SqlCommand command = new SqlCommand(query, _connection, _transaction);
                command.Parameters.AddWithValue("@playerUsername", player.Username);
                command.Parameters.AddWithValue("@playerID", player.PlayerID);

                Results result = new Results();
                using SqlDataReader reader = command.ExecuteReader();

                if (reader.Read()) {
                    result.Spy_SpyShot = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    result.Spy_CivilianShot = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                    result.Spy_MissionsWin = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                    result.Spy_TimeOut = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
                    result.Sniper_SpyShot = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
                    result.Sniper_CivilianShot = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);
                    result.Sniper_MissionsWin = reader.IsDBNull(6) ? 0 : reader.GetInt32(6);
                    result.Sniper_TimeOut = reader.IsDBNull(7) ? 0 : reader.GetInt32(7);
                }

                System.Diagnostics.Debug.WriteLine($"Returning {player.Name} result: {result}");

                return result;
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"Exception in UpdatePlayerStats: {ex.Message}");
                throw;
            }
        }


        // This is all weird. Because the Results of a player doesn't know if the opponent forfeited
        public void UpdatePlayerStatsFromMatch(Match match) {
            Player playerOne = match.PlayerOne;
            Player playerTwo = match.PlayerTwo;
            match.CalculateWinner();
            UpdatePlayerStatsFromResults(playerOne, match.Winner);
            UpdatePlayerStatsFromResults(playerTwo, match.Winner);


        }

        public void UpdatePlayerStatsFromResults(Player player, int winner) {
            System.Diagnostics.Debug.WriteLine($"UpdatePlayerStats: {player.Name}");

            try {
                Results result = player.Results;
                string query = @"
                    UPDATE Player
                    SET 
                        win = win + @Win,
                        loss = loss + @Loss,
                        tie = tie + @Tie,
                        spy_shot = spy_shot + @Spy_SpyShot,
                        spy_civilian_shot = spy_civilian_shot + @Spy_CivilianShot,
                        spy_mission_win = spy_mission_win + @Spy_MissionsWin,
                        spy_timeout = spy_timeout + @Spy_TimeOut,
                        sniper_shot = sniper_shot + @Sniper_SpyShot,
                        sniper_civilian_shot = sniper_civilian_shot + @Sniper_CivilianShot,
                        sniper_mission_win = sniper_mission_win + @Sniper_MissionsWin,
                        sniper_timeout = sniper_timeout + @Sniper_TimeOut 
                    WHERE player_id = @playerID AND season_ID = @seasonID;
                ";

                using SqlCommand command = new SqlCommand(query, _connection, _transaction);

                // Handle forfeit possibilities from match to add win/loss/tie stat
                int win = player.PlayerID == winner ? 1 : 0;
                int loss = winner > 0 && player.PlayerID != winner ? 1 : 0;
                int tie = winner == 0 ? 1 : 0;
                // if winner == -1 then neither win or lose technically? because they both ff

                command.Parameters.AddWithValue("@Win", win);
                command.Parameters.AddWithValue("@Loss", loss);
                command.Parameters.AddWithValue("@Tie", tie);
                command.Parameters.AddWithValue("@Spy_SpyShot", result.Spy_SpyShot);
                command.Parameters.AddWithValue("@Spy_CivilianShot", result.Spy_CivilianShot);
                command.Parameters.AddWithValue("@Spy_MissionsWin", result.Spy_MissionsWin);
                command.Parameters.AddWithValue("@Spy_TimeOut", result.Spy_TimeOut);
                command.Parameters.AddWithValue("@Sniper_SpyShot", result.Sniper_SpyShot);
                command.Parameters.AddWithValue("@Sniper_CivilianShot", result.Sniper_CivilianShot);
                command.Parameters.AddWithValue("@Sniper_MissionsWin", result.Sniper_MissionsWin);
                command.Parameters.AddWithValue("@Sniper_TimeOut", result.Sniper_TimeOut);
                command.Parameters.AddWithValue("@Points_Won", result.Points_Won);
                command.Parameters.AddWithValue("@Points_Lost", result.Points_Lost);
                command.Parameters.AddWithValue("@playerID", player.PlayerID);
                command.Parameters.AddWithValue("@seasonID", player.Season);

                int rowsAffected = command.ExecuteNonQuery();
                System.Diagnostics.Debug.WriteLine($"Updated {rowsAffected} rows for Player: {player.Username} in Season: {player.Season}");
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"Exception in UpdatePlayerStatsByMatch: {ex.Message}");
                throw;
            }
        }

    }
}
