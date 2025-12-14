using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiHelper.Config;
using AiHelper.Plugin;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using NAudio.Wave;
using Newtonsoft.Json;
//using Vosk;

namespace AiHelper
{
    class VoiceChat
    {
        //private VoskRecognizer rec;

        private WaveInEvent waveIn;

        private ChatHistory history = new ChatHistory();

        private Kernel kernel;

        private int noInputCount = 0;

        private StringBuilder currentInputBuilder = new StringBuilder();

        private const int activationTimeoutInSeconds = 60;
        private DateTime lastInteractionAt = DateTime.MinValue;

        private bool isRecording = false;
        private DateTime silenceStartedAt;
        private bool silenceStarted;
        private bool isListening = false;

        private MemoryStream gatheredWavData = new();

        public VoiceChat()
        {
            //string modelFile = @"D:\Downloads\vosk-model-small-de-0.15\vosk-model-small-de-0.15";
            //string modelFile = @"D:\Downloads\vosk-model-de-0.21\vosk-model-de-0.21";
            //var modelPath = new Model(modelFile);
            //rec = new VoskRecognizer(modelPath, 16000);

            waveIn = new WaveInEvent();
            waveIn.DeviceNumber = 0;
            waveIn.WaveFormat = new WaveFormat(16000, 1);
            waveIn.BufferMilliseconds = 100;

            this.SilenceVolumneLimit = ConfigProvider.Config?.SoundConfig.SilenceVolumeLimit ?? 0.005;            

            waveIn.DataAvailable += (object? sender, WaveInEventArgs e) =>
            {
                if (!isListening)
                {
                    return;
                }

                double maxVolume = AudioTools.GetMaxVolume(e);

                if (maxVolume > SilenceVolumneLimit)
                {
                    isRecording = true;
                    silenceStarted = false;
                }
                else
                {
                    if (DateTime.Now.Subtract(lastInteractionAt).TotalSeconds > activationTimeoutInSeconds)
                    {
                        Debug.WriteLine("Activation ended!");
                        this.isListening = false;
                        this.isRecording = false;
                        gatheredWavData.Close();
                        gatheredWavData.Dispose();
                        gatheredWavData = new MemoryStream();

                        Speaker2.Say("Ich warte jetzt, bis Du wieder das Wort Computer sagst.");

                        Task.Run(WaitForActivation);
                    }
                }

                if (isRecording)
                {
                    gatheredWavData.Write(e.Buffer, 0, e.BytesRecorded);

                    if (maxVolume < SilenceVolumneLimit)
                    {
                        if (!silenceStarted)
                        {
                            Debug.WriteLine("Silence Startedf");
                            silenceStarted = true;
                            silenceStartedAt = DateTime.Now;
                        }
                        else if (DateTime.Now.Subtract(silenceStartedAt).TotalSeconds > 2)
                        {
                            Debug.WriteLine("Silence lasted for 2 seconds");
                            isRecording = false;
                            silenceStarted = false;

                            var wavData = gatheredWavData.ToArray();
                            gatheredWavData.Close();
                            gatheredWavData = new MemoryStream();

                            HandleInput2(wavData);
                        }
                    }
                }


                //if (rec.AcceptWaveform(e.Buffer, e.BytesRecorded))
                //{
                //    string json = rec.Result();
                //    Debug.WriteLine(json);
                //    var result = JsonConvert.DeserializeObject<VoskResult>(json);
                //    if (string.IsNullOrEmpty(result?.Text))
                //    {
                //        if (currentInputBuilder.Length == 0)
                //        {
                //            return;
                //        }

                //        if (++noInputCount == 1)
                //        {
                //            string currentInput = currentInputBuilder.ToString();
                //            currentInputBuilder.Clear();
                //            noInputCount = 0;
                //            Task.Run(async () => await Ask(currentInput));
                //        }
                //    }

                //    if (result.Text.StartsWith("Computer", StringComparison.OrdinalIgnoreCase))
                //    {
                //        isActivated = true;
                //        currentInputBuilder.Append(result.Text.Substring(8));
                //        return;
                //    }                    

                //    currentInputBuilder.Append(result.Text);
                //}
            };

            waveIn.StartRecording();

            Initialize();


        }

