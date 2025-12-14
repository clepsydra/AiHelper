using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AiHelper.Config;
using static System.Net.Mime.MediaTypeNames;

namespace AiHelper
{
    internal class SpeechRecognition
    {
        public static async Task<string> Recognize(byte[] mp3Bytes)
        {
            var client = new OpenAI.OpenAIClient(ConfigProvider.Config.OpenAiApiKey);
            //string modelId = "gpt-audio-mini";
            //string modelId = "gpt-4o-transcribe";
            string modelId = "whisper-1";
            var audioClient = client.GetAudioClient(modelId);

            try
            {
                using MemoryStream stream = new MemoryStream(mp3Bytes);
                var result = await audioClient.TranscribeAudioAsync(stream, "input.mp3");
                string? text = result.Value?.Text;
                if (string.IsNullOrEmpty(text))
                {
                    text = string.Empty;
                }

                Debug.WriteLine($"SpeechRecognition.Recognize: {text}");
                return text;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SpeechRecognition.Recognize: Exception: {ex.ToString()}");
                return string.Empty;
            }
        }
    }
}
