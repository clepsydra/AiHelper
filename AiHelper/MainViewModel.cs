using System.Collections.ObjectModel;
using System.IO;
using System.Speech.Synthesis;
using System.Text;
using System.Windows;
using System.Windows.Input;
using AiHelper.Actions;
using AiHelper.Config;
using Newtonsoft.Json;
using OpenAI.Chat;

namespace AiHelper
{
    internal class MainViewModel : ViewModelBase
    {
        private readonly Action bringToFront;
        private readonly Func<Window, bool> showDialog;
        private readonly Action scrollToEnd;
        private VoiceChat voiceChat;

        public MainViewModel(Action bringToFront, Func<Window, bool> showDialog, Action scrollToEnd)
        {
            this.bringToFront = bringToFront;
            this.showDialog = showDialog;
            this.scrollToEnd = scrollToEnd;
            Task.Run(HandleStayOnTop);

            this.Options = new List<ICustomAction>
            {
                new ShortSummaryImageAction(this.AddToOutput, () => this.ShowImage),
                new ReadImageAction(this.AddToOutput, () => this.ShowImage),
            };

            this.Options.Add(new HelpAction(this.Options));
            this.OpenConfigCommand = new RelayCommand(EditConfiguration);
        }

        private async void EditConfiguration()
        {
            await ConfigProvider.EditConfiguration();
            this.voiceChat.SilenceVolumneLimit = ConfigProvider.Config.SoundConfig.SilenceVolumeLimit;
        }

        private async Task HandleErrors(string errorMessage)
        {
            this.AddToOutput("Ein Fehler ist aufgetreten: " + errorMessage);
            Speaker2.Say("Ein Fehler ist aufgetreten. " + errorMessage);
        }

        private bool showImage = false;
        public bool ShowImage
        {
            get => this.showImage;
            set
            {
                this.showImage = value;
            }
        }

        private bool stayOnTop = false;
        public bool StayOnTop
        {
            get => this.stayOnTop;
            set
            {
                this.stayOnTop = value;
                this.OnPropertyChanged();
            }
        }

        private async void HandleStayOnTop()
        {
            while (true)
            {
                if (this.StayOnTop)
                {
                    bringToFront();
                }

                await Task.Delay(2000);
            }
        }

        public List<ICustomAction> Options { get; }

        private DateTime keyLastPressedAt = DateTime.MinValue;

        internal async Task<bool> KeyPressed(Key key)
        {
            if (key == Key.VolumeDown || key == Key.VolumeUp || key == Key.VolumeMute)
            {
                return false;
            }

            if (DateTime.Now.Subtract(keyLastPressedAt).TotalMilliseconds < 500)
            {
                return false;
            }

            keyLastPressedAt = DateTime.Now;

            string? text = Enum.GetName(key);
            if (text == null)
            {
                return false;
            }

            if (text.StartsWith("D", StringComparison.OrdinalIgnoreCase))
            {
                text = text.Substring(1);
            }

            if (key == Key.Space)
            {
                text = "Leertaste";
            }

            Speaker2.Say(text);

            if (key == Key.F12)
            {
                this.CreateDummyOutput();
                return true;
            }

            var registeredOption = this.Options.FirstOrDefault(option => option.Key == key);
            if (registeredOption == null)
            {
                return false;
            }

            await registeredOption.Run();

            return true;
        }

        private void CreateDummyOutput()
        {
            this.AddToOutput("Some Dummy output at " + DateTime.Now.ToString("yyyyMMdd HHmmss"));
        }

        internal async Task Initialize()
        {
            await ConfigProvider.Initialize(this.showDialog, stayOnTop => this.StayOnTop = stayOnTop, this.HandleErrors);
            Speaker2.Initialize();
            await AiAccessor.Initialize(this.HandleErrors);

            this.voiceChat = new VoiceChat();
        }

        public ICommand OpenConfigCommand { get; }

        private bool IsBusy { get; set; }

        public ObservableCollection<TextOutput> Outputs { get; } = new ObservableCollection<TextOutput>();

        private void AddToOutput(string text)
        {
            this.Outputs.Add(new TextOutput(text.Trim()));
            if (this.Outputs.Count > 50)
            {
                this.Outputs.RemoveAt(0);
            }

            this.scrollToEnd();
        }
    }
}
