using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SoundMixer.Core.Interfaces;
using SoundMixer.Core.Objects;

using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace SoundMixer.Core.NAudioPlayer
{
    public class NAudioPlayer : ISoundPlayer, IDisposable
    {

        private IWavePlayer OutputDevice;
        private MixingSampleProvider Mixer;
        private Dictionary<Guid, CachedSound> SoundCache;
        private Dictionary<Guid, CachedSoundSampleProvider> SoundProviders;

        public NAudioPlayer(int sampleRate = 44100, int channelCount = 2)
        {
            OutputDevice = new WaveOutEvent();
            Mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount))
            {
                ReadFully = true
            };
            SoundCache = new Dictionary<Guid, CachedSound>();
            SoundProviders = new Dictionary<Guid, CachedSoundSampleProvider>();

            OutputDevice.Init(Mixer);
        }

        public void LoadMood(Mood? mood)
        {
            if (mood == null) return;

            foreach (var sound in mood.SoundProperties)
            {
                CachedSound cachedSound;
                if (SoundCache.ContainsKey(sound.Sound.Guid))
                {
                    cachedSound = SoundCache[sound.Sound.Guid];
                }
                else
                {
                    // Don't have this one, load from file and cache
                    cachedSound = new CachedSound(sound.Sound.FilePath, Mixer.WaveFormat);
                    SoundCache.Add(sound.Sound.Guid, cachedSound);
                }

                // Set all the initial properties for the sample provider
                var sampleProvider = new CachedSoundSampleProvider(SoundCache[sound.Sound.Guid])
                {
                    IsLoop = sound.IsLoop,
                    Volume = sound.Volume
                };

                // Store sample provider for future modifications
                if (SoundProviders.ContainsKey(sound.Guid))
                {
                    SoundProviders[sound.Guid] = sampleProvider;
                }
                else
                { 
                    SoundProviders.Add(sound.Guid, sampleProvider);
                }

                Mixer.AddMixerInput(sampleProvider);
            }
        }

        public void Play()
        {
            OutputDevice.Play();
        }

        public void Pause()
        {
            OutputDevice.Pause();
        }

        public void Stop()
        {
            OutputDevice.Stop();
        }

        public List<string> GetOutputDevices()
        {
            return new List<string> { };
        }
        
        public void SelectOutputDevice(string outputDevice)
        {
            
        }
        public void Dispose()
        {
            OutputDevice?.Dispose();
        }

        #region Property Updates

        public void UpdateSoundVolume(Guid soundGuid, decimal volume, int fadeMS = 3000)
        {
            if (SoundProviders.TryGetValue(soundGuid, out var sampleProvider))
            {
                sampleProvider.UpdateSoundVolume(volume, fadeMS);
            }
        }

        public void SetMuted(Guid soundGuid, bool muted)
        {
            if (SoundProviders.TryGetValue(soundGuid, out var sampleProvider))
            {
                sampleProvider.SetIsMuted(muted);
            }
        }

        #endregion

    }
}
