using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using SML.Exceptions;
using System.IO.Compression;
using SML.Models;
using static SML.Models.Replays;
using SML.DAL.Repositories;
using SML.DAL;
using System.Configuration;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Web.UI.WebControls;
using Microsoft.Data.SqlClient;

namespace SML {
    public class UploadService {

        private string replayDirectoryLocation;
        private List<Match> matchList = new List<Match>();

        public List<Tuple<int, string>> LoadSeasons() {
            using UnitOfWork uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["SML_db-connection"].ToString());
            
            return uow.SeasonsRepo.LoadSeasons();
        }

        // =======================================================================================
        // Upload the file into the server's App_Data/replays directory as a .zip and directory
        // =======================================================================================
        public string MoveReplaysToServer(HttpPostedFile uploadedFile, string serverPath) {
            System.Diagnostics.Debug.WriteLine("moveReplaysToServer()");

            // Get the name of the uploaded file
            string uploadFileName = uploadedFile.FileName;
            FileInfo fileInfo = new FileInfo(uploadFileName);
            string fileExtension = fileInfo.Extension;

            // Reject the uploaded file if it's not a .ZIP format
            if (fileExtension != ".zip") {
                throw new InvalidFileFormatException($"{fileExtension} is not a valid .zip file! Please upload a .zip file");
            }

            // Remove illegal characters)
            string zipSanitizedFileName = string.Join("_", Path.GetFileNameWithoutExtension(uploadFileName).Split(Path.GetInvalidFileNameChars()));
            zipSanitizedFileName = zipSanitizedFileName.Replace("%2fsteam", "");

            string tempDir = Path.Combine(serverPath, "App_Data", "replays");
            string zipFilePath = Path.Combine(tempDir, zipSanitizedFileName + ".zip");
            string extractPath = Path.Combine(tempDir, zipSanitizedFileName);

            // Ensure replays directory exists
            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);

            // Save the uploaded .zip file to the server in the replays directory
            uploadedFile.SaveAs(zipFilePath);
            System.Diagnostics.Debug.WriteLine($"Zip saved at: {zipFilePath}");

            // Ensure directory exists to extract the .zip contents into, in case there are loose .replay files
            // Ensure replays directory exists
            if (!Directory.Exists(extractPath))
                Directory.CreateDirectory(extractPath);

            try {
                ZipFile.ExtractToDirectory(zipFilePath, extractPath);
                System.Diagnostics.Debug.WriteLine($"Files extracted to: {extractPath}");
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            // Get all extracted files, searching nested directories within
            string[] files = Directory.GetFiles(extractPath, "*.*", SearchOption.AllDirectories);

            // Ensure only .replay files are uploaded before we start opening them and reading
            foreach (string file in files) {
                System.Diagnostics.Debug.WriteLine($"Searching for non-replay files. {file}");
                FileInfo replayFileInfo = new FileInfo(file);
                string replayFileExtension = replayFileInfo.Extension;
                if (replayFileExtension != ".replay") {
                    throw new InvalidFileFormatException($"{replayFileExtension} is not a valid .replay file! Please upload a .zip file containing .replay files");
                }

            }

            replayDirectoryLocation = extractPath;
            return extractPath;
        }


        // =======================================================================================
        //  Deletes the pathway and all it's subdirectories
        // =======================================================================================
        public void ClearReplaysDirectory() {
            DeletePath(replayDirectoryLocation + ".zip");
            DeletePath(replayDirectoryLocation);
            matchList = new List<Match>();
        }

        // =======================================================================================
        //  Deletes the pathway and all it's subdirectories
        // =======================================================================================
        private static void DeletePath(string path) {
            System.Diagnostics.Debug.WriteLine($"DeletePath({path})");
            // Delete extracted files before removing the directory
            if (Directory.Exists(path)) {
                foreach (string file in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)) {
                    try {
                        File.Delete(file);
                        System.Diagnostics.Debug.WriteLine($"Deleted file: {file}");
                    }
                    catch (Exception ex) {
                        System.Diagnostics.Debug.WriteLine($"Failed to delete file: {file} - {ex.Message}");
                    }
                }

                // Ensure all subdirectories are deleted
                foreach (string dir in Directory.GetDirectories(path, "*", SearchOption.AllDirectories)) {
                    try {
                        Directory.Delete(dir, true);
                        System.Diagnostics.Debug.WriteLine($"Deleted directory: {dir}");
                    }
                    catch (Exception ex) {
                        System.Diagnostics.Debug.WriteLine($"Failed to delete directory: {dir} - {ex.Message}");
                    }
                }
            }

