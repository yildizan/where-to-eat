using Microsoft.ApplicationInsights;
using System.Web.Mvc;

namespace MSAWeb.Classes
{
    public class ErrorHandler : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            if (!filterContext.ExceptionHandled)
            {
                var telemetry = new TelemetryClient();
                telemetry.TrackException(filterContext.Exception);
                Logger.Log(Logger.Error, filterContext.Controller.ToString(), filterContext.Exception.Message);
            }
            base.OnException(filterContext);
        }
    }
}