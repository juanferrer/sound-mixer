using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using SoundMixer.Miscellanea;

using Stylet;

namespace SoundMixer.ViewModels
{
    class StreamViewModel : Screen
    {
        readonly private IEventAggregator eventAggregator;

        private string videoURL;
        public string VideoURL
        {
            get { return this.videoURL; }
            set { this.SetAndNotify(ref this.videoURL, value); }
        }

        public StreamViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        public void Window_Closed(object sender, EventArgs e)
        {
            // TODO: Check it's a valid URL
            this.eventAggregator.Publish(new AddedSoundFromStream(new AddedSoundFromStream.SoundFromStreamArgs(VideoURL)));
        }

    }
}
