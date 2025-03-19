using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SML.Pages {
    public partial class EventsEdit : System.Web.UI.Page {
        private readonly EventsService _eventsService = new EventsService();
        private string _eventName;

        protected void Page_Load(object sender, EventArgs e) {
            if (Master is SiteMaster master) {
                master.EnableDynamicBackground = true;
            }

            LoadEventsData();
        }

        private void LoadEventsData() {
            string eventName = Request.QueryString["season"];


            if (string.IsNullOrEmpty(eventName)) {
                System.Diagnostics.Debug.WriteLine($"{eventName} event not found.");
                EventNameLabel.Text = $"Event not found.";
            }
            else if (_eventsService.CheckEventName(eventName) == false) {
                System.Diagnostics.Debug.WriteLine($"{eventName} event not found.");
                EventNameLabel.Text = $"{eventName} not found.";
            }
            else {
                _eventName = eventName;
                string authorizedEvent = HttpContext.Current.Session["AuthorizedEvent"] as string ?? string.Empty;

                if (authorizedEvent == null || authorizedEvent != eventName) {
                    System.Diagnostics.Debug.WriteLine($"{eventName} not currently authorized for this user.");
                    EventPasswordContainer.Visible = true;
                    passwordLabel.Text = $"Enter {eventName} Password:";
                    return;
                }
                System.Diagnostics.Debug.WriteLine($"{eventName} authorized.");

                EventPasswordContainer.Visible = false;
                EventNameLabel.Text = $"{eventName}";
                ViewState["eventData"] = null;

                PopulateAuthenticatedUI();
            }
        }

        protected void Submit_EventPassword(object sender, EventArgs e) {

            string eventPassword = passwordTextbox.Text;

            if (string.IsNullOrEmpty(eventPassword)) {
                eventErrorLabel.Text = "Event password cannot be empty.";
                return;
            }
            else if (eventPassword.Length > 50) {
                eventErrorLabel.Text = "Event password cannot exceed 50 characters length";
                return;
            }


            HttpContext.Current.Session["AuthorizedEvent"] = _eventName;
            Response.Redirect("/Pages/EventsEdit.aspx?season=" + HttpUtility.UrlEncode(_eventName));

        }


        //  ================================================================================
        //  Buttons for when the user is authenticated to edit Divisions, Players, Settings
        //  ================================================================================
        public void PopulateAuthenticatedUI() {
            Button EditDivisionButton = new Button {
                ID = "EditDivisionButton",
                Text = "Divisions",
                CssClass = "edit-button"
            };

            EditDivisionButton.Click += new EventHandler(Submit_EditDivision);

            Button EditPlayersButton = new Button {
                ID = "EditPlayersButton",
                Text = "Players",
                CssClass = "edit-button"
            };

            EditPlayersButton.Click += new EventHandler(Submit_EditPlayers);

            Button EditSettingsButton = new Button {
                ID = "EditSettingsButton",
                Text = "Settings",
                CssClass = "edit-button"
            };

            EditSettingsButton.Click += new EventHandler(Submit_EditSettings);

            Button QuitButton = new Button {
                ID = "QuitButton",
                Text = "Quit",
                CssClass = "edit-button"
            };

            QuitButton.Click += new EventHandler(Submit_Quit);

            AuthButtons.Controls.Add(EditDivisionButton);
            AuthButtons.Controls.Add(EditPlayersButton);
            AuthButtons.Controls.Add(EditSettingsButton);
            AuthButtons.Controls.Add(QuitButton);
        }

        public void Submit_EditDivision(object sender, EventArgs e) {
            System.Diagnostics.Debug.WriteLine("Edit Division Click");
        }

        public void Submit_EditPlayers(object sender, EventArgs e) {
            System.Diagnostics.Debug.WriteLine("Edit Players Click");
        }

        public void Submit_EditSettings(object sender, EventArgs e) {
            System.Diagnostics.Debug.WriteLine("Edit Settings Click");
        }

        // Navigates back to normal view events page
        public void Submit_Quit(object sender, EventArgs e) {
            System.Diagnostics.Debug.WriteLine("Quit Click");
        }
    }
}
