using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiHelper.Config.Models
{
    public class SoundConfig
    {
        public double SilenceVolumeLimit { get; set; } = 0.05;

        public int SilenceWaitTimeInMs { get; set; } = 2000;

        public int MinimumVoiceTimeInMs { get; set; } = 600;

        public SoundConfig Clone()
        {
            return new SoundConfig
            {
                SilenceVolumeLimit = SilenceVolumeLimit,
                SilenceWaitTimeInMs = SilenceWaitTimeInMs
            };
        }

        public string Voice { get; set; } = "Shimmer";
    }
}
