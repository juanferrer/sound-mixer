using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SoundMixer.Core.Objects
{
    public class SoundProperties
    {
        [JsonIgnore]
        public Guid Guid { get; set; }
        [JsonIgnore]
        public Sound Sound { get; set; }
        public Guid SoundGuid { get; set; }
        /// <summary>
        /// 0-1
        /// </summary>
        public decimal Volume { get; set; }
        public bool IsLoop { get; set; }
        public bool IsDelayed { get; set; }
        public long DelayTime { get; set; }
        public bool UseRandomeDelay { get; set; }
        public long MinDelay { get; set; }
        public long MaxDelay { get; set; }
        public bool IsMuted { get; set; }
        public bool IsSolo { get; set; }

        public SoundProperties(Sound sound)
        {
            Sound = sound;

            Volume = 0.5m;
            MinDelay = 0;
            MaxDelay = 60000;
        }
    }
}
