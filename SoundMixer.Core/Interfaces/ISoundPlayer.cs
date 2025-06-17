using SoundMixer.Core.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundMixer.Core.Interfaces
{
    public interface ISoundPlayer
    {
        public void LoadMood(Mood? mood);
        public void Play();
        public void Pause();
        public void Stop();

        public List<string> GetOutputDevices();
        public void SelectOutputDevice(string outputDevice);

        #region Property Updates

        public void UpdateSoundVolume(Guid guid, decimal volume, int fadeMS = 3000);
        public void SetMuted(Guid guid, bool muted);

        #endregion

        //public bool IsPlaying { get; protected set; }
        //public bool IsLoaded { get; protected set; }

    }
}
