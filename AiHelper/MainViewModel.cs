using System.Collections.ObjectModel;
using System.IO;
using System.Speech.Synthesis;
using System.Text;
using System.Windows;
using System.Windows.Input;
using AiHelper.Config;
using Newtonsoft.Json;
using OpenAI.Chat;

namespace AiHelper
{
    internal class MainViewModel : ViewModelBase
    {
        private string? apiKey;

        private ChatClient? client;
        private readonly Action bringToFront;
        private readonly Func<Window, bool> showDialog;
        private SpeechSynthesizer synthesizer = new SpeechSynthesizer();
        

        public MainViewModel(Action bringToFront, Func<Window, bool> showDialog)
        {
            this.SummarizeImageInOneSentenceComand = new RelayCommand(this.SummarizeImageInOneSentence);
            this.SummarizeImageComand = new RelayCommand(this.SummarizeImage);
            this.OpenConfigCommand = new RelayCommand(this.HandleCreateConfiguration);

            synthesizer.SetOutputToDefaultAudioDevice();
            this.bringToFront = bringToFront;
            this.showDialog = showDialog;
            Task.Run(HandleStayOnTop);
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

        private bool stayOnTop = true;
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

        public ICommand SummarizeImageInOneSentenceComand { get; }

        public ICommand SummarizeImageComand { get; }

        private async Task<BinaryData> CaptureImage()
        {
            await Task.Delay(1000);
            await this.Say("Halte den Gegenstand vor die Kamera");
            await Task.Delay(1000);
            await this.Say("Drei");
            await Task.Delay(1000);
            await this.Say("Zwei");
            await Task.Delay(1000);
            await this.Say("Eins");
            await Task.Delay(1000);
            await this.Say("Aufnahme läuft");

            var data = ImageHelper.CaptureImage(this.ShowImage);
            await this.Say("Bild aufgenommen, Ich analysiere jetzt das Bild");

            BinaryData binaryData = BinaryData.FromBytes(data);

            return binaryData;
        }

        private async Task SummarizeImageInOneSentence()
        {
            if (this.IsBusy)
            {
                return;
            }

            if (client == null)
            {
                await this.Say("Konfiguration ist nicht vollständig.");
                return;
            }

            this.IsBusy = true;

            try
            {
                //this.AddToOutput("Dies ist ein Test fglkjdfgfdgh dsf dsfg dfg dfg sdfg sg s gfh sgh dgh d dfgh gfh f fgh dfgh fdgh dfgh fdgh fdgh fdgh fdh fdg dfh fgh" + DateTime.Now.ToString("yyyyMMdd-HHmmss"));

                var imageData = await CaptureImage();

                var message = new UserChatMessage(
                    ChatMessageContentPart.CreateTextPart("Fasse in einem Satz zusammen worum es sich hier handelt"),
                    ChatMessageContentPart.CreateImagePart(imageData, "image/png"));

                List<ChatMessage> chatHistory = [];
                chatHistory.Add(message);

                ChatCompletion completion = await client.CompleteChatAsync(chatHistory);

                var fullText = new StringBuilder();
                foreach (var content in completion.Content)
                {
                    this.AddToOutput(content.Text);
                    fullText.AppendLine(content.Text);
                }

                await this.Say(fullText.ToString());
            }
            catch(Exception ex)
            {
                await this.HandleException(ex);
            }

            this.IsBusy = false;
        }

        private async Task SummarizeImage()
        {
            if (this.IsBusy)
            {
                return;
            }

            if (client == null)
            {
                await this.Say("Konfiguration ist nicht vollständig.");
                return;
            }

            this.IsBusy = true;

            try
            {
                var imageData = await CaptureImage();

                var message = new UserChatMessage(
                    ChatMessageContentPart.CreateTextPart("Erstelle eine Zusammenfassung zu dem Bild"),
                    ChatMessageContentPart.CreateImagePart(imageData, "image/png"));

                List<ChatMessage> chatHistory = [];
                chatHistory.Add(message);

                ChatCompletion completion = await client.CompleteChatAsync(chatHistory);

                var fullText = new StringBuilder();
                foreach (var content in completion.Content)
                {
                    this.AddToOutput(content.Text);
                    fullText.AppendLine(content.Text);
                }

                await this.Say(fullText.ToString());
            }
            catch (Exception ex)
            {
                await this.HandleException(ex);
            }

            this.IsBusy = false;
        }


        private async Task HandleException(Exception ex)
        {
            this.AddToOutput($"Fehler: {ex.Message}");

            try
            {
                await this.Say($"Ein Fehler ist aufgetreten: {ex.Message}\r\n");
            }
            catch
            { }
        }

        private async Task Say(string text)
        {
            try
            {
                await Task.Run(() => this.synthesizer.Speak(text));
            }
            catch (Exception ex)
            {
                this.AddToOutput($"Speak Fehler: {ex.Message}");
            }
        }

        internal async Task<bool> KeyPressed(Key key)
        {
            if (key == Key.VolumeDown || key == Key.VolumeUp || key == Key.VolumeMute)
            {
                return false;
            }

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

            await this.Say(text);

            switch(key)
            {
                case Key.Space:
                    await this.SummarizeImageInOneSentence();
                    return true;                    
                case Key.F2:
                    await this.SummarizeImage();
                    return true;
                case Key.F1:
                    await this.Help();
                    return true;
                case Key.F12:
                    this.CreateDummyOutput();
                    return true;
            }

            return false;
        }

        private void CreateDummyOutput()
        {
            this.AddToOutput("Some Dummy output at " + DateTime.Now.ToString("yyyyMMdd HHmmss"));
        }

        private async Task Help()
        {
            if (this.IsBusy)
            {
                return;
            }

            this.IsBusy = true;
            await this.Say("Dieses Programm unterstützt Dich.");
            await this.Say("Aktuell kann es Bilder erkennen und zusammenfassen.");
            await this.Say("Wenn Du die Leertaste drückst wird über die eingebaute Kamera ein Bild von dem Gegenstand vor der Kamera gemacht und der Inhalt in einem Satz zusammengefasst");
            await this.Say("Also wenn Du z.B. Post bekommst, oder Dir bei der Medikamentenpackung nicht  sicher bist: Halte es vor die Webcam und drücke die Leertaste.");
            await this.Say("Wenn Du F2 drückst wird das Gleiche gemacht, aber das Bild wird etwas mehr erläutert.");
            this.IsBusy = false;
        }

        private string ConfigFileName => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "aiHelper.config");

