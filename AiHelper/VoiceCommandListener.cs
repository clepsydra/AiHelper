using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiHelper.Config;
using NAudio.Wave;

namespace AiHelper
{
    internal class VoiceCommandListener
    {
        private const int activationTimeoutInSeconds = 60;

        public async Task<string> GetNextVoiceCommand(string language, string prompt)
        {
            var waveIn = new WaveInEvent();
            waveIn.DeviceNumber = 0;
            waveIn.WaveFormat = new WaveFormat(16000, 1);
            waveIn.BufferMilliseconds = 100;

            var silenceVolumneLimit = ConfigProvider.Config?.SoundConfig.SilenceVolumeLimit ?? 0.005;

            var silenceWaitTimeOutInMs = ConfigProvider.Config?.SoundConfig.SilenceWaitTimeInMs ?? 2000;
            var minimumVoiceTimeInMs = ConfigProvider.Config?.SoundConfig.MinimumVoiceTimeInMs ?? 400;

            DateTime? voiceStartedAt = null;

            bool isRecording = false;
            bool silenceStarted = false;

            DateTime lastInteractionAt = DateTime.Now;
            DateTime silenceStartedAt = DateTime.Now;

            MemoryStream gatheredWavData = new MemoryStream();
            string? result = null;

            bool isListening = true;

            waveIn.DataAvailable += async (object? sender, WaveInEventArgs e) =>
            {
                if (!isListening)
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
                                await Speaker2.SayAndCache($"Ich habe seit {activationTimeoutInSeconds} Sekunden nichts gehört.", true);
                                isListening = false;
                                waveIn.StopRecording();
                            }
                        }
                        else if (DateTime.Now.Subtract(silenceStartedAt).TotalMilliseconds > silenceWaitTimeOutInMs)
                        {
                            // Something recorded, but input ended
                            Debug.WriteLine($"Silence lasted for {silenceWaitTimeOutInMs} ms");
                            isRecording = false;
                            silenceStarted = false;

                            waveIn.StopRecording();
                            isListening = false;
                        }
                    }

                }
            };

            waveIn.StartRecording();

            while (isListening)
            {
                await Task.Delay(50);
            }

            waveIn.Dispose();

            if (gatheredWavData.Length > 0)
            {
                var wavData = gatheredWavData.ToArray();

                var mp3Bytes = Mp3Converter.PcmBytesToMp3Bytes(wavData, new WaveFormat(16000, 1), 128);
                result = await SpeechRecognition.Recognize(mp3Bytes, language, prompt);
            }
            else
            {
                result = string.Empty;
            }

            gatheredWavData.Close();
            gatheredWavData.Dispose();

            return result;
        }
    }
}
