using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace api.Utils
{
    /// <summary>
    /// Utility used for sending emails from the app
    /// </summary>
    public class EmailSender : IEmailSender
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options"></param>
        public EmailSender(IAuthMessageSenderOptions options)
        {
            this.Options = options;
        }

        /// <summary>
        /// Sendgrid Options
        /// </summary>
        /// <value></value>
        public IAuthMessageSenderOptions Options { get; }

        /// <summary>
        /// Send email
        /// </summary>
        /// <param name="email"></param>
        /// <param name="subject"></param>
        /// <param name="htmlMessage"></param>
        /// <returns></returns>
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Execute(Options.SendGridKey, subject, htmlMessage, email);
        }

        public Task Execute(string apiKey, string subject, string message, string email)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("bomo@planthydra.com", "Bogdan from PlantHydra"),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };

            msg.AddTo(new EmailAddress(email));
            msg.SetClickTracking(false, false);

            return client.SendEmailAsync(msg);
        }
    }
}