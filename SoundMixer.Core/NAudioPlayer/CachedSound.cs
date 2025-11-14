using NAudio.Mixer;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace SoundMixer.Core.NAudioPlayer
{
    /// <summary>
    /// <para>Read a file from disk and convert it to the desired channel cound and sample rate.<br/>
    /// Contains audio samples, should be used with <see cref="CachedSoundSampleProvider"/>.</para>
    /// <para>Slightly modified from <a href="https://www.markheath.net/post/fire-and-forget-audio-playback-with"/></para>
    /// </summary>
    public class CachedSound
    {
        public float[] AudioData { get; }
        public WaveFormat WaveFormat { get; }
        public CachedSound(string audioFileName, WaveFormat waveFormat)
        {
            using (var audioFileReader = new AudioFileReader(audioFileName))
            {
                WaveFormat = waveFormat;
                var sampleProvider = ConvertToRightSampleRate(audioFileReader, waveFormat);
                sampleProvider = ConvertToRightChannelCount(sampleProvider, waveFormat);

                var wholeFile = new List<float>((int)(audioFileReader.Length) / 4);
                var readBuffer = new float[WaveFormat.SampleRate * WaveFormat.Channels];
                int samplesRead;

                while ((samplesRead = sampleProvider.Read(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    wholeFile.AddRange(readBuffer.Take(samplesRead));
                }

                AudioData = wholeFile.ToArray();
            }
        }

        private static ISampleProvider ConvertToRightChannelCount(ISampleProvider input, WaveFormat waveFormat)
        {
            if (input.WaveFormat.Channels == waveFormat.Channels)
            {
                return input;
            }

            if (input.WaveFormat.Channels == 1 && waveFormat.Channels == 2)
            {
                return new MonoToStereoSampleProvider(input);
            }

            throw new NotImplementedException("Stereo to Mono conversion not implemented");
        }

        private static ISampleProvider ConvertToRightSampleRate(IWaveProvider input, WaveFormat waveFormat)
        {
            if (input.WaveFormat.SampleRate == waveFormat.SampleRate)
            {
                return input.ToSampleProvider();
            }

            using (var resample = new MediaFoundationResampler(input, waveFormat.SampleRate))
            {
                return resample.ToSampleProvider();
            }
        }
    }
}