            if (Directory.Exists(path)) {
                try {
                    Directory.Delete(path, true);
                    System.Diagnostics.Debug.WriteLine($"Deleted dir: {path}");
                }
                catch (Exception ex) {
                    System.Diagnostics.Debug.WriteLine($"Failed to delete directory: {path} - {ex.Message}");
                }
            }
            if (File.Exists(path)) {
                try {
                    File.Delete(path);
                    System.Diagnostics.Debug.WriteLine($"Deleted file: {path}");
                }
                catch (Exception ex) {
                    System.Diagnostics.Debug.WriteLine($"Failed to delete file: {path} - {ex.Message}");
                }
            }
        }


        // =======================================================================================
        //  Process the replays uploaded and then return the match list
        // =======================================================================================
        // Call from business layer to start recursive exploration for replays from the top directory
        public List<Match> ProcessSeasonMatches(int seasonID) {
            ProcessDirectoryMatches(replayDirectoryLocation, seasonID);

            return matchList;
        }


        // =======================================================================================
        //  Directory processing of all uploaded directories/replays
        // =======================================================================================
        // Given the server directory path and the seasonID from the webpage, will return all the match data collected
        private List<Match> ProcessDirectoryMatches(string replayFilesLocation, int seasonID) {
            try {
                string[] directories = Directory.GetDirectories(replayFilesLocation, "*", SearchOption.TopDirectoryOnly);
                string[] files = Directory.GetFiles(replayFilesLocation, "*.replay", SearchOption.TopDirectoryOnly);

                if (files.Length > 0) {
                    // If this directory contains .replay files, process it
                    ProcessMatch(replayFilesLocation, seasonID);
                }

                // Recursively process subdirectories
                foreach (string directory in directories) {
                    ProcessDirectoryMatches(directory, seasonID);
                }

                return matchList;
            }
            catch (Exception ex) {
                throw new Exception($"Error processing directory {replayFilesLocation}: {ex.Message}", ex);
            }
        }

        // =======================================================================================
        //  Directory processing of all uploaded directories/replays
        // =======================================================================================
        private void ProcessMatch(string directoryPath, int seasonID) {
            // Ensure directory exists
            if (!Directory.Exists(directoryPath)) {
                System.Diagnostics.Debug.WriteLine($"Invalid path: {directoryPath}");
                return;
            }

            // Get all replay files in the directory
            string[] files = Directory.GetFiles(directoryPath, "*.replay", SearchOption.TopDirectoryOnly);
            if (files.Length == 0) return; // No replay files found, nothing to process

            Match match = new Match {
                SeasonID = seasonID
            };

            foreach (var file in files) {
                ReplayData replay = Replays.ReadFile(Path.GetFullPath(file));

                // Skip processing this file
                if (replay == null) {
                    System.Diagnostics.Debug.WriteLine($"Warning: Failed to process replay file {file}. It returned null.");
                    continue;
                } else if (replay.result == "In_Progress") {
                    System.Diagnostics.Debug.WriteLine($"Warning: Skipped replay file {file}. It returned In_Progress.");
                    continue;
                }

                ProcessReplay(match, replay);
            }

            // Add the match to the global match list
            matchList.Add(match);
        }

        // =======================================================================================
        //  Match processing of replay data
        // =======================================================================================
        private void ProcessReplay(Match match, ReplayData replay) {
            string spy = replay.spy_displayname.Replace("/steam", "");
            string sniper = replay.sniper_displayname.Replace("/steam", "");

            if (match.PlayerOne == null && match.PlayerTwo == null) {
                match.PlayerOne = new Player(spy);
                match.PlayerTwo = new Player(sniper);
            }
            else {
                bool playerOneFound = match.PlayerOne.Name == spy || match.PlayerOne.Name == sniper;
                bool playerTwoFound = match.PlayerTwo.Name == spy || match.PlayerTwo.Name == sniper;

                if (!playerOneFound || !playerTwoFound) {
                    string replayPlayers = $"{spy} vs {sniper}";
                    string matchPlayers = $"{match.PlayerOne.Name} vs {match.PlayerTwo.Name}";
                    throw new InvalidMatchException($"{replayPlayers} invalid replay for match {matchPlayers}");
                }
            }

            match.ProcessReplay(replay);
        }


        // =======================================================================================
        // Azure SQL
        // =======================================================================================
        // =======================================================================================
        // Process upload confirmation of business layer
        // =======================================================================================
        public async Task ConfirmUpload() {
            System.Diagnostics.Debug.WriteLine($"ConfirmUpload()");

            using UnitOfWork uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["SML_db-connection"].ToString());

            try {
                uow.BeginTransaction();

                // Upload all the matches to SQL first in case we run into any issues before uploading Blobs
                foreach (Match match in matchList) {
                    SaveMatchToSQL(match, uow);
                }

                // Less likely to have issues, and lost replays is less important than saving the replay data to SQL
                foreach (Match match in matchList) {
                    await UploadReplayBlobs(match);
                }

                // Once all the SQL entries are made and the Blobs uploaded then we commit
                uow.CommitTransaction();
                System.Diagnostics.Debug.WriteLine($"Matches successfully uploaded to SQL and Azure Blob Storage");

            }
            catch {
                // If there was an issue with either SQL or the Blob upload then we roll it back
                uow.Rollback();
                throw;
            }
            finally {
                ClearReplaysDirectory();
                matchList = new List<Match>();
            }
        }


        // =======================================================================================
        // Save the match's ReplayData to SQL
        // =======================================================================================
        private void SaveMatchToSQL(Match match, UnitOfWork uow) {
            System.Diagnostics.Debug.WriteLine($"SaveMatchToSQL()");

            // Handle if null player data was passed
            if (match == null || match.PlayerOne == null || match.PlayerTwo == null || match.SeasonID <= 0) {
                throw new Exception("Unable to use player data to save match. Invalid player data or season provided.");
            }

            match.CalculateWinner();
            Player playerOne = match.PlayerOne;
            Player playerTwo = match.PlayerTwo;
            int season_id = match.SeasonID;


            try {
                Player playerOneSQL = uow.PlayersRepo.GetPlayerByNameAndSeason(playerOne.Name, season_id);
                Player playerTwoSQL = uow.PlayersRepo.GetPlayerByNameAndSeason(playerTwo.Name, season_id);

                // Handle if either of the players were unable to be found in the current season
                if (playerOneSQL == null || playerTwoSQL == null) {
                    throw new NullReferenceException($"Unable to find players in current season database.");
                }

                // Update our current player info with their player_id, username, division_id, forfeit status
                playerOne.UpdatePlayerInfo(playerOneSQL);
                playerTwo.UpdatePlayerInfo(playerTwoSQL);

                // Handle if the players are both in the season, but are in different divisions
                if (playerOne.Division != playerTwo.Division) {
                    throw new Exception($"{playerOne.Name}:{playerOne.Division} and {playerTwo.Name}:{playerTwo.Division} are not in the same division!");
                }

                // Upload the match replays to SQL
                uow.MatchesRepo.CreateMatchWithReplays(match);

                // Set the username values if they were null, since this would be this player's first games
                // We're gonna need the in-game username to query replays later on
                if (playerOne.Username == null) {
                    uow.PlayersRepo.UpdatePlayerUsername(playerOne);
                }
                if (playerTwo.Username == null) {
                    uow.PlayersRepo.UpdatePlayerUsername(playerTwo);
                }

                // Update the player's stats for win/tie/loss in the division and their spy/sniper
                uow.PlayersRepo.UpdatePlayerStatsFromMatch(match);

                //updateLabel(fileUploadNameLabel, Color.Green, "Upload Successful!");
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"Error Caught: Rolling back transaction");
                //updateLabel(fileUploadNameLabel, Color.Red, $"Error: {ex.Message}");
                throw ex;
            }
        }


        // =======================================================================================
        //  Azure Blobs
        // =======================================================================================
        //  Iterates through a filePath, uploading all of the files to the /replays/ blob container
        // =======================================================================================
        private async Task UploadReplayBlobs(Match match) {
            try {
                System.Diagnostics.Debug.WriteLine($"Uploading match {match.PlayerOne.Name} vs {match.PlayerTwo.Name}");
                foreach (ReplayData replay in match.Replays) {
                    try {
                        var service = new AzureBlobService();
                        await service.UploadFilesAsync(replay.file_path, replay.uuid);
                        System.Diagnostics.Debug.WriteLine($"Uploaded blob {replay.file_path}:{replay.uuid} to server.");
                    }
                    catch (Exception ex) {
                        System.Diagnostics.Debug.WriteLine($"{replay.file_path}:{replay.uuid} \n unable to upload replays to cloud service \n {ex.Message}");
                        //updateLabel(fileUploadNameLabel, Color.Red, $"{replay.file_path}:{replay.uuid} \n unable to upload replays to cloud service \n {ex.Message}");
                    }
                }
            }
            catch (Exception ex) {
                throw ex;
                //fileUploadNameLabel.Text = $"{match.PlayerOne.Name} vs {match.PlayerTwo.Name} \n unable to get replays \n {ex.Message}";
            }

        }

    }
}
