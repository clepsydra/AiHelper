using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiHelper.Config.Models
{
    public class EMailConfig
    {
        public string SmtpServer { get; set; } = string.Empty;

        public string ImapServer { get; set; } = string.Empty;

        public string EMailAddress { get; set; } = string.Empty;

        public string ApiKey { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public Dictionary<string, string> AddressBook { get; set; } = new ();

        internal EMailConfig Clone()
        {
            return new EMailConfig
            {
                SmtpServer = SmtpServer,
                ImapServer = ImapServer,
                EMailAddress = EMailAddress,
                ApiKey = ApiKey,
                Name = Name,
                AddressBook = AddressBook.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };
        }
    }
}
