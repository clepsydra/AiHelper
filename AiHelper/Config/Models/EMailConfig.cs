using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiHelper.Config.Models
{
    public class EMailConfig
    {
        public string SmtpServer { get; set; }

        public string ImapServer { get; set; }

        public string EMailAddress { get; set; }

        public string ApiKey { get; set; }

        public string Name { get; set; }

        public Dictionary<string, string>? AddressBook { get; set; }

        internal EMailConfig Clone()
        {
            return new EMailConfig
            {
                SmtpServer = SmtpServer,
                ImapServer = ImapServer,
                EMailAddress = EMailAddress,
                ApiKey = ApiKey,
                Name = Name,
                AddressBook = AddressBook?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };
        }
    }
}
