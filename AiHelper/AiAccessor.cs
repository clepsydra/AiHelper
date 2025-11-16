using System.IO;
using System.Text;
using System.Windows;
using AiHelper.Config;
using Newtonsoft.Json;
using OpenAI.Chat;

namespace AiHelper
{
    internal class AiAccessor
    {
        private static ChatClient? client;

        private static string? apiKey = null;

        private static string ConfigFileName => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "aiHelper.config");

        private static Func<string, Task>? ErrorHandler;
        private static Action<bool>? StayOnTop;
        private static Func<Window, bool>? ShowDialog;

        public static async Task Initialize(Func<Window, bool> showDialogInvoker, Action<bool> setStayOnTop, Func<string, Task> errorHandler)
        {
            ErrorHandler = errorHandler;
            StayOnTop = setStayOnTop;
            ShowDialog = showDialogInvoker;

            if (!File.Exists(ConfigFileName))
            {
                await EditConfiguration();
                return;
            }

            AiHelperConfig? config = null;

            try
            {
                string json = File.ReadAllText(ConfigFileName);
                config = JsonConvert.DeserializeObject<AiHelperConfig>(json);
            }
            catch (Exception ex)
            {
                await ErrorHandler("Konfiguration kann nicht gelesen werden: " + ex.Message);
                await Speaker.Say("Konfiguration kann nicht gelesen werden.");
            }

            if (string.IsNullOrEmpty(config?.OpenAiApiKey))
            {
                await EditConfiguration();
                return;
            }

            apiKey = config.OpenAiApiKey;

            InitClient();
        }

        private static void InitClient()
        {
            string model = "o4-mini";
            client = new(model: model, apiKey: apiKey);
        }

        public static async Task EditConfiguration()
        {
            StayOnTop?.Invoke(false);
            var configDialog = new ConfigUI(apiKey);
            var result = ShowDialog?.Invoke(configDialog);
            if (result != true)
            {
                return;
            }

            var config = new AiHelperConfig { OpenAiApiKey = configDialog.ApiKey };
            apiKey = config.OpenAiApiKey;
            InitClient();
            StayOnTop?.Invoke(true);

            try
            {
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(ConfigFileName, json);
            }
            catch (Exception ex)
            {
                await OnErrorOccurred(ex.Message);
            }
        }

        private async static Task OnErrorOccurred(string message)
        {
            if (ErrorHandler == null)
            {
                throw new Exception("Error Handler not defined");
            }

            await ErrorHandler(message);
        }

        public static async Task<string> AskAi(List<ChatMessage> chatHistory)
        {
            if (client == null)
            {
                await OnErrorOccurred("Client not available");
                return string.Empty;
            }

            ChatCompletion completion = await client.CompleteChatAsync(chatHistory);

            var fullText = new StringBuilder();
            foreach (var content in completion.Content)
            {
                fullText.AppendLine(content.Text);
            }

            return fullText.ToString();
        }
    }
}
