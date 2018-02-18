using System;
using System.Net;
using System.Net.Mail;

namespace MSAWeb.Classes
{
    public static class MailSender
    {
        private const string from = "restaurant.suggestion@gmail.com"; 
        private const string pass = "********";
        private const string subject = "Restaurant Suggestion";

        public static void SendMail(string to, string body)
        {
            try
            {
                SmtpClient smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(from, pass),
                    Timeout = 3000
                };

                body = body + Environment.NewLine + Environment.NewLine +
                    "Nerede Yesek? Applicaton - http://msaweb.azurewebsites.net";

                MailMessage message = new MailMessage(from, to, subject, body);

                smtp.Send(message);
            }

            catch(Exception ex)
            {
                Logger.Log(Logger.Error, "MailSender", ex.Message);
            }
        }
    }
}
