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
using Microsoft.Extensions.Primitives;
using NAudio.Wave;
using OpenAI.Audio;
using static System.Net.Mime.MediaTypeNames;

namespace AiHelper
{
    internal class Speaker2
    {
        private static string? apiKey;

        private static GeneratedSpeechVoice generatedSpeechVoice = GeneratedSpeechVoice.Shimmer;

        public static void SetVoice(string voice)
        {
            switch (voice)
            {
                case "Alloy":
                    generatedSpeechVoice = GeneratedSpeechVoice.Alloy;
                    return;                
                case "Echo":
                    generatedSpeechVoice = GeneratedSpeechVoice.Echo;
                    return;
                case "Fable":
                    generatedSpeechVoice = GeneratedSpeechVoice.Fable;
                    return;
                case "Onyx":
                    generatedSpeechVoice = GeneratedSpeechVoice.Onyx;
                    return;
                case "Nova":
                    generatedSpeechVoice = GeneratedSpeechVoice.Nova;
                    return;               
                case "Shimmer":
                    generatedSpeechVoice = GeneratedSpeechVoice.Shimmer;
                    return;
                case "Ash":
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                    generatedSpeechVoice = GeneratedSpeechVoice.Ash;
                    return;
                case "Ballad":
                    generatedSpeechVoice = GeneratedSpeechVoice.Ballad;
                    return;
                case "Coral":
                    generatedSpeechVoice = GeneratedSpeechVoice.Coral;
                    return;
                case "Sage":
                    generatedSpeechVoice = GeneratedSpeechVoice.Sage;
                    return;
                case "Verse":
                    generatedSpeechVoice = GeneratedSpeechVoice.Verse;
                    return;
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            }
        }

        public static void Initialize()
        {
            if (string.IsNullOrEmpty(ConfigProvider.Config?.OpenAiApiKey))
            {
                throw new Exception("Open AI Api Key nicht gesetzt");
            }

            apiKey = ConfigProvider.Config.OpenAiApiKey;
            SetVoice(ConfigProvider.Config.SoundConfig.Voice);

            Task.Run(ProcessQueue);
        }

        private static Queue<string> messageQueue = new Queue<string>();

        /// <summary>
        /// Texts for which the output shall be cached
        /// </summary>
        public static HashSet<string> ToCache { get; } = new HashSet<string>();

        private static Dictionary<string, byte[]> cachedOutput = new Dictionary<string, byte[]>();

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

            var result = audioClient.GenerateSpeech(text, generatedSpeechVoice, options);
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

                if (!cachedOutput.TryGetValue(message, out var bytes))
                {
                    var result = audioClient.GenerateSpeech(message, generatedSpeechVoice, options);
                    bytes = result.Value.ToArray();

                    if (ToCache.Contains(message))
                    {
                        cachedOutput[message] = bytes;
                    }
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

                while (waveOut != null && waveOut.PlaybackState == PlaybackState.Playing)
                {
                    await Task.Delay(25);
                }

                messageQueue.Dequeue();
            }
        }

        internal static async Task SayAndCache(string message, bool wait = false)
        {
            ToCache.Add(message);
            await Say(message, wait);
        }
    }
}
