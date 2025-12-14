using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace AiHelper
{
    internal class AudioTools
    {
        public static double GetMaxVolume(WaveInEventArgs e)
        {
            var waveBuffer = new WaveBuffer(e.Buffer);
            int bytesRecorded = e.BytesRecorded;
            int samplesRecorded = bytesRecorded / 2;

            double maxVolume = 0.0;
            for (int i = 0; i < samplesRecorded; i++)
            {
                // Sample als 16-Bit-Integer (short) lesen
                short sample = waveBuffer.ShortBuffer[i];

                // Den Betrag des Samples normalisieren (auf 1.0 basierend auf max. 32767)
                double absoluteNormalizedSample = Math.Abs((double)sample / short.MaxValue);

                if (absoluteNormalizedSample > maxVolume)
                    maxVolume = absoluteNormalizedSample;
            }

            return maxVolume;
        }
    }
}
