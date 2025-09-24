using System.Diagnostics;
using System.Windows;

namespace AiHelper.Config
{
    /// <summary>
    /// Interaction logic for ConfigUI.xaml
    /// </summary>
    public partial class ConfigUI : Window
    {
        private readonly ConfigViewModel viewModel;

        public ConfigUI(string? apiKey)
        {
            InitializeComponent();

            this.viewModel = new ConfigViewModel(apiKey, this.CloseDialog);
            this.DataContext = viewModel;
        }

        private void CloseDialog(bool result)
        {
            this.DialogResult = result;
        }

        public string ApiKey => this.viewModel.ApiKey;

        private void NavigateTo(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo() { FileName = e.Uri.ToString(), UseShellExecute = true });
        }
    }
}
