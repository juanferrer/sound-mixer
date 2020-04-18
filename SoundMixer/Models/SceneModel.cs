using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;
using Stylet;

namespace SoundMixer.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SceneModel : PropertyChangedBase
    {
        [JsonProperty]
        private string name;

        [JsonProperty]
        private BindableCollection<MoodModel> moods;

        [JsonProperty]
        private BindableCollection<SoundModel> sounds;

        public string Name
        {
            get { return this.name; }
            set { this.SetAndNotify(ref this.name, value); }
        }

        public BindableCollection<MoodModel> Moods
        {
            get { return this.moods; }
            set { this.SetAndNotify(ref this.moods, value); }
        }

        public BindableCollection<SoundModel> Sounds
        {
            get { return this.sounds; }
            set { this.SetAndNotify(ref this.sounds, value); }
        }

        public SceneModel(string name)
        {
            Name = name;
            Moods = new BindableCollection<MoodModel>();
            Sounds = new BindableCollection<SoundModel>();
        }
    }
}
