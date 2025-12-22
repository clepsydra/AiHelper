using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AiHelper.Config.EMailAddresses
{
    public class EMailAddressesViewModel : ViewModelBase
    {
        public EMailAddressesViewModel(Dictionary<string, string> mailAdresses, Action<bool> closeDialog)
        {            
            var models = mailAdresses.Select(x => new EMailAddressViewModel { Name = x.Key, EmailAddress = x.Value });
            EMailAddresses.AddRange(models);

            AddCommand = new RelayCommand(this.ExecuteAdd);
            DeleteCommand = new RelayCommand(this.ExecuteDelete);
            this.closeDialog = closeDialog;

            OkCommand = new RelayCommand(() => closeDialog(true));
            CancelCommand = new RelayCommand(() => closeDialog(true));
        }

        public ObservableCollection<EMailAddressViewModel> EMailAddresses { get; set; } = new ObservableCollection<EMailAddressViewModel>();

        private EMailAddressViewModel? selectedEMailAddress;
        private readonly Action<bool> closeDialog;

        public EMailAddressViewModel? SelectedEMailAddress
        {
            get => selectedEMailAddress;
            set
            {
                this.selectedEMailAddress = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand AddCommand { get; }

        private void ExecuteAdd()
        {
            this.EMailAddresses.Add(new EMailAddressViewModel());
        }

        public ICommand DeleteCommand { get; }

        private void ExecuteDelete()
        {
            if (this.SelectedEMailAddress == null)
            {
                return;
            }

            this.EMailAddresses.Remove(this.SelectedEMailAddress);
        }

        public ICommand OkCommand { get; }

        public ICommand CancelCommand { get; }
    }
}
