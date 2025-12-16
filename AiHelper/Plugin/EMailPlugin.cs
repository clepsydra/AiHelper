using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiHelper.Config;
using AiHelper.Config.Models;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using Microsoft.SemanticKernel;
using MimeKit;
using NAudio.Midi;

namespace AiHelper.Plugin
{
    internal class EMailPlugin
    {
        private EMailConfig EMailConfig => ConfigProvider.Config.EMailConfig;

        [KernelFunction]
        [Description(@"Send an text based email.
Before sending the mail make sure the user does not want to change something.
So before sending the mail tell the user what the mail will contain and who the recipient is.
And ask for confirmation whether the user wants to send this mail.
Parameters:
- Recipient: email Adresse des Empfängers
- Subject: The subject of the email
- Body: The text content of the email.
Return value:
- A message whether the mail has been sent successfully, else the information about the error")]

        public async Task<string> SendEmail(string recipient, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(EMailConfig.Name, EMailConfig.EMailAddress));            
            message.To.Add(new MailboxAddress(recipient, recipient));
            message.Subject = subject;

            message.Body = new TextPart("plain")
            {
                Text = body
            };

            try
            {
                using (var client = new SmtpClient())
                {
                    client.Connect(EMailConfig.SmtpServer, 465, true);
                    client.Authenticate(EMailConfig.EMailAddress, EMailConfig.ApiKey);

                    string result = client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                return "An error occurred: " + ex.ToString();
            }

            return "successfully sent";
        }

        [KernelFunction]
        [Description(@"Gets the email address from the address book. Parameters:
- Name: The short name of the recipient
Return value: The email address if it is available, else nothing.")]
        public string GetEmailAdressFromName(string name)
        {
            var addressBook = ConfigProvider.Config?.EMailConfig?.AddressBook;
            if (addressBook == null)
            {
                return string.Empty;
            }

            var matchingEntry = addressBook.FirstOrDefault(kvp => kvp.Key.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (matchingEntry.Value == null)
            {
                return string.Empty;
            }

            return matchingEntry.Value;
        }

        [KernelFunction]
        [Description(@"Gets emails from the server. It returns only the names of the senders.
Intention is that the user can then ask for specific mails by asking for mails of this sender.")]
        public async Task<List<string>> GetEmailsWithSenders()
        {           
            try
            {
                var inbox = GetInbox();

                HashSet<string> names = new HashSet<string>();

                for (var i = 1; i < 10; i++)
                {
                    var message = inbox.GetMessage(inbox.Count - i);
                    string from = message.From.FirstOrDefault()?.Name;
                    if (string.IsNullOrEmpty(from))
                    {
                        from = (message.From.FirstOrDefault() as MailboxAddress).ToString();
                    }

                    Console.WriteLine("From: {0}", from);
                    names.Add(from);
                }

                //client.Disconnect(true);

                return names.ToList();
            }
            catch (Exception ex)
            {
                return new List<string> { "An error occurred: " + ex.Message };
            }
        }

        [KernelFunction]
        [Description(@"Gets last mail for a specific sender from the server.
Parameters:
- sender
Returns: The date, the subject and the content of the mails of this sender")]
        public async Task<Dictionary<string, string>> GetEmailsForSender(string sender)
        {
            var inbox = GetInbox();

            for (var i = 1; i < 10; i++)
            {
                var message = inbox.GetMessage(inbox.Count - i);
                string from = message.From.FirstOrDefault()?.ToString();
                if (string.IsNullOrEmpty(from))
                {
                    continue;
                }

                if (!from.ToUpperInvariant().Contains(sender.ToUpperInvariant()))
                {
                    continue;
                }

                return new Dictionary<string, string>
                {
                    { "DateTime", message.Date.ToString("o")},
                    { "Subject", message.Subject },
                    { "Body" , message.TextBody??message.HtmlBody }
                };
            }

            return null;
        }

        private ImapClient client;
        private IMailFolder inbox;

        private IMailFolder GetInbox()
        {
            if (inbox != null)
            {
                return inbox;
            }

            if (client == null)
            {
                client = new ImapClient();
            }

            client.Connect(EMailConfig.ImapServer, 993, true);

            client.Authenticate(EMailConfig.EMailAddress, EMailConfig.ApiKey);

            inbox = client.Inbox;
            inbox.Open(FolderAccess.ReadOnly);

            Console.WriteLine("Total messages: {0}", inbox.Count);
            Console.WriteLine("Recent messages: {0}", inbox.Recent);

            return inbox;
        }
    }
}
