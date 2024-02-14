using System;

using Newtonsoft.Json;
using Stylet;

namespace SoundMixer.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SoundModel : PropertyChangedBase
    {
        [JsonProperty]
        private string filePath;

        [JsonProperty]
        private string name;

        [JsonProperty]
        private bool isURL;

        [JsonProperty]
        private Guid guid;

        [JsonProperty]
        private long duration;

        public string FilePath
        {
            get { return this.filePath; }
            set { this.SetAndNotify(ref this.filePath, value); }
        }

        public string Name
        {
            get { return this.name; }
            set { this.SetAndNotify(ref this.name, value); }
        }

        public bool IsURL
        {
            get { return this.isURL; }
            set { this.SetAndNotify(ref this.isURL, value); }
        }

        public Guid GUID
        {
            get { return this.guid; }
            private set { this.SetAndNotify(ref this.guid, value); }
        }

        public long Duration
        {
            get { return this.duration; }
            private set { this.SetAndNotify(ref this.duration, value); }
        }

        public SoundModel(string name, string filePath, long duration, Guid guid, bool isURL = false)
        {
            Name = name;
            FilePath = filePath;
            IsURL = isURL;
            GUID = guid == Guid.Empty ? Guid.NewGuid() : guid;
            Duration = duration;
        }
    }
}
