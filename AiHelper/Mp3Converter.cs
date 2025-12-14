using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Lame;

namespace AiHelper
{
    internal class Mp3Converter
    {
        public static byte[] PcmBytesToMp3Bytes(byte[] pcmBytes, WaveFormat sourceFormat, int mp3BitRate = 128)
        {
            using (var pcmStream = new MemoryStream(pcmBytes))
            {
                using (var pcmProvider = new RawSourceWaveStream(pcmStream, sourceFormat))
                {
                    using (var mp3Stream = new MemoryStream())
                    {
                        using (var mp3Writer = new LameMP3FileWriter(mp3Stream, pcmProvider.WaveFormat, mp3BitRate))
                        {
                            pcmProvider.CopyTo(mp3Writer);
                        }
         
                        mp3Stream.Position = 0;

                        return mp3Stream.ToArray();
                    }
                }
            }
        }
    }
}
