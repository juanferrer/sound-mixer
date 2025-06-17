using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundMixer.Core.Objects
{
    public class Mood
    {
        public string Name { get; set; }
        public Guid Guid { get; set; }
        public List<SoundProperties> SoundProperties {get;set;}

        public Mood(string name)
        {
            Name = name;
            SoundProperties = new List<SoundProperties>();
        }
    }
}
