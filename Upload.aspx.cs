using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using static SML.Util;
using static SML.Replays;
using static SML.XMLUpdater;

namespace SML {
    public partial class Upload : Page {
        protected void Page_Load(object sender, EventArgs e) {

        }

        // =======================================================================================
        // Eventually I realized I wasn't using any of this so commented it out. Clean it up
        // =======================================================================================
        /*
        // =======================================================================================
        // Takes in the pathway to a directory and will create a table for each match folder
        // =======================================================================================
        public void LoadReplays(string path) {
            // Get the directory where your uploaded files are stored
            string folderPath = Server.MapPath("~/App_Data/replays/");
            string fullPath = folderPath + path;

            if (Directory.Exists(fullPath)) {
                string[] entries = Directory.GetFileSystemEntries(fullPath);

                // Iterate through every match within the replays folder
                foreach (string entryPath in entries) {
                    if (File.Exists(entryPath) || Directory.Exists(entryPath)) {
                        string name = Path.GetFileName(entryPath);
                        if (name == "desktop.ini") continue;
                        DateTime createdDate = Directory.Exists(entryPath)
                            ? new DirectoryInfo(entryPath).CreationTime
                            : new FileInfo(entryPath).CreationTime;

                        // Create a match header row which will hold all of the replay files and
                        // expand to show information for each individual game
                        HtmlTableRow matchFolderRow = createFileRow(name, createdDate);
                        matchFolderRow.Attributes["class"] = "match-folder-row";

                        // Put the main folder's row into the table
                        fileTable.Rows.Add(matchFolderRow);
                    }
                }
            }

           CreateReplaysTable(path);
        }

        // =======================================================================================
        // Takes in the pathway of a match 
        // =======================================================================================
        private void CreateReplaysTable(string folderPath) {
                if (Directory.Exists(folderPath)) {
                string[] entries = Directory.GetFileSystemEntries(folderPath);

                foreach (string entryPath in entries) {
                    if (File.Exists(entryPath) || Directory.Exists(entryPath)) {
                        string name = Path.GetFileName(entryPath);
                        string tableName = "  - " + name + "\n" + Replays.ReadFile(entryPath);
                        
                        DateTime createdDate = Directory.Exists(entryPath)
                            ? new DirectoryInfo(entryPath).CreationTime
                            : new FileInfo(entryPath).CreationTime;

                        HtmlTableRow replayRow = createFileRow(tableName, createdDate);
                        replayRow.Attributes["class"] = "replay-row";
                        fileTable.Rows.Add(replayRow);
                        AddReplayToTable(folderPath, name);
                    }
                }
            }
        }


        private void AddReplayToTable(string folderPath, string fileName) {
            folderPath = $"{folderPath}//{fileName}";

            // Check if the file exists before attempting to read it
            if (File.Exists(folderPath)) {
                ReplayData replay = Replays.ReadFile(folderPath);
                string data = Replays.StringFormatJSON(replay);
                fileData.Text = data;
            }
            else {
                fileData.Text = $"{fileName} not found.";
            }

        }


        // =======================================================================================
        // Create the main header row for the entire file which expands to show replays
        // =======================================================================================
        private HtmlTableRow createFileRow(string fileName, DateTime uploadDate) {
            // Create a new table row
            HtmlTableRow row = new HtmlTableRow();

            // Create table cells for file name and upload date
            HtmlTableCell cellFileName = new HtmlTableCell();
            HtmlTableCell cellUploadDate = new HtmlTableCell();

            // Set the text for the table cells
            cellFileName.InnerText = fileName;
            cellUploadDate.InnerText = uploadDate.ToString();

            // Add the cells to the row
            row.Cells.Add(cellFileName);
            row.Cells.Add(cellUploadDate);
            return row;
        }*/


