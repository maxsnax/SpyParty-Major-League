using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SML {
    public partial class SiteMaster : MasterPage {
        public bool EnableDynamicBackground { get; set; } = false;
        protected string ImageListJson;

        protected void Page_Load(object sender, EventArgs e) {
            if (EnableDynamicBackground) {
                string folderPath = Server.MapPath("~/Images/Venues");
                if (Directory.Exists(folderPath)) {
                    var files = Directory.GetFiles(folderPath);
                    var imageUrls = files.Select(file => ResolveUrl("~/Images/Venues/" + Path.GetFileName(file))).ToList();
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    ImageListJson = serializer.Serialize(imageUrls);
                }
                else {
                    ImageListJson = "[]";
                }
            }
        }
    }

}