        //private async void HandleInput(byte[] input)
        //{
        //    if (rec.AcceptWaveform(input, input.Length))
        //    {
        //        string json = rec.Result();
        //        Debug.WriteLine(json);
        //        var result = JsonConvert.DeserializeObject<VoskResult>(json);
        //        if (!string.IsNullOrEmpty(result?.Text))
        //        {
        //            string currentInput = result.Text;
        //            isListening = false;
        //            Task.Run(async () => await Ask(currentInput));
        //        }
        //    }
        //}

        private void HandleInput2(byte[] input)
        {
            this.lastInteractionAt = DateTime.Now;
            isListening = false;

            var mp3Bytes = Mp3Converter.PcmBytesToMp3Bytes(input, new WaveFormat(16000, 1), 128);

            Task.Run(async () =>
            {
                string text = await SpeechRecognition.Recognize(mp3Bytes);

                if (!string.IsNullOrEmpty(text))
                {
                    await Ask(text);
                }
            });            
        }

        bool isActivated = false;

        public double SilenceVolumneLimit { get; set; }

        private void Initialize()
        {
            var builder = Kernel.CreateBuilder();
            //string modelId = "gpt-5-nano";
            string modelId = "gpt-4o-mini";

            string apiKey = ConfigProvider.Config.OpenAiApiKey;

            builder.AddOpenAIChatCompletion(modelId, apiKey);

            kernel = builder.Build();
            PluginRegistrar.RegisterPlugins(kernel, this.CloseSession);

            AddSystemMessage();

            Speaker2.Say("Ich warte jetzt, bis Du das Wort Computer sagst.");
            Task.Run(WaitForActivation);

        }

        private void AddSystemMessage()
        {
            history.AddSystemMessage(@"Du hilfst alten Menschen.
Antworte eher kurz und mit einfachen Worten.
Und maximal 3 Sätzen am Stück.
Wenn Du mit einer Aktion fertig bist fragen den Benutzer ob er noch etwas möchte.
Wenn er nichts mehr möchte rufe das ClosePlugin auf.");
        }

        private void WaitForActivation()
        {
            Debug.WriteLine("Waiting for activation...");
            this.isListening = false;
            this.isRecording = false;

            
            ActivatorByCodeword activator = new ActivatorByCodeword();
            activator.WaitForActivation();

            history.Clear();
            AddSystemMessage();

            isListening = true;
            isActivated = true;
            lastInteractionAt = DateTime.Now;
            Debug.WriteLine("Activated!");
            Speaker2.Say("Ich höre zu!");
        }

        private void CloseSession()
        {
            Debug.WriteLine("Close Session started");
            this.isListening = false;
            this.isRecording = false;
            this.isActivated = false;

            gatheredWavData.Close();
            gatheredWavData.Dispose();
            gatheredWavData = new MemoryStream();

            Task.Run(async () =>
            {
                this.WaitForActivation();
            });
        }

        public async Task Ask(string text)
        {
            this.lastInteractionAt = DateTime.Now;
            Debug.WriteLine("Ask: " + text);
            history.AddUserMessage(text);

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            var streamingContent = chatCompletionService.GetStreamingChatMessageContentsAsync(
                    history,
                    executionSettings: openAIPromptExecutionSettings,
                    kernel: kernel);

            var enumerator = streamingContent.GetAsyncEnumerator();

            StringBuilder resultBuilder = new StringBuilder();

            var stopwatch = Stopwatch.StartNew();
            var sentenceEnds = new char[] { '!', '?', '.', ';', '\n', '\r' };

            Debug.WriteLine($"Vor enumerator.MoveNextAsync()");

            while (await enumerator.MoveNextAsync())
            {                
                Debug.WriteLine("MoveNextAsync");

                var content = enumerator.Current;
                string textSnippet = content.ToString();
                if (textSnippet.Length == 0)
                {
                    continue;
                }
                
                Debug.WriteLine($"TextSnippet={textSnippet}");

                resultBuilder.Append(textSnippet);
            }


            Debug.WriteLine($"Nach enumerator.MoveNextAsync(), Elapsed: {stopwatch.ElapsedMilliseconds} ms");

            this.lastInteractionAt = DateTime.Now;
            string output = resultBuilder.ToString();
            Debug.WriteLine($"Ask: output={output}");
            await Speaker2.Say(output);

            history.AddAssistantMessage(resultBuilder.ToString());

            isListening = isActivated;
            this.lastInteractionAt = DateTime.Now;
        }
    }
}
