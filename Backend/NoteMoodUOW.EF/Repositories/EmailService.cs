using MimeKit;
using MailKit.Net.Smtp;
using NoteMoodUOW.Core.Interfaces;
using NoteMoodUOW.Core.Models;
using NoteMoodUOW.Core.Configurations;

namespace NoteMoodUOW.EF.Repositories
{
    public class EmailService : IEmailService
    {
        private readonly EmailConfiguration _emailConfiguration;

        public EmailService(EmailConfiguration emailConfiguration)
        {
            _emailConfiguration = emailConfiguration;
        }

        /// <summary>
        /// Sends an email using the provided message.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        public void SendEmail(Message message)
        {
            var emailMessage = CreateEmailMessage(message);
            Send(emailMessage);
        }

        /// <summary>
        /// Creates a MimeMessage object based on the provided message.
        /// </summary>
        /// <param name="message">The message to create the email from.</param>
        /// <returns>The created MimeMessage object.</returns>
        /// Mime is a standard that defines the structure of an email message.
        private MimeMessage CreateEmailMessage(Message message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("email", _emailConfiguration.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text)
            {
                Text = message.Content
            };
            return emailMessage;
        }

        /// <summary>
        /// Sends the provided MimeMessage using the configured SMTP server.
        /// </summary>
        /// <param name="mailMessage">The MimeMessage to be sent.</param>
        private void Send(MimeMessage mailMessage)
        {
            using var client = new SmtpClient();
            try
            {
                client.Connect(_emailConfiguration.SmtpServer, _emailConfiguration.Port, true);
                // Disable OAuth 2.0 authentication mechanism.
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(_emailConfiguration.Username, _emailConfiguration.Password);
                client.Send(mailMessage);
            }
            catch
            {
                throw;
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
            }
        }
    }
}
