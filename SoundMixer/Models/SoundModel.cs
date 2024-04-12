using System;
using System.Collections.Generic;
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
        private BindableCollection<string> filePaths;

        [JsonProperty]
        private string name;

        [JsonProperty]
        private bool isURL;

        [JsonProperty]
        private Guid guid;

        [JsonProperty]
        private long duration;

        [JsonProperty]
        private bool isList;

        private int soundIndex = 0;

        public int SoundIndex
        {
            get
            {
                if (soundIndex >= filePaths.Count)
                {
                    soundIndex = 0;
                }

                return soundIndex++; // Return index and then increment it, so we need to check before using
            }
            private set { this.SetAndNotify(ref this.soundIndex, value); }
        }

        public string FilePath
        {
            get { return this.filePaths[SoundIndex]; }
        }

        public BindableCollection<string> FilePaths
        {
            get { return this.filePaths; }
            private set { this.SetAndNotify(ref this.filePaths, value); }
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

        public bool IsList
        {
            get { return this.isList; }
            set { this.SetAndNotify(ref this.isList, value); }
        }

        public SoundModel(string name, string filePath, long duration, Guid guid, bool isURL = false)
        {
            Name = name;
            FilePaths = new BindableCollection<string> { filePath };
            IsURL = isURL;
            GUID = guid == Guid.Empty ? Guid.NewGuid() : guid;
            Duration = duration;

        }
    }
}
