using System.Net;
using System.Net.Mail;

namespace DayBuddy.Services
{
    public class GmailSMTPEmailService
    {
        private readonly IConfiguration config;
        private string emailPassword = "";
        private string emailAdress = "";
        public GmailSMTPEmailService(IConfiguration config)
        {
            this.config = config;
            emailAdress = config.GetValue<string>("EmailAdress")!;
            emailPassword = config.GetValue<string>("EmailPassword")!;
        }

        public async Task<bool> TrySendEmailAsync(string targetEmail, string subject, string htmlContent)
        {
            //handle testing enviroment
            if (string.IsNullOrEmpty(emailPassword) || string.IsNullOrEmpty(emailAdress)) return true;

            try
            {
                MailMessage message = new();

                message.From = new MailAddress(emailAdress);
                message.Subject = subject;

                message.To.Add(new MailAddress(targetEmail));
                message.Body = htmlContent;
                message.IsBodyHtml = true;

                SmtpClient smtpClient = new("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(emailAdress, emailPassword),
                    EnableSsl = true
                };

                await smtpClient.SendMailAsync(message);    
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false; 
            }

            return true;
        }
    }
}
