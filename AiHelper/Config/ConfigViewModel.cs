using System.Windows.Input;

namespace AiHelper.Config
{
    public class ConfigViewModel : ViewModelBase
    {
        private readonly Action<bool> close;

        public ConfigViewModel(string? apiKey, Action<bool> close)
        {
            this.close = close;
            this.OkCommand = new RelayCommand(() => this.close(true));
            this.CancelCommand = new RelayCommand(() => this.close(false));
            this.apiKey = apiKey ?? "Not Set";
        }

        public ICommand OkCommand { get; }

        public ICommand CancelCommand { get; }

        private string apiKey;

        public string ApiKey
        {
            get => this.apiKey;
            set
            {
                this.apiKey = value;
                this.OnPropertyChanged();
            }
        }
    }
}
