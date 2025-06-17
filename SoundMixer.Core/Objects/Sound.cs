using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SoundMixer.Core.Objects
{
    public class Sound
    {
        public string Name { get; set; }
        public List<string> FilePaths { get; set; }
        public bool IsURL { get; set; }
        public Guid Guid { get; set; }
        public long Duration { get; set; }
        public bool IsList { get; set; }

        [JsonIgnore]
        private int soundIndex { get; set; }
        [JsonIgnore]
        public int SoundIndex
        {
            get
            {
                if (soundIndex >= FilePaths.Count)
                {
                    soundIndex = 0;
                }

                return soundIndex++;
            }
            private set { }
        }

        [JsonIgnore]
        public string FilePath => FilePaths[SoundIndex];

        public Sound(string name)
        {
            Name = name;
            FilePaths = new List<string>();
        }
    }
}