        // =======================================================================================
        // Store the uploaded replays into the server files
        // =======================================================================================
        protected void uploadFileButton_Click(object sender, EventArgs e) {
            System.Diagnostics.Debug.WriteLine("uploadFileButton_Click");

            string output = "";
            bool successful = true;
            string file = "";

            if (fileUploadData.HasFile) {
                string zipFileName = fileUploadData.PostedFile.FileName; // Get the full path of the uploaded file
                string zipSanitizedFileName = string.Join("_", Path.GetFileName(zipFileName).Split(Path.GetInvalidFileNameChars()));
                string folderPath = Server.MapPath("~/App_Data/replays");
                string zipSavePath = Path.Combine(folderPath, zipSanitizedFileName);
                string extractedFileName = Path.GetFileNameWithoutExtension(zipSavePath);
                file = folderPath + "\\" + extractedFileName;
  

                if (Directory.Exists(zipSavePath)) {
                    output += $"{zipSavePath} already exists <br>";
                } else if (Directory.Exists(extractedFileName)) {
                    output += $"{extractedFileName} already exists <br>";
                } else if (Directory.Exists(file)) {
                    output += $"{file} already exists <br>";
                }
                else {
                    output += $"<br/>ZipSavePath: {zipSavePath}<br/>extractedFileName: {extractedFileName}<br/>file: {file}<br/>";
                    try {
                        output += $"Creating copy of zip <b>{zipSavePath}</b> <br>";
                        fileUploadData.SaveAs(zipSavePath);

                        output += $"Extracting <b>{zipFileName}</b><br>";
                        ZipFile.ExtractToDirectory(zipSavePath, folderPath);

                        output += $"Extraction completed successfully. <br>";

                    }
                    catch (Exception ex) {
                        output += $"Extraction failure. <br>" + ex.Message;
                        successful = false;
                    }
                    finally {
                        output += $"Deleting <b>{zipSavePath}</b> <br>";
                        File.Delete(zipSavePath);
                    }
                }
            }
            else {
                successful = false;
                output += "No file was uploaded.<br>";
            }

            fileUploadNameLabel.Attributes["style"] = "font-size: 12px";
            fileUploadNameLabel.Text = output;

            if (successful) BuildMatchReplayTable(file);
            XMLUpdater stats = new XMLUpdater();
            stats.CollectMatchData(file);
        }


