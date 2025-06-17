using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;

namespace SoundMixer.Core.NAudioPlayer
{
    /// <summary>
    /// <para>Sample provider for <see cref="CacheSound"/>.</para>
    /// <para>Originally from <a href="https://www.markheath.net/post/fire-and-forget-audio-playback-with"/></para>
    /// </summary>
    class CachedSoundSampleProvider : ISampleProvider
    {
        private readonly CachedSound CachedSound;
        private long Position;

        public bool IsLoop { get; set; }
        public bool IsDelayed { get; set; }
        public long DelayTime { get; set; }
        public bool UseRandomeDelay { get; set; }
        public long MinDelay { get; set; }
        public long MaxDelay { get; set; }
        public bool IsMuted { get; set; }
        public bool IsSolo { get; set; }

        // Volume
        public decimal Volume { get; set; }
        public decimal VolumeDifference { get; set; }
        public decimal VolumeStep { get; set; }
        public int VolumeFadeSampleCount { get; set; }

        public CachedSoundSampleProvider(CachedSound cachedSound)
        {
            CachedSound = cachedSound;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            // Muted
            if (IsMuted) return count;

            var availableSamples = CachedSound.AudioData.Length - Position;
            var samplesToCopy = Math.Min(availableSamples, count);
            var copiedSamples = 0;

            // Loop
            if (IsLoop && availableSamples < count)
            {
                // We're reaching the end of the sound, copy however many samples are left in the data
                // and start copying from the beginning again
                Array.Copy(CachedSound.AudioData, Position, buffer, offset, availableSamples);
                Position = count - availableSamples;
                Array.Copy(CachedSound.AudioData, 0, buffer, offset, Position);
                copiedSamples = count;
            }
            else
            {
                Array.Copy(CachedSound.AudioData, Position, buffer, offset, samplesToCopy);
                Position += samplesToCopy;
                copiedSamples = (int)samplesToCopy;
                
            }

            // Multiply the buffer by the volume level
            if (Volume != 1m || VolumeDifference > 0)
            {
                if (VolumeDifference != 0m)
                {
                    for (int sample = 0; sample < copiedSamples;)
                    {
                        if (Math.Abs(VolumeStep) > Math.Abs(VolumeDifference))
                        {
                            Volume += VolumeDifference;
                            VolumeDifference = 0;
                        }
                        else
                        {
                            Volume += VolumeStep;
                            VolumeDifference -= VolumeStep;
                        }
                        // Different channels are together in the buffer but they need the same volume modification
                        for (int ch = 0; ch < WaveFormat.Channels; ch++)
                        {
                            buffer[offset + sample++] *= (float)Volume;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < copiedSamples; i++)
                    {
                        buffer[offset + i] *= (float)Volume;
                    }
                }
            }

            // Finally, return the number of samples we copied
            return copiedSamples;
        }

        public WaveFormat WaveFormat => CachedSound.WaveFormat;

        #region Setters

        public void UpdateSoundVolume(decimal volumeTarget, int fadeMS)
        {
            VolumeDifference = volumeTarget - Volume;
            VolumeFadeSampleCount = (int)((fadeMS * WaveFormat.SampleRate) / 1000);
            VolumeStep = VolumeDifference / (VolumeFadeSampleCount / WaveFormat.Channels);
        }

        public void SetIsMuted(bool isMuted)
        {
            IsMuted = isMuted;
        }

        #endregion
    }
}
