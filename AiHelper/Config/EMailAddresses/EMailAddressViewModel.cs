using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AiHelper.Config.EMailAddresses
{
    public class EMailAddressViewModel : ViewModelBase
    {
        private string name = string.Empty;

        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private string emailAddress = string.Empty;

        public string EmailAddress
        {
            get => emailAddress;
            set
            {
                emailAddress = value;
                OnPropertyChanged();
            }
        }
    }
}
