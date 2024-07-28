using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static System.Net.Mime.MediaTypeNames;
using Image = System.Web.UI.WebControls.Image;

namespace SML {
    public partial class _Default : Page {
        protected void Page_Load(object sender, EventArgs e) {
            if (!IsPostBack) {
                // Create the table structure
                Table1.Rows.Clear(); // Clear any existing rows
                Table1.BorderStyle = BorderStyle.Solid;
                Table1.BorderWidth = 1;

                // Create the table header
                TableHeaderRow headerRow = new TableHeaderRow();
                headerRow.Cells.Add(new TableHeaderCell { Text = "Player" });
                headerRow.Cells.Add(new TableHeaderCell { Text = "Score" });
                headerRow.Cells.Add(new TableHeaderCell { Text = "Rank" });
                Table1.Rows.Add(headerRow);

                // Add data rows
                AddDataRow("John", "1000", "1");
                AddDataRow("Jane", "850", "2");
                AddDataRow("Bob", "720", "3");
            }
        }

        private void AddDataRow(string player, string score, string rank) {
            TableRow dataRow = new TableRow();
            dataRow.Cells.Add(new TableCell { Text = player });
            dataRow.Cells.Add(new TableCell { Text = score });
            dataRow.Cells.Add(new TableCell { Text = rank });
            Table1.Rows.Add(dataRow);
        }

    }
}