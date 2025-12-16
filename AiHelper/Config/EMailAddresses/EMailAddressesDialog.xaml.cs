using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AiHelper.Config.EMailAddresses
{
    /// <summary>
    /// Interaction logic for EMailAddressesDialog.xaml
    /// </summary>
    public partial class EMailAddressesDialog : Window
    {
        private EMailAddressesViewModel viewModel;

        public EMailAddressesDialog(Dictionary<string, string> mailAdresses)
        {
            InitializeComponent();

            viewModel = new EMailAddressesViewModel(mailAdresses, CloseDialog);
            DataContext = viewModel;
        }

        private void CloseDialog(bool result)
        {
            DialogResult = result;
            Close();
        }

        public Dictionary<string, string> EMailAddresses => this.viewModel.EMailAddresses
                    .Where(entry => !string.IsNullOrEmpty(entry.EmailAddress) && !string.IsNullOrEmpty(entry.Name))
                    .ToDictionary(entry => entry.Name, entry => entry.EmailAddress);
    }
}
