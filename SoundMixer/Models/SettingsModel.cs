using System;

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

        /*private LegacyAudioDeviceInfo selectedOutputDevice;

        public LegacyAudioDeviceInfo SelectedOutputDevice
        {
            get => this.selectedOutputDevice;
            set => this.SetAndNotify(ref this.selectedOutputDevice, value);
        }

        private BindableCollection<LegacyAudioDeviceInfo> availableOutputDevices;

        public BindableCollection<LegacyAudioDeviceInfo> AvailableOutputDevices
        {
            get => this.availableOutputDevices;
            set => this.SetAndNotify(ref this.availableOutputDevices, value);
        }*/

        public SettingsModel()
        {

        }
    }
}
