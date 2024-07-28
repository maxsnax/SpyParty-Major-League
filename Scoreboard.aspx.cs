using Newtonsoft.Json.Linq;
using Python.Runtime;
using System;
using System.IO;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using static SML.XMLUpdater;
using static SML.XmlUpdater;


namespace SML {
    public partial class Scoreboard : System.Web.UI.Page {

        private string xmlFilePath = "C:\\Users\\Max\\source\\repos\\SML\\App_Data\\Seasons.xml";

        protected void Page_Load(object sender, EventArgs e) {

            XmlDocument doc = new XmlDocument();
            doc.Load(xmlFilePath);

            HtmlTable masterTable = new HtmlTable();
            masterTable.Attributes["class"] = "master-table";

            // Get the list of nodes
            XmlNodeList seasonNodes = doc.SelectNodes("//Season");

            // Add each season to the dropdown menu and select a default
            foreach (XmlNode seasonNode in seasonNodes) {

                string seasonName = seasonNode.SelectSingleNode("Name").InnerText;
                seasonSelectList.Items.Add(new ListItem(seasonName));

            }

            string defaultSeason = "Test";
            XmlNode defaultSeasonNode = doc.SelectSingleNode($"//Season[Name='{defaultSeason}']");
            CreateSeasonTable(defaultSeasonNode);

            seasonSelectList.SelectedValue = defaultSeason;

            masterTablePanel.Controls.Add(masterTable);

        }


        private void CreateSeasonTable(XmlNode seasonNode) {
            string seasonName = seasonNode.SelectSingleNode("Name").InnerText;
            // Create new tables for each season
            HtmlTable seasonTable = new HtmlTable();
            seasonTable.Attributes["class"] = "season-table"; // Add a CSS class to the season table
            HtmlTableRow headerRow = Util.row(seasonName, "season-header");
            seasonTable.Rows.Add(headerRow);    // Add the header row to the season table
            masterTablePanel.Controls.Add(seasonTable);  // Add to the master panel

            // Select the rank nodes to create the tables for it
            XmlNodeList rankNodes = seasonNode.SelectNodes("Rank");

            // Add each rank to the season table
            foreach (XmlNode rankNode in rankNodes) {
                string rankName = rankNode.SelectSingleNode("Name").InnerText;

                // Create a table for each rank
                HtmlTable rankTable = new HtmlTable();
                rankTable.Attributes["class"] = $"rank-table {rankName}";

                // Create the table header
                HtmlTableRow rankHeaderRow = new HtmlTableRow();
                rankHeaderRow.Attributes["class"] = $"rank-header-row {rankName}";

                // =========================================
                //  Logo, Rank, Glicko, Win, Tie, Loss
                // =========================================
                HtmlTableCell logoCell = new HtmlTableCell();
                HtmlImage rankLogo = new HtmlImage();
                rankLogo.Src = $@"\Images\divisions\SML_Badge_{rankName}.png";
                rankLogo.Alt = "Logo";
                rankLogo.Width = 50;
                logoCell.Controls.Add(rankLogo);
                rankHeaderRow.Cells.Add(logoCell);

                // Displays the name of the rank of the player
                HtmlTableCell rankCell = new HtmlTableCell();
                rankCell.InnerText = rankName;
                rankCell.Width = "60%";
                rankHeaderRow.Cells.Add(rankCell);

                // Displays the Spy logo used for spy win % column
                string spyImagePath = @"\Images\icons\spy.png";
                HtmlTableCell spyCell = Util.cellImage(spyImagePath, className: "stat-column");
                rankHeaderRow.Cells.Add(spyCell);

                // Displays the Sniper logo used for sniper win % column
                string sniperImagePath = @"\Images\icons\sniper.png";
                HtmlTableCell sniperCell = Util.cellImage(sniperImagePath, className: "stat-column");
                rankHeaderRow.Cells.Add(sniperCell);

                // GLICKO column name label
                HtmlTableCell eloCell = Util.cellText("Rating", className: "stat-column");
                rankHeaderRow.Cells.Add(eloCell);

                // Scoreline column name label
                HtmlTableCell winCell = new HtmlTableCell();
                winCell.InnerText = "W-T-L";
                winCell.Attributes["class"] = "stat-column";
                rankHeaderRow.Cells.Add(winCell);

                rankTable.Rows.Add(rankHeaderRow);              // Add row to the rank table
                CreateRankTable(rankTable, rankNode, rankName); // Populate the players information

                // Create a div for each table
                HtmlGenericControl tableDiv = new HtmlGenericControl("div");
                tableDiv.Attributes["class"] = $"table-container {rankName}";
                tableDiv.Controls.Add(rankTable);
                
                masterTablePanel.Controls.Add(tableDiv);        // Add the div to the Panel
   
            }

        }


