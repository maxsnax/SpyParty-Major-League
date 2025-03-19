using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using SML.Models;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Windows.Forms;
using System.Data.Common;

namespace SML {
    public partial class Players : System.Web.UI.Page {
        private readonly PlayersService _playersService = new PlayersService();

        protected void Page_Load(object sender, EventArgs e) {
            if (!IsPostBack) {
                LoadPlayersData();
            }
        }

        private void LoadPlayersData() {
            string playerName = Page.RouteData.Values["playerName"] as string ?? Request.QueryString["playerName"];

            if (string.IsNullOrEmpty(playerName)) {
                System.Diagnostics.Debug.WriteLine("No specific player requested. Loading all players...");

                DataTable rawData;
                rawData = _playersService.PopulateAllPlayerData(PlayerGridView);
                ViewState["dataTable"] = rawData;
            }
            else {
                List<Player> playerData = _playersService.GetPlayerData(playerName);
                //lblPlayerName.Text = playerData != null ? $"Profile of {playerName}" : "Player not found.";
            }
        }

        public void PlayerGridView_Sorting(object sender, GridViewSortEventArgs e) {
            System.Diagnostics.Debug.WriteLine("Sort GridView");

            if (ViewState["dataTable"] is DataTable dataTable) {
                DataView dataView = new DataView(dataTable);

                string sortDirection = GetSortDirection(e.SortExpression);
                dataView.Sort = e.SortExpression + " " + sortDirection;

                PlayerGridView.DataSource = dataView;
                PlayerGridView.DataBind();

                // Apply underline to the sorted column
                int columnIndex = GetColumnIndexBySortExpression(e.SortExpression);
                if (PlayerGridView.HeaderRow != null && columnIndex >= 0) {
                    foreach (TableCell cell in PlayerGridView.HeaderRow.Cells) {
                        cell.Style["border-bottom"] = "1px solid black"; // Remove underline from all headers
                    }
                    PlayerGridView.HeaderRow.Cells[columnIndex].Style["border-bottom"]= "3px solid black"; // Underline sorted column
                }
            }
        }

        protected void PlayerGridView_RowDataBound(object sender, GridViewRowEventArgs e) {
            if (e.Row.RowType == DataControlRowType.DataRow) {
                string playerName = DataBinder.Eval(e.Row.DataItem, "player_name").ToString();
                e.Row.Attributes["onclick"] = "window.location='Players/" + playerName + "';";
                e.Row.Style["cursor"] = "pointer";  // Optional: changes the cursor to a pointer when hovering
            }
        }


        // Helper function to get column index
        private int GetColumnIndexBySortExpression(string sortExpression) {
            for (int i = 0; i < PlayerGridView.Columns.Count; i++) {
                if (PlayerGridView.Columns[i] is BoundField field && field.SortExpression == sortExpression) {
                    return i;
                }
            }
            return -1;
        }

        private string GetSortDirection(string column) {
            string sortDirection = "ASC";

            if (ViewState["SortColumn"] as string == column) {
                if (ViewState["SortDirection"] as string == "ASC") {
                    sortDirection = "DESC";
                }
            }

            ViewState["SortColumn"] = column;
            ViewState["SortDirection"] = sortDirection;

            return sortDirection;
        }


        //private void PopulateSeasons(List<Tuple<int, string>> seasonList) {
        //    // Add option to filter by "All"
        //    selectSeasonList.Items.Add(new ListItem("All", "0"));

        //    foreach (var season in seasonList) {
        //        int seasonID = season.Item1;
        //        string seasonName = season.Item2;
        //        selectSeasonList.Items.Add(new ListItem(seasonName, seasonID.ToString()));
        //    }
        //}

        private void PopulateAllPlayerData() {

        }

        private void PopulatePlayerData(List<Player> playerList) {
            if (playerList == null || playerList.Count == 0) return;

            Player player = playerList[0];

            // Define image path
            string playerIcon = $"/Images/playerIcons/{player.Name}.png";
            string fullPath = Path.Combine(@"C:\Users\Max\source\repos\SML\Images\playerIcons", $"{player.Name}.png");

            // Check if file exists, else use default
            if (!File.Exists(fullPath)) {
                playerIcon = "/Images/icons/Smallman.png";
            }

            // Update Image control
            //playerProfilePhoto.ImageUrl = playerIcon;

            HtmlImage playerImage = new HtmlImage();
            playerImage.Src = playerIcon;
            //playerPhotoCell.Controls.Add(playerImage);
        }

        private List<Player> GetPlayerData(string playerName) {
            DatabaseHelper db = new DatabaseHelper ();

            List<Player> playerData = null;

            //selectSeasonList.Items.Clear();
            List<Tuple<int, string>> seasonList = new List<Tuple<int, string>>();

            using (SqlConnection connection = DatabaseHelper.GetConnection()) {
                connection.Open();

                try {
                    playerData = db.GetPlayerByName(playerName, connection, null);
                    if (playerData == null) return null;

                    foreach (Player p in playerData) {
                        int seasonID = p.Season;
                        string seasonName = db.GetSeasonNameById(seasonID, connection, null);
                        System.Diagnostics.Debug.WriteLine($"SeasonID:{seasonID}:{seasonName}");
                        seasonList.Add(Tuple.Create(seasonID, seasonName));
                    }
                }
                catch (Exception ex) {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }

            //PopulateSeasons(seasonList);

            return playerData;
        }

    }
}
