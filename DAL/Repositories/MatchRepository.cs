using Newtonsoft.Json;
using SML.Exceptions;
using SML.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static SML.Models.Replays;

namespace SML.DAL.Repositories {
    public class MatchRepository : BaseRepository {

        private readonly SqlConnection _connection;
        private SqlTransaction _transaction;

        public MatchRepository(SqlConnection connection) {
            _connection = connection;
        }

        public void SetTransaction(SqlTransaction transaction) {
            _transaction = transaction;
        }


        // Upload a single replay into the matchID provided
        public void UploadGame(ReplayData replay, int matchID) {
            System.Diagnostics.Debug.WriteLine($"SQL UploadGame:{matchID}-{replay.spy_displayname} vs {replay.sniper_displayname}");

            try {

                string query = @"
                    INSERT INTO Replay (spy_username, sniper_username, result, level, selected_missions, picked_missions, completed_missions, sequence_number, start_time, duration, game_type, uuid, map_variant, spy_displayname, sniper_displayname, guest_count, clock_seconds, match_id)
                    VALUES (@spy_username, @sniper_username, @result, @level, @selected_missions, @picked_missions, @completed_missions, @sequence_number, @start_time, @duration, @game_type, @uuid, @map_variant, @spy_displayname, @sniper_displayname, @guest_count, @clock_seconds, @match_id)";
                using SqlCommand command = new SqlCommand(query, _connection, _transaction);

                command.Parameters.AddWithValue("@spy_username", (string)replay.spy_username);
                command.Parameters.AddWithValue("@sniper_username", (string)replay.sniper_username);
                command.Parameters.AddWithValue("@result", (string)replay.result);
                command.Parameters.AddWithValue("@level", (string)replay.level);
                command.Parameters.AddWithValue("@selected_missions", JsonConvert.SerializeObject(replay.selected_missions));
                command.Parameters.AddWithValue("@picked_missions", JsonConvert.SerializeObject(replay.picked_missions));
                command.Parameters.AddWithValue("@completed_missions", JsonConvert.SerializeObject(replay.completed_missions));
                command.Parameters.AddWithValue("@sequence_number", (int)replay.sequence_number);
                command.Parameters.AddWithValue("@start_time", (DateTime)replay.start_time);
                command.Parameters.AddWithValue("@duration", (int)replay.duration);
                command.Parameters.AddWithValue("@game_type", (string)replay.game_type);
                command.Parameters.AddWithValue("@uuid", (string)replay.uuid);
                command.Parameters.AddWithValue("@map_variant", (object)replay.map_variant ?? DBNull.Value);
                command.Parameters.AddWithValue("@spy_displayname", (string)replay.spy_displayname);
                command.Parameters.AddWithValue("@sniper_displayname", (string)replay.sniper_displayname);
                command.Parameters.AddWithValue("@guest_count", (int)replay.guest_count);
                command.Parameters.AddWithValue("@clock_seconds", (int)replay.clock_seconds);
                command.Parameters.AddWithValue("@match_id", matchID);

                command.ExecuteNonQuery();
            }
            // Unique Constraint Violation (Duplicate UUID)
            catch (SqlException ex) when (ex.Number == 2627) {
                string errorMessage = $"Upload failed: A replay with the same UUID ({replay.uuid}) already exists.";
                System.Diagnostics.Debug.WriteLine(errorMessage);

                // Throw a custom exception to be handled by the calling function
                throw new InvalidOperationException(errorMessage);
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }


        // Create a match between two players and upload all of the replays with the Match's Replays object
        //public int CreateMatchWithReplays(Match match) {
        //    using SqlConnection connection = GetConnection();
        //    connection.Open();
        //    using SqlTransaction transaction = connection.BeginTransaction();
        //    try {
        //        int matchId;

        //        using SqlCommand command = new SqlCommand(@"
        //                    INSERT INTO Matches (player_one_id, player_two_id, player_one_name, player_two_name, division_id) 
        //                    VALUES (@playerOneId, @playerTwoId, @playerOneName, @playerTwoName, @divisionId);
        //                    SELECT SCOPE_IDENTITY();", connection, transaction);
        //        command.Parameters.AddWithValue("@playerOneId", match.PlayerOne.PlayerID);
        //        command.Parameters.AddWithValue("@playerTwoId", match.PlayerTwo.PlayerID);
        //        command.Parameters.AddWithValue("@playerOneName", match.PlayerOne.Name);
        //        command.Parameters.AddWithValue("@playerTwoName", match.PlayerTwo.Name);
        //        command.Parameters.AddWithValue("@divisionId", match.DivisionID);

        //        System.Diagnostics.Debug.WriteLine("Submitted query for new match.");
        //        matchId = Convert.ToInt32(command.ExecuteScalar());
        //        System.Diagnostics.Debug.WriteLine($"MatchID:{matchId}");

        //        match.ID = matchId;

        //        foreach (var replay in match.Replays) {
        //            UploadGame(replay, match.ID, connection, transaction);
        //        }

        //        transaction.Commit();
        //        return matchId;
        //    }
        //    catch (Exception ex) {
        //        transaction.Rollback();
        //        throw new Exception("Error creating match and uploading replays", ex);
        //    }
        //}

        public void CreateMatchWithReplays(Match match) {
            System.Diagnostics.Debug.WriteLine($"SQL CreateMatchWithReplays: {match.PlayerOne.Name} vs {match.PlayerTwo.Name}");

            try {
                Player playerOne = match.PlayerOne;
                Player playerTwo = match.PlayerTwo;

                int divisionID = playerOne.Division == playerTwo.Division ? match.PlayerTwo.Division : -1;
                if (divisionID == -1) throw new InvalidMatchException("Players are in two different divisions.");

                // Going to call this just in case, although it should have already been calculated
                match.CalculateWinner();
                int? winner = match.Winner; // Nullable int

                System.Diagnostics.Debug.WriteLine($"Creating match for {playerOne.Name} vs {playerTwo.Name}");
                System.Diagnostics.Debug.WriteLine($"{playerOne.Name}\n:ID={playerOne.PlayerID}:Div={playerOne.Division}:Forfeit={playerOne.Forfeit}:Username{playerOne.Username}\n");
                System.Diagnostics.Debug.WriteLine($"{playerTwo.Name}\n:ID={playerTwo.PlayerID}:Div={playerTwo.Division}:Forfeit={playerTwo.Forfeit}:Username{playerTwo.Username}\n");

                string query = "INSERT INTO Match (division_id, player_one_id, player_one_score, player_one_name, player_two_id, player_two_score, player_two_name, winner, forfeit) " +
                    "OUTPUT INSERTED.match_id VALUES (@division_id, @player_one_id, @player_one_score, @player_one_name, @player_two_id, @player_two_score, @player_two_name, @winner, @forfeit)";

                using SqlCommand command = new SqlCommand(query, _connection, _transaction);

                command.Parameters.AddWithValue("@division_id", divisionID);
                command.Parameters.AddWithValue("@player_one_id", playerOne.PlayerID);
                command.Parameters.AddWithValue("@player_one_score", playerOne.Results.Points_Won);
                command.Parameters.AddWithValue("@player_one_name", playerOne.Name);
                command.Parameters.AddWithValue("@player_two_id", playerTwo.PlayerID);
                command.Parameters.AddWithValue("@player_two_score", playerTwo.Results.Points_Won);
                command.Parameters.AddWithValue("@player_two_name", playerTwo.Name);
                command.Parameters.AddWithValue("@winner", winner ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@forfeit", match.Forfeit);

                match.ID = (int)command.ExecuteScalar();

                foreach (var replay in match.Replays) {
                    UploadGame(replay, match.ID);
                }

            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }


        public List<int> GetMatchWinners(Player playerOne, Player playerTwo) {
            System.Diagnostics.Debug.WriteLine($"GetMatchWinners {playerOne.Name} vs {playerTwo.Name}");

            List<int> winners = new List<int>();

            try {
                string query = "SELECT winner FROM Match " +
                    "WHERE ((player_one_id = @playerOne AND player_two_id = @playerTwo) " +
                    "OR (player_one_id = @playerTwo AND player_two_id = @playerOne)) " +
                    "AND division_id = @divisionId ";

                using SqlCommand command = new SqlCommand(query, _connection, _transaction);

                command.Parameters.AddWithValue("@playerOne", playerOne.PlayerID);
                command.Parameters.AddWithValue("@playerTwo", playerTwo.PlayerID);
                command.Parameters.AddWithValue("@divisionId", playerOne.Division);

                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read()) {
                    if (reader["winner"] != DBNull.Value) {
                        winners.Add(Convert.ToInt32(reader["winner"]));
                    }
                }
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
            }

            return winners;
        }
    }
}
