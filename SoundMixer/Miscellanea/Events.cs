using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace SoundMixer.Miscellanea
{
    public class AddedSoundFromStream
    {
        public SoundFromStreamArgs Args;

        public AddedSoundFromStream(SoundFromStreamArgs e)
        {
            Args = e;
        }

        public class SoundFromStreamArgs : RoutedEventArgs
        {
            public string SoundPath { get; set; }
            public SoundFromStreamArgs(string path)
            {
                SoundPath = path;
            }
        }
    }

    public class AddingSoundFromStream
    {
        // Just need the event, no data required
    }
}
