using Microsoft.EntityFrameworkCore.Query.Internal;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Models
{
    // Represents an email message.
    public class Message
    {
        // The message contains the recipient, subject, and content.
        public List<MailboxAddress> To { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public Message(IEnumerable<string> to, string subject, string content)
        {
            To = new List<MailboxAddress>();
            // Converts each recipient email address (string) in the provided collection "to" 
            // into a MailboxAddress object and adds it to the "To" list.
            To.AddRange(to.Select(x => new MailboxAddress("email",x)));
            Subject = subject;
            Content = content;
        }
    }
}
