using SML.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace SML.Pages {
    public partial class Events : System.Web.UI.Page {
        private readonly EventsService _eventsService = new EventsService();

        protected void Page_Load(object sender, EventArgs e) {

            LoadEventsData();

        }

        private void LoadEventsData() {
            string eventName = Request.QueryString["season"];

            if (string.IsNullOrEmpty(eventName)) {
                System.Diagnostics.Debug.WriteLine("No specific event requested. Loading all events...");

                DataTable rawData;
                rawData = _eventsService.PopulateAllEventData(EventGridView);

                // DEBUG: Print all column names
                foreach (DataColumn col in rawData.Columns) {
                    System.Diagnostics.Debug.WriteLine("Column: " + col.ColumnName);
                }

                if (rawData.Rows.Count > 0) {
                    EventGridView.DataSource = rawData;
                    EventGridView.DataBind();
                }
                else {
                    throw new Exception("Error: No data loaded for EventGridView.");
                }

                EventsGridSection.Visible = true;
                EventsPlayerSection.Visible = false;

                ViewState["dataTable"] = rawData;
            }
            else {
                DataTable eventData = _eventsService.GetEventData(EventPlayerView, eventName);
                LobbyLabel.Text = eventData != null ? $"{eventName}" : "Not found.";

                EventsGridSection.Visible = false;
                EventsPlayerSection.Visible = true;

                EventEditLink.NavigateUrl = "/Pages/EventsEdit.aspx?season=" + HttpUtility.UrlEncode(eventName);

                ViewState["eventData"] = eventData;
            }
        }

        public void EventGridView_Sorting(object sender, GridViewSortEventArgs e) {
            System.Diagnostics.Debug.WriteLine("Sort GridView");

            if (ViewState["dataTable"] is DataTable dataTable) {
                DataView dataView = new DataView(dataTable);

                string sortDirection = GetSortDirection(e.SortExpression);
                dataView.Sort = e.SortExpression + " " + sortDirection;

                EventGridView.DataSource = dataView;
                EventGridView.DataBind();

                // Apply underline to the sorted column
                int columnIndex = GetColumnIndexBySortExpression(e.SortExpression);
                if (EventGridView.HeaderRow != null && columnIndex >= 0) {
                    foreach (TableCell cell in EventGridView.HeaderRow.Cells) {
                        cell.Style["border-bottom"] = "1px solid black"; // Remove underline from all headers
                    }
                    EventGridView.HeaderRow.Cells[columnIndex].Style["border-bottom"] = "3px solid black"; // Underline sorted column
                }
            }
        }


        public void EventPlayerView_Sorting(object sender, GridViewSortEventArgs e) {
            System.Diagnostics.Debug.WriteLine("Sort PlayerView");

            if (ViewState["eventData"] is DataTable dataTable) {
                DataView dataView = new DataView(dataTable);

                string sortDirection = GetSortDirection(e.SortExpression);
                dataView.Sort = e.SortExpression + " " + sortDirection;

                EventPlayerView.DataSource = dataView;
                EventPlayerView.DataBind();

                // Apply underline to the sorted column
                int columnIndex = GetColumnIndexBySortExpression(e.SortExpression);
                if (EventPlayerView.HeaderRow != null && columnIndex >= 0) {
                    foreach (TableCell cell in EventPlayerView.HeaderRow.Cells) {
                        cell.Style["border-bottom"] = "1px solid black"; // Remove underline from all headers
                    }
                    EventPlayerView.HeaderRow.Cells[columnIndex].Style["border-bottom"] = "3px solid black"; // Underline sorted column
                }
            }
        }

        protected void EventGridView_RowDataBound(object sender, GridViewRowEventArgs e) {
            if (e.Row.RowType == DataControlRowType.DataRow) {
                string seasonName = DataBinder.Eval(e.Row.DataItem, "season_name").ToString();
                string encodedSeasonName = HttpUtility.UrlEncode(seasonName);

                // Update the row's onclick to use the encoded season name
                e.Row.Attributes["onclick"] = "location.href='/Pages/Events.aspx?season=" + HttpUtility.UrlEncode(seasonName) + "';";
                e.Row.Style["cursor"] = "pointer";
            }
        }

        protected void EventPlayerView_RowDataBound(object sender, GridViewRowEventArgs e) {
            if (e.Row.RowType == DataControlRowType.DataRow) {
                string playerName = DataBinder.Eval(e.Row.DataItem, "player_name").ToString();
                string encodedSeasonName = HttpUtility.UrlEncode(playerName);

                // Update the row's onclick to use the encoded season name
                e.Row.Attributes["onclick"] = "window.location='" + Page.ResolveUrl("~/Players/" + playerName) + "';";
                e.Row.Style["cursor"] = "pointer";
            }
        }



        // Helper function to get column index
        private int GetColumnIndexBySortExpression(string sortExpression) {
            for (int i = 0; i < EventGridView.Columns.Count; i++) {
                if (EventGridView.Columns[i] is BoundField field && field.SortExpression == sortExpression) {
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

        //private void PopulateAllPlayerData() {

        //}

        //private void PopulatePlayerData(List<Player> playerList) {
        //    if (playerList == null || playerList.Count == 0) return;

        //    Player player = playerList[0];

        //    // Define image path
        //    string playerIcon = $"/Images/playerIcons/{player.Name}.png";
        //    string fullPath = Path.Combine(@"C:\Users\Max\source\repos\SML\Images\playerIcons", $"{player.Name}.png");

        //    // Check if file exists, else use default
        //    if (!File.Exists(fullPath)) {
        //        playerIcon = "/Images/icons/Smallman.png";
        //    }

        //    // Update Image control
        //    //playerProfilePhoto.ImageUrl = playerIcon;

        //    HtmlImage playerImage = new HtmlImage();
        //    playerImage.Src = playerIcon;
        //    //playerPhotoCell.Controls.Add(playerImage);
        //}

        //private List<Player> GetPlayerData(string playerName) {
        //    DatabaseHelper db = new DatabaseHelper();

        //    List<Player> playerData = null;

        //    //selectSeasonList.Items.Clear();
        //    List<Tuple<int, string>> seasonList = new List<Tuple<int, string>>();

        //    using (SqlConnection connection = DatabaseHelper.GetConnection()) {
        //        connection.Open();

        //        try {
        //            playerData = db.GetPlayerByName(playerName, connection, null);
        //            if (playerData == null) return null;

        //            foreach (Player p in playerData) {
        //                int seasonID = p.Season;
        //                string seasonName = db.GetSeasonNameById(seasonID, connection, null);
        //                System.Diagnostics.Debug.WriteLine($"SeasonID:{seasonID}:{seasonName}");
        //                seasonList.Add(Tuple.Create(seasonID, seasonName));
        //            }
        //        }
        //        catch (Exception ex) {
        //            System.Diagnostics.Debug.WriteLine(ex.Message);
        //        }
        //    }

        //    //PopulateSeasons(seasonList);

        //    return playerData;
        //}

    }
}