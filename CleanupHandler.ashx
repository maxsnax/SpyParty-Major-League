using System;
using System.Web;

namespace SML
{
    public class CleanupHandler : IHttpHandler, IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            if (context.Session["dataLayer"] is UploadService dataLayer)
            {
                dataLayer.ClearReplaysDirectory(); // Deletes stored replays
            }
        }

        public bool IsReusable => false;
    }
}