        private void CreateRankTable(HtmlTable rankTable, XmlNode rankNode, string rankName) {
            XmlNodeList playerNodes = rankNode.SelectNodes("Player");

            // Add each rank to the table
            foreach (XmlNode playerNode in playerNodes) {
                string playerName = playerNode.SelectSingleNode("DisplayName").InnerText;

                string playerDataFilePath = "C:\\Users\\Max\\source\\repos\\SML\\App_Data\\PlayerData.xml";
                XmlDocument playerData = new XmlDocument();
                playerData.Load(playerDataFilePath);
                string playerPath = ($"/PlayerData/Players/Name[PlayerName='{playerName}']/Season[Name='Test']");
                XmlNode playerDataNode = playerData.SelectSingleNode(playerPath);

                if (playerDataNode == null) {
                    XmlUpdater updater = new XmlUpdater(playerData);
                    updater.AddPlayerNode(playerName);
                    playerDataNode = playerData.SelectSingleNode(playerPath);
                }

                int spyWinsCount = int.Parse(playerDataNode.SelectSingleNode("Spy/Wins").InnerText);
                int spyGamesCount = int.Parse(playerDataNode.SelectSingleNode("Spy/TotalGames").InnerText);
                string spyStats = "0";
                // To avoid division by zero error
                if (spyGamesCount != 0) {
                    float spyStatsPercentage = (float)spyWinsCount / spyGamesCount * 100;
                    int spyStatsRounded = (int)Math.Round(spyStatsPercentage);
                    spyStats = $"{spyStatsRounded}%";
                }

                int sniperWinsCount = int.Parse(playerDataNode.SelectSingleNode("Sniper/Wins").InnerText);
                int sniperGamesCount = int.Parse(playerDataNode.SelectSingleNode("Sniper/TotalGames").InnerText);
                string sniperStats = "0%";
                // To avoid division by zero error
                if (sniperGamesCount != 0) {
                    float sniperStatsPercentage = (float)sniperWinsCount / sniperGamesCount * 100;
                    int sniperStatsRounded = (int)Math.Round(sniperStatsPercentage);
                    sniperStats = $"{sniperStatsRounded}%";
                }

                string wins = playerDataNode.SelectSingleNode("MatchWins").InnerText;
                string ties = playerDataNode.SelectSingleNode("MatchTies").InnerText;
                string losses = playerDataNode.SelectSingleNode("MatchLosses").InnerText;

                AddPlayerToTable(rankTable, rankName, playerName, spyStats, sniperStats, "Glicko", wins, ties, losses);

            }
        }


        private void AddPlayerToTable(HtmlTable rankTable, string rankName, string player, string spyStats, string sniperStats, string elo, string win, string tie, string loss) {
            HtmlTableRow dataRow = new HtmlTableRow();
            dataRow.Attributes["class"] = "player-row";

            // =========================================
            //  Logo, Rank, Elo, Win, Tie, Loss
            // =========================================
            HtmlTableCell logoCell = new HtmlTableCell();
            HtmlImage rankLogo = new HtmlImage();

            string playerIcon = @"\Images\playerIcons\" + player + ".png";
            rankLogo.Src = playerIcon;
            if (!File.Exists($"C:\\Users\\Max\\source\\repos\\SML\\Images\\playerIcons\\" + player + ".png")) {
                rankLogo.Src = @"\Images\icons\Smallman.png";
            }
            rankLogo.Width = 50;
            logoCell.Attributes.Add("style", "border: 1px solid;");
            logoCell.BgColor = "transparent";
            logoCell.Controls.Add(rankLogo);
            dataRow.Cells.Add(logoCell);  // Add the logo cell to the row

            HtmlTableCell playerCell = new HtmlTableCell();
            playerCell.InnerText = player;
            playerCell.Attributes["class"] = "name-cell";
            dataRow.Cells.Add(playerCell);

            HtmlTableCell spyCell = new HtmlTableCell();
            spyCell.InnerText = spyStats;
            spyCell.Attributes["class"] = "stat-column";
            dataRow.Cells.Add(spyCell);

            HtmlTableCell sniperCell = new HtmlTableCell();
            sniperCell.InnerText = sniperStats;
            sniperCell.Attributes["class"] = "stat-column";
            dataRow.Cells.Add(sniperCell);

            HtmlTableCell eloCell = new HtmlTableCell();
            eloCell.InnerText = elo;
            eloCell.Attributes["class"] = "stat-column";
            dataRow.Cells.Add(eloCell);

            HtmlTableCell winCell = new HtmlTableCell();
            winCell.InnerText = $"{win}-{tie}-{loss}";
            winCell.Attributes["class"] = "stat-column";
            dataRow.Cells.Add(winCell);


            // Add the player to the rank table
            rankTable.Rows.Add(dataRow);
        }

    }
}
