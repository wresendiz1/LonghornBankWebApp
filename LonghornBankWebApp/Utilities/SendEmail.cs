using System;
using System.Net.Mail;
using System.Net;


namespace LonghornBankWebApp.Utilities
{
    public static class EmailMessaging
    {
        public static void SendEmail(String toEmailAddress, String emailSubject, String emailBody)
        {

            String strFromEmailAddress = "longhornbanktrust@gmail.com";

            try
            {
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com", 587);
                SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
                MailMessage email = new MailMessage();
                // START
                email.From = new MailAddress(strFromEmailAddress);
                email.To.Add(toEmailAddress);
                //email.CC.Add(strFromEmailAddress);
                email.Subject = "Team 33 " + emailSubject;
                email.Body = emailBody;

                //END
                SmtpServer.Timeout = 5000;
                SmtpServer.EnableSsl = true;
                SmtpServer.UseDefaultCredentials = false;
                SmtpServer.Credentials = new NetworkCredential(strFromEmailAddress, "hlfdwpqzvnnsfezj");
                SmtpServer.Send(email);
            }
            catch (Exception ex)
            {
                throw new Exception("Error sending email: " + ex.Message);
            }

        }
    }
}
