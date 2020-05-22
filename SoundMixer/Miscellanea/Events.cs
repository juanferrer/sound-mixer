using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace SoundMixer.Miscellanea
{
    public class AddedSoundFromYouTube
    {
        public SoundFromYouTubeArgs Args;

        public AddedSoundFromYouTube(SoundFromYouTubeArgs e)
        {
            Args = e;
        }

        public class SoundFromYouTubeArgs : RoutedEventArgs
        {
            public string SoundPath { get; set; }
            public SoundFromYouTubeArgs(string path)
            {
                SoundPath = path;
            }
        }
    }

    public class AddingSoundFromYouTube
    {
        // Just need the event, no data required
    }
}
