using System.IO;
using System.Text;
using System.Windows;
using AiHelper.Config;
using AiHelper.Config.Models;
using Newtonsoft.Json;
using OpenAI.Chat;

namespace AiHelper
{
    internal class AiAccessor
    {
        private static ChatClient? client;

        private static Func<string, Task>? ErrorHandler;

        public static async Task Initialize(Func<string, Task> errorHandler)
        {
            ErrorHandler = errorHandler;

            if (string.IsNullOrEmpty(ConfigProvider.Config?.OpenAiApiKey))
            {
                await Speaker.Say("In den Einstellungen fehlt der Open AI Api Key.");
                return;
            }

            InitClient();
        }

        private static void InitClient()
        {
            string model = "o4-mini";
            string apiKey = ConfigProvider.Config!.OpenAiApiKey;
            client = new(model: model, apiKey: apiKey);
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
