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

            }

            // Ensure the master page is correctly cast before accessing EnableDynamicBackground
            SiteMaster master = Master as SiteMaster;
            if (master != null) {
                master.EnableDynamicBackground = true; // Enable background effect for this page
            }
        }

    }
}
