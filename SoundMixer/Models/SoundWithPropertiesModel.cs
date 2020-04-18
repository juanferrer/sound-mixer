using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;
using Stylet;

namespace SoundMixer.Models
{
    public class SoundWithPropertiesModel : PropertyChangedBase
    {
        private string filePath;

        private string name;

        private double volume;

        /*private bool single;

        private double timeInterval;

        private bool randomInterval;*/


        public string FilePath
        {
            get { return this.filePath; }
            set { this.SetAndNotify(ref this.filePath, value); }
        }

        public string Name
        {
            get { return this.name; }
            set { this.SetAndNotify(ref this.name, value); }
        }

        public double Volume
        {
            get { return this.volume; }
            set { this.SetAndNotify(ref this.volume, value); }
        }
    }
}