        public void BuildMatchReplayTable(string filePath) {

            HtmlTable replayFolder = new HtmlTable();
            replayFolder.Attributes["class"] = "match-table";
            HtmlTableRow replayFolderHeader = new HtmlTableRow();
            HtmlTableCell playersCell = new HtmlTableCell();
            replayFolder.Controls.Add(replayFolderHeader);
            string currFile = "";


            try {
                string[] files = Directory.GetFiles(filePath);
                string folderPath = Server.MapPath("~/App_Data/replays/");
                string replayFilePath = Path.GetFullPath(files[0]);

                ReplayData replay = Replays.ReadFile(replayFilePath);

                string spy = replay.spy_displayname.Replace("/steam", "");
                string sniper = replay.sniper_displayname.Replace("/steam", "");
                playersCell.InnerText = $"{spy} vs {sniper}";
                playersCell.ColSpan = 2;
                replayFolderHeader.Cells.Add(playersCell);

                // Spy logo column
                HtmlTableCell spyCell = Util.cellImage(@"\Images\icons\spy.png", className: "stat-column", colSpan: 2);
                replayFolderHeader.Cells.Add(spyCell);

                // Sniper logo column
                HtmlTableCell sniperCell = Util.cellImage(@"\Images\icons\sniper.png", className: "stat-column", colSpan: 2);
                replayFolderHeader.Cells.Add(sniperCell);

                // Results column
                HtmlTableCell resultCell = new HtmlTableCell();
                CheckBox showResultCheckbox = new CheckBox();
                showResultCheckbox.Attributes["style"] = "height: 40px;";
                resultCell.Controls.Add(showResultCheckbox);
                Label resultText = new Label();
                resultText.Text = " Show Result ";
                resultText.Attributes["style"] = "font-size: 20px;";
                resultCell.Attributes["class"] = "stat-column";
                resultCell.Controls.Add(resultText);
                replayFolderHeader.Cells.Add(resultCell);

                foreach (string file in files) {

                    currFile = file;
                    replay = Replays.ReadFile(Path.GetFullPath(file));
                    HtmlTableRow replayRow = new HtmlTableRow();
                    replayRow.Attributes.Add("class", "tooltip"); // Add the functionality to show/hide results column
                    replayRow.Attributes["class"] = "replay-row";
                    spy = replay.spy_displayname.Replace("/steam", "");
                    sniper = replay.sniper_displayname.Replace("/steam", "");

                    // Venue image cell and venue name + game mode cell
                    string venue = replay.level;
                    HtmlTableCell venueImageCell = Util.cellImage($@"\Images\venues\{venue}.png", width: 200);
                    HtmlTableCell venueTextCell = Util.cellText($"{venue} {replay.game_type}");
                    venueTextCell.Attributes["style"] = "text-align: left;";

                    // =======================================================================================
                    // Use the Spy's profile picture, if it does not exist default to Smallman.png
                    // =======================================================================================
                    HtmlTableCell spyRoleImageCell = new HtmlTableCell();
                    HtmlImage spyRoleImage = new HtmlImage();
                    string playerIcon = $@"\Images\playerIcons\{spy}.png";
                    spyRoleImage.Src = playerIcon;

                    // If there is a player profile picture available then use it, otherwise default to Smallman.png image
                    if (!File.Exists($"C:\\Users\\Max\\source\\repos\\SML\\Images\\playerIcons\\{spy}.png")) {
                        spyRoleImage.Src = @"\Images\icons\Smallman.png";
                    }
                    spyRoleImage.Width = 50;
                    spyRoleImageCell.Attributes["class"] = "center-column";
                    spyRoleImageCell.Controls.Add(spyRoleImage);
                    replayRow.Cells.Add(spyRoleImageCell);  // Add the logo cell to the row

                    HtmlTableCell spyRoleTextCell = new HtmlTableCell();
                    spyRoleTextCell.InnerText = spy;

                    // =======================================================================================
                    // Use the Sniper's profile picture, if it does not exist default to Smallman.png
                    // =======================================================================================
                    HtmlTableCell sniperRoleImageCell = new HtmlTableCell();
                    HtmlImage sniperRoleImage = new HtmlImage();
                    playerIcon = $@"\Images\playerIcons\{sniper}.png";
                    sniperRoleImage.Src = playerIcon;

                    // If there is a player profile picture available then use it, otherwise default to Smallman.png image
                    if (!File.Exists($"C:\\Users\\Max\\source\\repos\\SML\\Images\\playerIcons\\{sniper}.png")) {
                        sniperRoleImage.Src = @"\Images\icons\Smallman.png";
                    }
                    sniperRoleImage.Width = 50;
                    sniperRoleImageCell.Attributes["class"] = "center-column";
                    sniperRoleImageCell.Controls.Add(sniperRoleImage);
                    replayRow.Cells.Add(sniperRoleImageCell);  // Add the logo cell to the row

                    HtmlTableCell sniperRoleTextCell = new HtmlTableCell();
                    sniperRoleTextCell.InnerText = sniper;

                    // =======================================================================================
                    // Result of the game. Toggle visibility if checkmarked to show/hide
                    // =======================================================================================
                    //HtmlTableCell resultTextCell = new HtmlTableCell();
                    //resultTextCell.InnerText = replay.result;

                    HtmlTableCell resultTextCell = new HtmlTableCell();
                    resultTextCell.InnerText = replay.result;

                    // Add the cells to the row
                    replayRow.Cells.Add(venueImageCell);
                    replayRow.Cells.Add(venueTextCell);
                    replayRow.Cells.Add(spyRoleImageCell);
                    replayRow.Cells.Add(spyRoleTextCell);
                    replayRow.Cells.Add(sniperRoleImageCell);
                    replayRow.Cells.Add(sniperRoleTextCell);
                    replayRow.Controls.Add(resultTextCell);

                    replayFolder.Controls.Add(replayRow);
                }

            } catch (Exception ex) {
                Label1.Text = $"{filePath} \n {currFile} \n unable to get replay data \n {ex.Message}";
            }

            //Label1.Text = readFile(currFile);
            masterTablePlaceholder.Controls.Add(replayFolder);
            
        }

    }

}
