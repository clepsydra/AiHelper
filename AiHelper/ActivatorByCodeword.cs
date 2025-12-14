using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Text;
using System.Threading.Tasks;

namespace AiHelper
{
    internal class ActivatorByCodeword
    {
        private SpeechRecognitionEngine _recognizer;
        private SemaphoreSlim _waitHandle = new SemaphoreSlim(0, 1);
        private const string Codeword = "Computer";

        public void WaitForActivation()
        {
            try
            {
                _recognizer = new SpeechRecognitionEngine();

                Choices choices = new Choices(Codeword);
                GrammarBuilder gb = new GrammarBuilder(choices);
                Grammar grammar = new Grammar(gb);

                _recognizer.LoadGrammar(grammar);

                _recognizer.SpeechRecognized += Recognizer_SpeechRecognized;
                _recognizer.RecognizeCompleted += Recognizer_RecognizeCompleted;

                _recognizer.SetInputToDefaultAudioDevice();

                _recognizer.RecognizeAsync(RecognizeMode.Multiple);

                _waitHandle.Wait();
                
                _recognizer.RecognizeAsyncStop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ein Fehler ist aufgetreten: {ex.Message}");
            }
        }

        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result != null && e.Result.Text != null)
            {
                string recognizedText = e.Result.Text;
                float confidence = e.Result.Confidence;

                Console.WriteLine($"Erkannt: '{recognizedText}' (Konfidenz: {confidence:P})");

                if (recognizedText.Equals(Codeword, StringComparison.OrdinalIgnoreCase))
                {
                    if (confidence >= 0.7)
                    {
                        Console.WriteLine("\n[✅ CODÉWORT ERKANNT! Starte Aktion...]");

                        _waitHandle.Release();
                    }
                    else
                    {
                        Console.WriteLine("[❌ Konfidenz zu niedrig. Ignoriere Erkennung.]");
                    }
                }
            }
        }

        private void Recognizer_RecognizeCompleted(object sender, RecognizeCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Console.WriteLine($"Fehler bei der Erkennung: {e.Error.Message}");
            }
            else if (e.Cancelled)
            {
                Console.WriteLine("Erkennung wurde abgebrochen.");
            }         
        }
    }
}
