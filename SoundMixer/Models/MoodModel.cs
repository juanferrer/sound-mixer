using Newtonsoft.Json;
using Stylet;

namespace SoundMixer.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class MoodModel : PropertyChangedBase
    {
        [JsonProperty]
        private BindableCollection<SoundPropertyModel> soundProperties;
        [JsonProperty]
        private string name;

        public string Name
        {
            get { return this.name; }
            set { this.SetAndNotify(ref this.name, value); }
        }

        [JsonIgnore]
        public BindableCollection<SoundPropertyModel> SoundProperties
        {
            get { return this.soundProperties; }
            set { this.SetAndNotify(ref this.soundProperties, value); }
        }

        public MoodModel(string name)
        {
            Name = name;
            SoundProperties = new BindableCollection<SoundPropertyModel>();
        }
    }
}
