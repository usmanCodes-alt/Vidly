using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vidly.Service
{
    public class EmailSender : IEmailSender
    {
        public EmailOptions EmailOptions { get; set; }
        public EmailSender(IOptions<EmailOptions> options)
        {
            EmailOptions = options.Value;
        }
        public Task SendEmailAsync(string email, string subject, string message)
        {
            return Execute(EmailOptions.SendGridKey, subject, message, email);
        }

        private Task Execute(string sendGridKey, string subject, string message, string email)
        {
            SendGridClient client = new SendGridClient(sendGridKey);
            SendGridMessage sendGridMessage = new SendGridMessage
            {
                From = new EmailAddress("moazali626@gmail.com", "Vidly"),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            sendGridMessage.AddTo(new EmailAddress(email));
            try
            {
                return client.SendEmailAsync(sendGridMessage);
            }
            catch(Exception)
            {

            }
            return null;
        }
    }
}
