using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents.Serialization;
using Emgu.CV.Cuda;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using MimeKit.Tnef;
using NAudio.Wave;
using Newtonsoft.Json;

namespace AiHelper.Plugin
{
    public class ChristmasGiftPlugin
    {
        private bool cancelReceived = false;
        private readonly Action closeSession;
        private bool isPlaying = false;

        public ChristmasGiftPlugin(ICancelRegistrar cancelRegistrar, Action closeSession)
        {
            cancelRegistrar.Cancel += CancelRegistrar_Cancel;
            this.closeSession = closeSession;
        }

        private void CancelRegistrar_Cancel(object? sender, CancelEventArgs e)
        {
            if (!isPlaying)
            {
                return;
            }

            cancelReceived = true;
            e.IsHandled = true;
        }

        [KernelFunction]
        [Description(@"
The user received a christmas gift: It is an audio book which is self recorded.
When the user asks for his christmas gift use this function. It will explain the gift to the user.
The function returns the number of chapters. The user can then be asked for a chapter number to play.")]
        public async Task<int> Explanation()
        {
            await Speaker2.SayAndCache("Dein Geschenk ist ein Hörbuch. Ich spiele jetzt die Einleitung ab.", true);

            string fileName = Path.Combine(ChristmasGiftFolder, "Introduction.mp3");
            if (!File.Exists(fileName))
            {
                throw new Exception("Datei nicht gefunden");
            }

            await PlayAudioFile(fileName);

            int numberOfChapters = await GetNumberOfChapters();
            return numberOfChapters;
        }

        [KernelFunction]
        [Description(@"
The user received a christmas gift: It is an audio book which is self recorded.
To bring the Christmas gift to the user this function allows to play chapters of a self recorded audio book.
When it is called with a chapter number it will play the mp3 file for that chapter on a new thread.
If the user does not provide the chapter number ask him whether he wants to continue.
If he wants to continue just start the this function with -1 as parameter.
And if he wants to start at the beginning call the function with 1 as parameter.
This functions ends the input by the user.
After calling this function you MUST therefore not ask the user whether he wants to do something else.
Parameters:
-  ChapterNumber")]
        public async Task Play(int chapterNumber)
        {
            cancelReceived = false;
            isPlaying = true;

            long startPosition = 0;

            if (chapterNumber == -1)
            {
                var lastPlayPosition = GetLastPosition();
                chapterNumber = lastPlayPosition.CurrentChapter;
                startPosition = lastPlayPosition.CurrentPlayPosition - 20000;

                if (startPosition < 0)
                {
                    startPosition = 0;
                }

                Debug.WriteLine($"ChapterNumber == -1. Continuing with Chapter {chapterNumber}, Position: {startPosition}");
            }

            string fileName = Path.Combine(ChristmasGiftFolder, chapterNumber == 0 ? "Introduction.mp3" : $"Chapter {chapterNumber}.mp3");
            if (!File.Exists(fileName))
            {
                throw new Exception($"Datei {fileName} nicht gefunden");
            }

            await Speaker2.SayAndCache($"Ich spiele gleich Kapitel {chapterNumber}. Wenn Du die Wiedergabe beenden möchtest drücke die Leertaste.", true);

            this.closeSession();

            SaveLastPosition(chapterNumber, 0);

            _ = Task.Run(async () =>
            {
                await Task.Delay(8000);
                (bool cancelled, long position) = await this.PlayAudioFile(fileName, startPosition);
                if (cancelled)
                {
                    await Speaker2.SayAndCache($"Wiedergabe abgebrochen.", true);
                    SaveLastPosition(chapterNumber, position);
                }
                else
                {
                    await Speaker2.SayAndCache($"Kapitel {chapterNumber} ist nun zuende.", true);

                    if (chapterNumber < await GetNumberOfChapters())
                    {
                        _ = Task.Run(async () => await Play(chapterNumber + 1));
                        return;
                    }

                    await Speaker2.SayAndCache($"Das Buch ist jetzt zuende. Drücke die Leertaste, wenn ich wieder zuhören soll.", true);
                }

                isPlaying = false;
            });
        }

        private async Task<(bool, long)> PlayAudioFile(string fileName, long startPosition = 0)
        {
            using var stream = File.OpenRead(fileName);
            stream.Position = startPosition;
            var reader = new Mp3FileReader(stream);
            var waveOut = new WaveOut();
            waveOut.Init(reader);
            waveOut.Play();

            bool cancelled = false;

            while (!cancelReceived && waveOut.PlaybackState == PlaybackState.Playing)
            {
                await Task.Delay(100);
            }

            long streamPosition = 0;

            if (cancelReceived && waveOut.PlaybackState == PlaybackState.Playing)
            {
                waveOut.Stop();
                cancelled = true;
                streamPosition = stream.Position;
            }

            cancelReceived = false;

            return (cancelled, streamPosition);
        }

        [KernelFunction]
        [Description(@"
The user received a christmas gift: It is an audio book which is self recorded.
As part of a Christmas gift an audio book has been recorded. This function give back the number of chapters.")]
        public async Task<int> GetNumberOfChapters()
        {
            var files = Directory.GetFiles(ChristmasGiftFolder, "Chapter*.mp3");
            return files.Length;
        }

        private static string LastPlayPositionFileName => Path.Combine(ChristmasGiftFolder, "lastPlaysition.json");

        private static string ChristmasGiftFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ChristmasGift");

        private ChristmasGiftPluginStorage GetLastPosition()
        {
            var fileName = LastPlayPositionFileName;

            var startPosition = new ChristmasGiftPluginStorage
            {
                CurrentChapter = 1,
                CurrentPlayPosition = 0
            };

            if (!File.Exists(fileName))
            {
                return startPosition;
            }

            try
            {
                string json = File.ReadAllText(fileName);
                var lastPosition = JsonConvert.DeserializeObject<ChristmasGiftPluginStorage>(json);

                if (lastPosition == null)
                {
                    return startPosition;
                }


                return lastPosition;
            }

            catch (Exception ex)
            {
                return startPosition;
            }
        }

        private void SaveLastPosition(int chapterNumber, long position)
        {
            var lastPosition = new ChristmasGiftPluginStorage
            {
                CurrentChapter = chapterNumber,
                CurrentPlayPosition = position
            };

            try
            {
                string json = JsonConvert.SerializeObject(lastPosition);

                File.WriteAllText(LastPlayPositionFileName, json);
            }
            catch (Exception ex)
            {
                // Handle this?
            }

        }
    }
}
