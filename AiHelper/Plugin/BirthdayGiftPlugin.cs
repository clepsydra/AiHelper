using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using NAudio.Wave;

namespace AiHelper.Plugin
{
    public class BirthdayGiftPlugin
    {
        private bool cancelReceived = false;
        private readonly Action closeSession;
        private bool isPlaying = false;

        public BirthdayGiftPlugin(ICancelRegistrar cancelRegistrar, Action closeSession)
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
The user received a birthday gift: Several self recorded music pieces.
When the user asks for his birthday gift use this function. 
The function returns the number of music pieces. The user can then be asked for a music piece by number to play.")]
        public async Task<int> Explanation()
        {
            //await Speaker2.SayAndCache("Dein Geschenk ist sind ein paar nette Liedchen.", true);

            //string fileName = Path.Combine(BirthsdayGiftFolder, "Introduction.mp3");
            //if (!File.Exists(fileName))
            //{
            //    throw new Exception("Datei nicht gefunden");
            //}

            //await PlayAudioFile(fileName);

            int numberOfPieces = GetNumberOfPieces();
            return numberOfPieces;
        }

        [KernelFunction]
        [Description(@"
The user received a borthday gift: Several pieces of self recorded music.
To bring the Birthday gift to the user this function allows to play the music pieces.
When it is called with a music piece number it will play the mp3 file for that music piece on a new thread.
If the user does not provide the number call this function with the number 1 as parameter.
This functions ends the input by the user.
After calling this function you MUST therefore not ask the user whether he wants to do something else.
Parameters:
-  ChapterNumber")]
        public async Task PlayByNumber(int pieceNumber)
        {
            cancelReceived = false;
            isPlaying = true;

            var files = GetAllFiles();
            if (files.Count < pieceNumber)
            {
                throw new Exception("Es sind nicht so viele Dateien");
            }

            string fullFileName = files[pieceNumber - 1];
            string pureFileName = Path.GetFileNameWithoutExtension(fullFileName);
            if (!File.Exists(fullFileName))
            {
                throw new Exception($"Datei {Path.GetFileName(fullFileName)} nicht gefunden");
            }

            await Speaker2.SayAndCache($"Ich spiele gleich das Stück \"{pureFileName}\" ab. Wenn Du die Wiedergabe beenden möchtest drücke die Leertaste.", true);

            this.closeSession();

            _ = Task.Run(async () =>
            {
                await Task.Delay(8000);
                bool cancelled = await this.PlayAudioFile(fullFileName);
                if (cancelled)
                {
                    await Speaker2.SayAndCache($"Wiedergabe abgebrochen. Drücke die Leertaste, wenn ich wieder zuhören soll.", true);
                    //SaveLastPosition(chapterNumber, position);
                }
                else
                {
                    await Speaker2.SayAndCache($"Das Stück \"{pureFileName}\" ist nun zuende.", true);

                    if (pieceNumber < GetNumberOfPieces())
                    {
                        _ = Task.Run(async () => await PlayByNumber(pieceNumber + 1));
                        return;
                    }

                    await Speaker2.SayAndCache($"Alle Stücke abgespielt. Drücke die Leertaste, wenn ich wieder zuhören soll.", true);
                }

                isPlaying = false;
            });
        }

        [KernelFunction]
        [Description(@"
The user received a borthday gift: Several pieces of self recorded music.
To bring the Birthday gift to the user this function allows to play the music pieces.
This function can be used when the user gives a text input about the piece to play. It will try to find the corresponding file and play it on a new thread.
When it is called with a music piece number it will play the mp3 file for that music piece on a new thread.
This functions ends the input by the user.
After calling this function you MUST therefore not ask the user whether he wants to do something else.
Parameters:
-  text")]
        public async Task PlayByName(string text)
        {
            var files = GetAllFiles();
            string? matchingFileName = files.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f).Contains(text, StringComparison.OrdinalIgnoreCase));
            if (matchingFileName == null)
            {
                throw new Exception("Eine solche Datei wurde nicht gefunden");
            }

            await PlayByNumber(files.ToList().IndexOf(matchingFileName) + 1);
        }

        private async Task<bool> PlayAudioFile(string fileName)
        {
            using var stream = File.OpenRead(fileName);            
#pragma warning disable CA1416 // Validate platform compatibility
            var reader = new Mp3FileReader(stream);
#pragma warning restore CA1416 // Validate platform compatibility
            var waveOut = new WaveOut();
            waveOut.Init(reader);
            waveOut.Play();

            bool cancelled = false;

            while (!cancelReceived && waveOut.PlaybackState == PlaybackState.Playing)
            {
                await Task.Delay(100);
            }

            if (cancelReceived && waveOut.PlaybackState == PlaybackState.Playing)
            {
                waveOut.Stop();
                cancelled = true;             
            }

            cancelReceived = false;

            return cancelled;
        }

        private IReadOnlyList<string> GetAllFiles()
        {
            var files = Directory.GetFiles(BirthsdayGiftFolder, "*.mp3");
            return files;
        }

        [KernelFunction]
        [Description(@"
The user received a christmas gift: Self recorded music pieces.
This function give back the number of music pieces.")]
        public int GetNumberOfPieces()
        {
            var files = Directory.GetFiles(BirthsdayGiftFolder, "*.mp3");
            return files.Length;
        }

        private static string BirthsdayGiftFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "BirthdayGift");

        public static bool IsAvailable => Directory.Exists(BirthsdayGiftFolder);
    }
}
