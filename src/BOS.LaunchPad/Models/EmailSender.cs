using Microsoft.AspNetCore.Identity.UI.Services;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace BOS.LaunchPad.Models
{
    public class EmailSender : IEmailSender
    {
        private string _fromEmail;
        private string _apiKey;

        public EmailSender(string fromEmail, string apiKey)
        {
            _fromEmail = fromEmail;
            _apiKey = apiKey;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SendGridClient(_apiKey);
            var msg = MailHelper.CreateSingleEmail(new EmailAddress(_fromEmail), new EmailAddress(email), subject, "", htmlMessage);
            var response = await client.SendEmailAsync(msg);
        }
    }
}
