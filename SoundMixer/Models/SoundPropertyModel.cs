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
        private bool isLoop;

        [JsonProperty]
        private bool isDelayed;

        [JsonProperty]
        private int delayTime;

        [JsonProperty]
        private Guid guid;

        /*private double timeInterval;

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

        public bool IsLoop
        {
            get { return this.isLoop; }
            set { this.SetAndNotify(ref this.isLoop, value); }
        }

        public bool IsDelayed
        {
            get { return this.isDelayed; }
            set { this.SetAndNotify(ref this.isDelayed, value); }
        }

        public int DelayTime
        {
            get { return this.delayTime; }
            set { this.SetAndNotify(ref this.delayTime, value); }
        }

        public Guid GUID
        {
            get { return this.guid; }
            private set { this.SetAndNotify(ref this.guid, value); }
        }

        public SoundPropertyModel(double volume, Guid guid)
        {
            Volume = volume;
            GUID = guid;
            IsLoop = true;
            IsDelayed = false;
        }
    }
}
