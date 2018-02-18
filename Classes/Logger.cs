using System;
using System.IO;
using System.Web;

namespace MSAWeb.Classes
{
    public static class Logger
    {
        public const string Success = "Success";
        public const string Error = "Error";
        private const string path = "~/App_Data/Log.txt";

        public static void Log(string type, string tag, string message)
        {
            var file = HttpContext.Current.Server.MapPath(path);
            var writer = new StreamWriter(file, true);
            writer.WriteLine("**** Log Begin at " + DateTime.Now + " ****");
            writer.WriteLine("Log Type: " + type);
            writer.WriteLine("Log Tag: " + tag);
            writer.WriteLine("Message: " + message);
            writer.WriteLine("**** Log End ****");
            writer.WriteLine(Environment.NewLine);
            writer.Close();
        }
    }
}