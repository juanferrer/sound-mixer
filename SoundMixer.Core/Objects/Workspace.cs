using SoundMixer.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SoundMixer.Core.Objects
{
    public class Workspace
    {
        public Scene? ActiveScene { get; private set; }
        public Mood? ActiveMood { get; private set; }
        public List<Scene> Scenes { get; set; } = new List<Scene>();
        private ISoundPlayer? Player { get; set; }

        public void AddPlayer(ISoundPlayer player)
        {
            Player = player;
        }

        public void ValidateWorkspace()
        {
            foreach (var scene in Scenes)
            {
                if (scene.Moods.Count > 0)
                {
                    for (var i = 0; i < scene.Moods[0]?.SoundProperties.Count; i++)
                    {
                        // Since all moods contain the same sounds, we only need to find the matching sound once
                        foreach (var mood in scene.Moods)
                        {
                            var sound = scene.Sounds.Where(o => o.Guid == scene.Moods[0].SoundProperties[i].SoundGuid).FirstOrDefault();

                            if (sound != null)
                            {
                                // TODO: Can't find the sound
                            }

                            mood.SoundProperties[i].Sound = sound;
                            mood.SoundProperties[i].Guid = Guid.NewGuid();
                        }
                    }
                }
            }
        }

        public void LoadMood(Guid moodGuid)
        {
            ActiveScene = Scenes.Where(s => s.Moods.Any(m => m.Guid == moodGuid)).FirstOrDefault();
            ActiveMood = ActiveScene?.Moods?.Where(m => m.Guid == moodGuid).FirstOrDefault();
            Player?.LoadMood(ActiveMood);
        }

        public void Play() => Player?.Play();

        public void Pause() => Player?.Pause();

        public void Stop() => Player?.Stop();

        #region Property Updates

        public void UpdateSoundVolume(Guid guid, decimal volume) => Player?.UpdateSoundVolume(guid, volume);
        public void SetMuted(Guid guid, bool muted) => Player?.SetMuted(guid, muted);

        #endregion
    }
}
