using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;

namespace SML {
    public class Util {

        public static HtmlTableRow row(string text, string className = "defaultClassName", int height = -1) {

            HtmlTableRow row = new HtmlTableRow();
            row.Cells.Add(cellText(text, className));

            if (height > -1) {
                row.Height = height.ToString();
            }

            return row;
        }


        public static HtmlTableCell cellText(string text, string className = "defaultClassName", int colSpan = 1) {

            HtmlTableCell cell = new HtmlTableCell();
            cell.InnerText = text;
            cell.Attributes["class"] = className;
            cell.ColSpan = colSpan;

            return cell;
        }

        public static HtmlTableCell cellImage(string imagePath, string className = "defaultClassName", int colSpan = 1, int width = -1) {

            HtmlTableCell cell = new HtmlTableCell();
            HtmlImage image = new HtmlImage();
            image.Src = imagePath;
            image.Alt = "image";
            cell.ColSpan = colSpan;
            cell.Controls.Add(image);
            cell.Attributes.Add("class", className);

            if (width > 0) {
                image.Width = width;
            }

            return cell;
        }

        public static string scrubName(string name) {
            name = name.Replace("/steam", "");
            return name;
        }
    }
}
