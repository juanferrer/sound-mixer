using System;

using Newtonsoft.Json;
using Stylet;
using Unosquare.FFME.Common;

namespace SoundMixer.Models
{
    public class SettingsModel : PropertyChangedBase
    {
        private DirectSoundDeviceInfo selectedOutputDevice;

        public DirectSoundDeviceInfo SelectedOutputDevice
        {
            get => this.selectedOutputDevice;
            set => this.SetAndNotify(ref this.selectedOutputDevice, value);
        }

        private BindableCollection<DirectSoundDeviceInfo> availableOutputDevices;

        public BindableCollection<DirectSoundDeviceInfo> AvailableOutputDevices
        {
            get => this.availableOutputDevices;
            set => this.SetAndNotify(ref this.availableOutputDevices, value);
        }

        public SettingsModel()
        {

        }
    }
}
