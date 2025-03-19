using System.Web.Routing;
using Microsoft.AspNet.FriendlyUrls;

namespace SML {
    public static class RouteConfig {
        public static void RegisterRoutes(RouteCollection routes) {
            var settings = new FriendlyUrlSettings();
            settings.AutoRedirectMode = RedirectMode.Permanent;

            // Enable FriendlyUrls without overriding existing routes
            routes.EnableFriendlyUrls(settings);

            // Debug output to confirm FriendlyUrls was added
            System.Diagnostics.Debug.WriteLine("FriendlyUrls enabled. Now registering custom routes...");

            routes.Ignore("{resource}.axd/{*pathInfo}");


            // Route "Default" to "Pages/Default.aspx"
            routes.MapPageRoute("ContactRoute", "Contact", "~/Pages/Contact.aspx");

            // Route "Default" to "Pages/Default.aspx"
            routes.MapPageRoute("HomeRoute", "", "~/Pages/Default.aspx");

            // Route "Players" to "Pages/Players.aspx"
            routes.MapPageRoute("PlayersRoute", "players", "~/Pages/Players.aspx");

            // Route for a player by name
            routes.MapPageRoute("PlayersByNameRoute", "players/{playerName}", "~/Pages/Players.aspx");

            // Route "Events" to "Pages/Events.aspx"
            routes.MapPageRoute("EventsRoute", "events", "~/Pages/Events.aspx");

            // Route for an event by name
            routes.MapPageRoute("EventsByNameRoute", "events/{eventName}", "~/Pages/Events.aspx");

            // Route "EventsEdit" to "Pages/EventsEdit.aspx"
            routes.MapPageRoute("EventsEditRoute", "eventsedit", "~/Pages/EventsEdit.aspx");

            // Route for an event by name
            routes.MapPageRoute("EventsEditByNameRoute", "eventsedit/{eventName}", "~/Pages/EventsEdit.aspx");

            // Route "Scoreboard" to "Pages/Scoreboard.aspx"
            routes.MapPageRoute("ScoreboardRoute", "Scoreboard", "~/Pages/Scoreboard.aspx");

            // Route "Upload" to "Pages/Scoreboard.aspx"
            routes.MapPageRoute("UploadRoute", "Upload", "~/Pages/Upload.aspx");




            // Debug registered routes
            foreach (var routeItem in routes) {
                System.Diagnostics.Debug.WriteLine($"Registered Route: {routeItem}");
            }
        }
    }
}
