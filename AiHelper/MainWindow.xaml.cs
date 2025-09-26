using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OpenAI.Chat;

namespace AiHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();
            viewModel = new MainViewModel(this.BringToTop, this.ShowAsDialog);
            this.viewModel.Outputs.CollectionChanged += this.Outputs_CollectionChanged;
            this.DataContext = viewModel;

            this.PreviewKeyDown += MainWindow_PreviewKeyDown;

            Loaded += MainWindow_Loaded;

        }

        private void Outputs_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (this.viewModel.Outputs.Count == 0)
            {
                return;
            }

            Task.Run(async () =>
            {
                await Task.Delay(200);

                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        var lastItem = this.OutputDisplay.Items[this.OutputDisplay.Items.Count - 1];
                        this.OutputDisplay.ScrollIntoView(lastItem);
                    }
                    catch { }
                });
            });
        }

        private bool ShowAsDialog(Window window)
        {
            window.Owner = this;
            var result = window.ShowDialog();
            return result != null && result.Value == true;
        }
        private nint hwnd;

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.SpaceButton.Focus();
            this.viewModel.Initialize();

            hwnd = new WindowInteropHelper(this).EnsureHandle();
        }

        private void BringToTop()
        {
            Dispatcher.Invoke(async () =>
            {
                var hwndForground = GetForegroundWindow();
                if (hwnd == hwndForground)
                {
                    return;
                }

                var threadId1 = GetWindowThreadProcessId(hwndForground, IntPtr.Zero);
                var threadId2 = GetWindowThreadProcessId(hwnd, IntPtr.Zero);

                if (threadId1 != threadId2)
                {
                    AttachThreadInput(threadId1, threadId2, true);
                    SetForegroundWindow(hwnd);
                    AttachThreadInput(threadId1, threadId2, false);
                }
                else
                    SetForegroundWindow(hwnd);

            });
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        [DllImport("user32.dll")]
        public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        private async void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool handled = await this.viewModel.KeyPressed(e.Key);
            e.Handled = handled;
        }
    }
}