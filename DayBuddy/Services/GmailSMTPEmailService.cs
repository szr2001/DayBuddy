using System.Net;
using System.Net.Mail;

namespace DayBuddy.Services
{
    public class GmailSMTPEmailService
    {
        private readonly string gmailPassword = "lqbkzqsasnzjbaqo";
        private readonly string gmail = "daybuddy2024@gmail.com";
        public async Task<bool> TrySendEmailAsync(string targetEmail, string htmlContent)
        {
            try
            {
                MailMessage message = new();

                message.From = new MailAddress(gmail);
                message.Subject = "Email Send Test";

                message.To.Add(new MailAddress(targetEmail));
                message.Body = htmlContent;
                message.IsBodyHtml = true;

                SmtpClient smtpClient = new("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(gmail, gmailPassword),
                    EnableSsl = true
                };

                await smtpClient.SendMailAsync(message);    
                Console.WriteLine("Sent");
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
