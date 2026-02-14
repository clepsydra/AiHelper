using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AiHelper.Config;
using OpenAI.Audio;
using static System.Net.Mime.MediaTypeNames;

namespace AiHelper
{
    internal class SpeechRecognition
    {
        public static async Task<string> Recognize(byte[] mp3Bytes, string language = "", string prompt = "")
        {
            var client = new OpenAI.OpenAIClient(ConfigProvider.Config.OpenAiApiKey);
            //string modelId = "gpt-audio-mini";
            //string modelId = "gpt-4o-transcribe";
            string modelId = "whisper-1";
            var audioClient = client.GetAudioClient(modelId);

            try
            {
                using MemoryStream stream = new MemoryStream(mp3Bytes);
                AudioTranscriptionOptions? options = new AudioTranscriptionOptions
                {
                    Language = language,
                    Prompt = prompt,
                };

                var result = await audioClient.TranscribeAudioAsync(stream, "input.mp3", options);
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

                if (ex is HttpRequestException
                            || (ex is AggregateException aggregateException && aggregateException.InnerExceptions.Any(e => e is HttpRequestException || e is ClientResultException)))
                {
                    await Speaker.Say("Es gibt anscheinend Probleme mit der Internet Verbindung.");
                }

                return string.Empty;
            }
        }
    }
}
