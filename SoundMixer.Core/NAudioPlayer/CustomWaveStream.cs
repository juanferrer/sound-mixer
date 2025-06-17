using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;

namespace SoundMixer.Core.NAudioPlayer
{
    public class CustomWaveStream : WaveStream
    {
        WaveStream SourceStream;

        public override WaveFormat WaveFormat { get { return SourceStream.WaveFormat; } }

        public override long Length { get { return SourceStream.Length; } }

        public override long Position
        {
            get { return SourceStream.Position; }
            set { SourceStream.Position = value; }
        }

        public bool IsLoop { get; set; }
        public bool IsDelayed { get; set; }
        public bool UseRandomeDelay { get; set; }
        public long MinDelay { get; set; }
        public long MaxDelay { get; set; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var totalBytesRead = 0;

            while (totalBytesRead < count)
            {
                var bytesRead = SourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);

                if (bytesRead == 0)
                {
                    if (SourceStream.Position == 0 || !IsLoop)
                    {
                        // Something wrong with the source stream
                        break;
                    }

                    // Loop
                    if (IsDelayed)
                    {
                        // TODO: Maybe return silence bytes during the delay?
                    }
                    SourceStream.Position = 0;
                }

                totalBytesRead += bytesRead;
            }

            return totalBytesRead;
        }

        public CustomWaveStream(WaveStream sourceStream)
        {
            SourceStream = sourceStream;
        }
    }
}
