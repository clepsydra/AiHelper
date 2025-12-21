using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AiHelper.Config.Models;
using Newtonsoft.Json;

namespace AiHelper.Config
{
    public class ConfigProvider
    {
        private static string ConfigFileName => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "aiHelper.config");

        private static Func<string, Task>? ErrorHandler;
        private static Action<bool>? StayOnTop;
        private static Func<Window, bool>? ShowDialog;

        public static AiHelperConfig? Config { get; private set; }

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

            try
            {
                string json = File.ReadAllText(ConfigFileName);
                Config = JsonConvert.DeserializeObject<AiHelperConfig>(json);
            }
            catch (Exception ex)
            {
                await ErrorHandler("Konfiguration kann nicht gelesen werden: " + ex.Message);
                Speaker2.Say("Konfiguration kann nicht gelesen werden.");
            }

            if (string.IsNullOrEmpty(Config?.OpenAiApiKey))
            {
                await EditConfiguration();
                return;
            }
        }

        public static async Task EditConfiguration()
        {
            StayOnTop?.Invoke(false);
            var configDialog = new ConfigUI(Config);
            var result = ShowDialog?.Invoke(configDialog);
            if (result != true)
            {
                return;
            }

            Config = configDialog.Config;
            StayOnTop?.Invoke(true);

            try
            {
                string json = JsonConvert.SerializeObject(Config, Formatting.Indented);
                File.WriteAllText(ConfigFileName, json);
            }
            catch (Exception ex)
            {
                await OnErrorOccurred(ex.Message);
            }

            Speaker2.SetVoice(Config.SoundConfig.Voice);
        }

        private async static Task OnErrorOccurred(string message)
        {
            if (ErrorHandler == null)
            {
                throw new Exception("Error Handler not defined");
            }

            await ErrorHandler(message);
        }
    }
}
