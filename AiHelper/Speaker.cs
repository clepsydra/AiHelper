using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Synthesis;

namespace AiHelper
{
    class Speaker
    {
        private static SpeechSynthesizer synthesizer = new SpeechSynthesizer();

        static Speaker()
        {
            synthesizer.SetOutputToDefaultAudioDevice();
            var voice = synthesizer.GetInstalledVoices().FirstOrDefault(v => v.VoiceInfo.Gender == VoiceGender.Male && v.VoiceInfo.Culture.Name.StartsWith("de", StringComparison.OrdinalIgnoreCase));
            voice ??= synthesizer.GetInstalledVoices().FirstOrDefault(v => v.VoiceInfo.Culture.Name.StartsWith("de", StringComparison.OrdinalIgnoreCase));

            if (voice != null)
            {
                synthesizer.SelectVoice(voice.VoiceInfo.Name);
            }
        }

        public static Action<string>? ErrorHandler { get; set; }

        public static async Task Say(string text)
        {
            try
            {
                await Task.Run(() => synthesizer.Speak(text));
            }
            catch (Exception ex)
            {
                ErrorHandler?.Invoke($"Speak Fehler: {ex.Message}");
            }
        }
    }
}
