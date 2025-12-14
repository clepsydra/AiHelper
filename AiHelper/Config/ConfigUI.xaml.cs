using System.Diagnostics;
using System.Windows;
using AiHelper.Config.Models;

namespace AiHelper.Config
{
    /// <summary>
    /// Interaction logic for ConfigUI.xaml
    /// </summary>
    public partial class ConfigUI : Window
    {
        private readonly ConfigViewModel viewModel;

        public ConfigUI(AiHelperConfig? config)
        {
            InitializeComponent();

            this.viewModel = new ConfigViewModel(config, this.CloseDialog);
            this.DataContext = viewModel;

            this.Closed += OnClosed;
        }

        private void OnClosed(object? sender, EventArgs e)
        {
            this.viewModel.StopListening();
        }

        private void CloseDialog(bool result)
        {
            this.DialogResult = result;
        }
        public AiHelperConfig Config => this.viewModel.Config;

        private void NavigateTo(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo() { FileName = e.Uri.ToString(), UseShellExecute = true });
        }
    }
}
