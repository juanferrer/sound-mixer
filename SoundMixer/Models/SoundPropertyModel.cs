using System;

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
        private bool useRandomDelay;

        [JsonProperty]
        private int minDelay;

        [JsonProperty]
        private int maxDelay;

        [JsonProperty]
        private Guid guid;


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

        public bool UseRandomDelay
        {
            get { return this.useRandomDelay; }
            set { this.SetAndNotify(ref this.useRandomDelay, value); }
        }

        public int DelayTime
        {
            get { return this.delayTime; }
            set { this.SetAndNotify(ref this.delayTime, value); }
        }

        public int MinDelay
        {
            get { return this.minDelay; }
            set { this.SetAndNotify(ref this.minDelay, value); }
        }

        public int MaxDelay
        {
            get { return this.maxDelay; }
            set { this.SetAndNotify(ref this.maxDelay, value); }
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
            MinDelay = 0;
            MaxDelay = 60000; // A minute
        }
    }
}
