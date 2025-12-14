using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AiHelper.Config;
using NAudio.Wave;
using OpenAI.Audio;
using static System.Net.Mime.MediaTypeNames;

namespace AiHelper
{
    internal class Speaker2
    {
        private static string apiKey;

        public static void Initialize()
        {
            if (string.IsNullOrEmpty(ConfigProvider.Config?.OpenAiApiKey))
            {
                throw new Exception("Open AI Api Key nicht gesetzt");
            }

            apiKey = ConfigProvider.Config.OpenAiApiKey;

            Task.Run(ProcessQueue);
        }

        private static Queue<string> messageQueue = new Queue<string>();

        public static async Task Say(string text, bool wait = false)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            Debug.WriteLine($"Say: {text}");
            messageQueue.Enqueue(text);

            while (wait && messageQueue.Any())
            { 
                await Task.Delay(25);
            }
        }

        public static async Task Say2(string text)
        {
            var client = new OpenAI.OpenAIClient(apiKey);
            //string modelId = "gpt-audio-mini";
            string modelId = "gpt-4o-mini-tts";
            var audioClient = client.GetAudioClient(modelId);
            var options = new SpeechGenerationOptions
            {
                ResponseFormat = "mp3",                
            };

            var result = audioClient.GenerateSpeech(text, OpenAI.Audio.GeneratedSpeechVoice.Shimmer, options);
            var bytes = result.Value.ToArray();
            using MemoryStream stream = new MemoryStream(bytes);
            var reader = new Mp3FileReader(stream);
            var waveOut = new WaveOut();
            waveOut.Init(reader);
            waveOut.Play();

            while (waveOut.PlaybackState == PlaybackState.Playing)
            {
                await Task.Delay(100);
            }
        }

        private static async void ProcessQueue()
        {
            var client = new OpenAI.OpenAIClient(apiKey);
            //string modelId = "gpt-audio-mini";
            string modelId = "gpt-4o-mini-tts";
            var audioClient = client.GetAudioClient(modelId);
            var options = new SpeechGenerationOptions
            {
                ResponseFormat = "mp3",
            };

            WaveOut waveOut = null;
            MemoryStream? stream = null;

            while (true)
            {
                string? message;
                while (!messageQueue.TryPeek(out message))
                {
                    await Task.Delay(50);
                }

                var result = audioClient.GenerateSpeech(message, GeneratedSpeechVoice.Shimmer, options);
                var bytes = result.Value.ToArray();

                while (waveOut != null && waveOut.PlaybackState == PlaybackState.Playing)
                {
                    await Task.Delay(25);
                }

                if (stream != null)
                {
                    stream.Dispose();
                }

                stream = new MemoryStream(bytes);
                var reader = new Mp3FileReader(stream);                

                waveOut = new WaveOut();
                waveOut.Init(reader);
                waveOut.Play();

                messageQueue.Dequeue();

                //while (waveOut.PlaybackState == PlaybackState.Playing)
                //{
                //    await Task.Delay(25);
                //}
            }
        }
    }
}
