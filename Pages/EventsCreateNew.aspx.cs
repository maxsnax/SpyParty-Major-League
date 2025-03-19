using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SML.Pages {
    public partial class EventsCreateNew : System.Web.UI.Page {
        private readonly EventsService _eventsService = new EventsService();

        // This variable will hold the JSON string representing the image URLs.
        protected string ImageListJson;

        protected void Page_Load(object sender, EventArgs e) {
            // Ensure the master page is correctly cast before accessing EnableDynamicBackground
            if (Master is SiteMaster master) {
                master.EnableDynamicBackground = true; // Enable background effect for this page
            }
        }

        protected void Submit_EventName(object sender, EventArgs e) {
            string eventName = nameTextbox.Text;

            if (string.IsNullOrEmpty(eventName)) {
                eventErrorLabel.Text = "Event name cannot be empty.";
                return;
            } else if (eventName.Length > 50) {
                eventErrorLabel.Text = "Event name cannot exceed 50 characters length";
                return;
            }

            bool taken = _eventsService.CheckEventName(eventName);

            if (taken) {
                eventErrorLabel.Text = $"{eventName} is already taken!";
                return;
            }

            eventErrorLabel.Text = "";
            nameLabel.Text = $"{eventName} Password:";
            eventNamePanel.Visible = false;
            eventPasswordPanel.Visible = true;
        }

        protected void Submit_EventPassword(object sender, EventArgs e) {
            string eventName = nameTextbox.Text;
            string eventPassword = passwordTextbox.Text;

            if (string.IsNullOrEmpty(eventPassword)) {
                eventErrorLabel.Text = "Event password cannot be empty.";
                return;
            } else if (eventPassword.Length > 50) {
                eventErrorLabel.Text = "Event password cannot exceed 50 characters length";
                return;
            }

            _eventsService.CreateNewEvent(eventName, eventPassword);
            HttpContext.Current.Session["AuthorizedEvent"] = eventName;
            Response.Redirect("/Pages/EventsEdit.aspx?season=" + HttpUtility.UrlEncode(eventName));

        }

    }
}
