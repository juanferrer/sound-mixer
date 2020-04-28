using Newtonsoft.Json;
using Stylet;

namespace SoundMixer.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class WorkspaceModel : PropertyChangedBase
    {
        [JsonProperty]
        private BindableCollection<SceneModel> scenes;

        [JsonProperty]
        private double masterVolume;

        public BindableCollection<SceneModel> Scenes
        {
            get { return this.scenes; }
            set { this.SetAndNotify(ref this.scenes, value); }
        }

        public double MasterVolume
        {
            get { return this.masterVolume; }
            set { this.SetAndNotify(ref this.masterVolume, value); }
        }

        public WorkspaceModel()
        {
            Scenes = new BindableCollection<SceneModel>();
        }
    }
}
