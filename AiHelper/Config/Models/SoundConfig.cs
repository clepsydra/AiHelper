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

        public SoundConfig Clone()
        {
            return new SoundConfig
            {
                SilenceVolumeLimit = SilenceVolumeLimit
            };
        }
    }
}
