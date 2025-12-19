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
    public class VoiceChat : ViewModelBase
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
        private readonly Action<string, bool> addToOutput;
        private Action<string> errorHandle = null;

        private int silenceWaitTimeOutInMs = 2000;
        private int minimumVoiceTimeInMs = 400;

        private MemoryStream gatheredWavData = new();

        public VoiceChat(Action<string, bool> addToOutput, Action<string> handleErrors)
        {
            this.addToOutput = addToOutput;

            errorHandle = handleErrors;

            waveIn = new WaveInEvent();
            waveIn.DeviceNumber = 0;
            waveIn.WaveFormat = new WaveFormat(16000, 1);
            waveIn.BufferMilliseconds = 100;

            this.SilenceVolumneLimit = ConfigProvider.Config?.SoundConfig.SilenceVolumeLimit ?? 0.005;

            this.silenceWaitTimeOutInMs = ConfigProvider.Config?.SoundConfig.SilenceWaitTimeInMs ?? 2000;
            this.minimumVoiceTimeInMs = ConfigProvider.Config?.SoundConfig.MinimumVoiceTimeInMs ?? 400;

            DateTime? voiceStartedAt = null;
            DateTime? voiceStoppedAt = null;

            waveIn.DataAvailable += async (object? sender, WaveInEventArgs e) =>
            {
                if (!isListening)
                {
                    return;
                }

                double maxVolume = AudioTools.GetMaxVolume(e);

                if (maxVolume > SilenceVolumneLimit)
                {
                    if (!isRecording)
                    {
                        voiceStartedAt = DateTime.Now;
                    }

                    isRecording = true;
                    silenceStarted = false;
                }
                else
                {
                    if (voiceStartedAt != null && DateTime.Now.Subtract(voiceStartedAt.Value).TotalMilliseconds < minimumVoiceTimeInMs)
                    {
                        Debug.WriteLine($"Voice wasn't long enough: {DateTime.Now.Subtract(voiceStartedAt.Value).TotalMilliseconds}ms");
                        isRecording = false;
                        gatheredWavData.Close();
                        gatheredWavData.Dispose();
                        gatheredWavData = new MemoryStream();
                    }

                    if (DateTime.Now.Subtract(lastInteractionAt).TotalSeconds > activationTimeoutInSeconds)
                    {
                        Debug.WriteLine("Activation ended!");
                        this.isListening = false;
                        this.isRecording = false;
                        gatheredWavData.Close();
                        gatheredWavData.Dispose();
                        gatheredWavData = new MemoryStream();
                        
                        await Speaker2.SayAndCache("Ich warte jetzt, bis Du wieder die Leertaste drückst.");

                        //Task.Run(WaitForActivation);
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
                        else if (DateTime.Now.Subtract(silenceStartedAt).TotalMilliseconds > this.silenceWaitTimeOutInMs)
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

        private bool isActivated = false;

        public bool IsActivated
        {
            get => this.isActivated;
            set
            {
                this.isActivated = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(this.IsDeactivated));
            }
        }

        public bool IsDeactivated => !this.IsActivated;

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
                    this.addToOutput(text, true);
                    await Ask(text);
                }
            });            
        }        

        public double SilenceVolumneLimit { get; set; }

        private async void Initialize()
        {
            var builder = Kernel.CreateBuilder();
            //string modelId = "gpt-5-nano";
            string modelId = "gpt-4o-mini";

            string apiKey = ConfigProvider.Config.OpenAiApiKey;

            builder.AddOpenAIChatCompletion(modelId, apiKey);

            kernel = builder.Build();
            PluginRegistrar.RegisterPlugins(kernel, this.CloseSession);

            AddSystemMessage();

            //Speaker2.Say("Ich warte jetzt, bis Du das Wort Computer sagst.");
            await Speaker2.SayAndCache("Ich warte jetzt, bis Du die leertaste drückst.", true);
            //Task.Run(WaitForActivation);

        }

        private void AddSystemMessage()
        {
            history.AddSystemMessage(@"Du hilfst alten Menschen.
Antworte eher kurz und mit einfachen Worten.
Und maximal 3 Sätzen am Stück.
Wenn Du mit einer Aktion fertig bist fragen den Benutzer ob er noch etwas möchte.
Wenn er nichts mehr möchte rufe das ClosePlugin auf.");
        }

        public async Task Activate()
        {
            this.silenceWaitTimeOutInMs = ConfigProvider.Config?.SoundConfig.SilenceWaitTimeInMs ?? 2000;
            this.minimumVoiceTimeInMs = ConfigProvider.Config?.SoundConfig.MinimumVoiceTimeInMs ?? 400;
            history.Clear();
            AddSystemMessage();
            IsActivated = true;
            lastInteractionAt = DateTime.Now;
            Debug.WriteLine("Activate: Activated!");
            this.addToOutput("Sprach Chat Aktiviert", false);
            await Speaker2.SayAndCache("Ich höre zu!", true);
            if (gatheredWavData != null)
            {
                Debug.WriteLine($"Activate: gatheredWavData.Length= {gatheredWavData.Length}");
            }
            else
            {
                Debug.WriteLine($"Activate: gatheredWavData is null");
            }
            isListening = true;
        }

        public async void Deactivate()
        {
            isListening = false;
            IsActivated = false;

            Debug.WriteLine("Deactivate: Deactivated!");
            this.addToOutput("Sprach Chat Deaktiviert", false);
            await Speaker2.SayAndCache("Ich höre jetzt nicht mehr zu. Drücke die Leertaste sobald ich wieder zuhören soll.");
        }

        private void WaitForActivation()
        {
            Debug.WriteLine("Waiting for activation...");
            this.isListening = false;
            this.isRecording = false;


            ActivatorByCodeword activator = new ActivatorByCodeword();
            activator.WaitForActivation(this.errorHandle);

            history.Clear();
            AddSystemMessage();

            isListening = true;
            IsActivated = true;
            lastInteractionAt = DateTime.Now;
            Debug.WriteLine("Activated!");
            Speaker2.Say("Ich höre zu!");
        }

        private void CloseSession()
        {
            Debug.WriteLine("Close Session started");
            this.isListening = false;
            this.isRecording = false;
            this.IsActivated = false;

            gatheredWavData.Close();
            gatheredWavData.Dispose();
            gatheredWavData = new MemoryStream();

            //Task.Run(async () =>
            //{
            //    this.WaitForActivation();
            //});
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
                this.lastInteractionAt = DateTime.Now;

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
            this.addToOutput(output, false);
            await Speaker2.Say(output, true);

            history.AddAssistantMessage(resultBuilder.ToString());

            isListening = IsActivated;
            this.lastInteractionAt = DateTime.Now;
        }
    }
}
