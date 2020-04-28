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
        private Guid guid;

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

        public Guid GUID
        {
            get { return this.guid; }
            private set { this.SetAndNotify(ref this.guid, value); }
        }

        public SoundModel(string name, string filePath)
        {
            Name = name;
            FilePath = filePath;
            GUID = Guid.NewGuid();
        }
    }
}