        internal async void Initialize()
        {            
            if (!File.Exists(this.ConfigFileName))
            {
                await HandleCreateConfiguration();
                return;
            }

            AiHelperConfig? config = null;

            try
            {
                string json = File.ReadAllText(this.ConfigFileName);
                config = JsonConvert.DeserializeObject<AiHelperConfig>(json);
            }
            catch(Exception ex)
            {
                this.AddToOutput("Konfiguration kann nicht gelesen werden: " + ex.Message);
                await this.Say("Konfiguration kann nicht gelesen werden.");                
            }

            if (string.IsNullOrEmpty(config?.OpenAiApiKey))
            {
                await HandleCreateConfiguration();
                return;
            }

            this.apiKey = config.OpenAiApiKey;
            this.InitClient();
            
        }

        private void InitClient()
        {
            this.client = new(model: "o4-mini", apiKey: apiKey);
        }

        public ICommand OpenConfigCommand { get; }

        private async Task HandleCreateConfiguration()
        {
            this.StayOnTop = false;
            var configDialog = new ConfigUI(this.apiKey);
            var result = this.showDialog(configDialog);
            if (!result)
            {
                return;
            }

            var config = new AiHelperConfig { OpenAiApiKey = configDialog.ApiKey };
            this.apiKey = config.OpenAiApiKey;
            this.InitClient();
            this.StayOnTop = true;

            try
            {
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(this.ConfigFileName, json);
            }
            catch(Exception ex)
            {
                await this.HandleException(ex);
            }
        }

        private bool IsBusy { get; set; }

        public ObservableCollection<TextOutput> Outputs { get; } = new ObservableCollection<TextOutput>();

        private void AddToOutput(string text)
        {
            this.Outputs.Add(new TextOutput(text));
            if (this.Outputs.Count > 50)
            {
                this.Outputs.RemoveAt(0);
            }
        }
    }
}
