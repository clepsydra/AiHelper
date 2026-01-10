using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiHelper.Config;
using NAudio.Wave;
using Org.BouncyCastle.Tls.Crypto;

namespace AiHelper
{
    internal class VoiceCommandListener
    {
        private WaveInEvent waveIn;
        private MemoryStream gatheredWavData = new MemoryStream();
        private double silenceVolumneLimit;
        private int silenceWaitTimeOutInMs;

        private bool isListening = false;
        private bool silenceStarted;
        private DateTime lastInteractionAt;
        private DateTime silenceStartedAt;

        private Exception? exception;

        private VoiceCommandListener()
        {
            waveIn = new WaveInEvent();
            waveIn.DeviceNumber = 0;
            waveIn.WaveFormat = new WaveFormat(16000, 1);
            waveIn.BufferMilliseconds = 100;

            waveIn.DataAvailable += async (object? sender, WaveInEventArgs e) =>
            {
                try
                {
                if (!isListening || IgnoreAudio)
                {
                    return;
                }

                double maxVolume = AudioTools.GetMaxVolume(e);

                    if (maxVolume > silenceVolumneLimit)
                    {
                        gatheredWavData.Write(e.Buffer, 0, e.BytesRecorded);
                        silenceStarted = false;
                    }
                    else
                    {
                        if (!silenceStarted)
                        {
                            silenceStarted = true;
                            silenceStartedAt = DateTime.Now;
                        }
                        else
                        {
                            if (gatheredWavData.Length == 0)
                            {
                                // Nothing recorded yet
                                if (DateTime.Now.Subtract(lastInteractionAt).TotalSeconds > activationTimeoutInSeconds)
                                {
                                    Debug.WriteLine("No input until activation time out");
                                    isListening = false;
                                    waveIn.StopRecording();                                    
                                    await Speaker2.SayAndCache($"Ich habe seit {activationTimeoutInSeconds} Sekunden nichts gehört.", true);
                                    
                                }
                            }
                            else if (DateTime.Now.Subtract(silenceStartedAt).TotalMilliseconds > silenceWaitTimeOutInMs)
                            {
                                // Something recorded, but input ended
                                Debug.WriteLine($"Silence lasted for {silenceWaitTimeOutInMs} ms");
                                silenceStarted = false;

                                waveIn.StopRecording();
                                isListening = false;
                            }
                        }
                    }
                }

                catch (Exception ex)
                {
                    exception = ex;
                    isListening = false;
                }
            };
        }

        private const int activationTimeoutInSeconds = 60;

        public async Task<string> GetNextVoiceCommand(string language, string prompt)
        {
            this.exception = null;
            this.silenceVolumneLimit = ConfigProvider.Config?.SoundConfig.SilenceVolumeLimit ?? 0.005;

            this.silenceWaitTimeOutInMs = ConfigProvider.Config?.SoundConfig.SilenceWaitTimeInMs ?? 2000;

            this.silenceStarted = false;

            lastInteractionAt = DateTime.Now;
            silenceStartedAt = DateTime.Now;
            
            string? result = null;

            isListening = true;            
            waveIn.StartRecording();            

            while (isListening)
            {
                await Task.Delay(50);
            }

            if (exception != null)
            {
                throw exception;
            }

            //waveIn.Dispose();

            if (gatheredWavData.Length > 0)
            {
                var wavData = gatheredWavData.ToArray();

                var mp3Bytes = Mp3Converter.PcmBytesToMp3Bytes(wavData, new WaveFormat(16000, 1), 128);
                if (ConfigProvider.Config?.SoundConfig.PlayRecorded == true)
                {
                    await AudioTools.Play(mp3Bytes);
                }

                result = await SpeechRecognition.Recognize(mp3Bytes, language, prompt);
            }
            else
            {
                result = string.Empty;
            }

            gatheredWavData.Close();
            gatheredWavData.Dispose();
            
            gatheredWavData = new MemoryStream();

            return result;
        }

        private static VoiceCommandListener? instance;

        public bool IgnoreAudio { get; set; }

        public static VoiceCommandListener Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new VoiceCommandListener();
                }

                return instance;
            }
        }
    }
}
