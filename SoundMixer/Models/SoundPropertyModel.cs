using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;
using Stylet;

namespace SoundMixer.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SoundPropertyModel : PropertyChangedBase
    {
        [JsonProperty]
        private double volume;

        private SoundModel sound;

        [JsonProperty]
        private Guid guid;

        /*private bool single;

        private double timeInterval;

        private bool randomInterval;*/

        public double Volume
        {
            get { return this.volume; }
            set { this.SetAndNotify(ref this.volume, value); }
        }

        public SoundModel Sound
        {
            get { return this.sound; }
            set { this.SetAndNotify(ref this.sound, value); }
        }


        public Guid GUID
        {
            get { return this.guid; }
            private set { this.SetAndNotify(ref this.guid, value); }
        }

        public SoundPropertyModel(double volume, SoundModel sound)
        {
            Volume = volume;
            Sound = sound;
            GUID = sound.GUID;
        }
    }
}
