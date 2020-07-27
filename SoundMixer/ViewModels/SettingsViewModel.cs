using System.Windows;
using System.Windows.Controls;

using SoundMixer.Models;

using Stylet;

namespace SoundMixer.ViewModels
{
    class SettingsViewModel : Screen
    {
        private SettingsModel settingsModel;
        public SettingsModel SettingsModel
        {
            get { return this.settingsModel; }
            set { this.SetAndNotify(ref this.settingsModel, value); }
        }

        public SettingsViewModel(SettingsModel settingsModel)
        {
            this.settingsModel = settingsModel;
        }

        #region EventHandlers
        public void OutputDevice_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        #endregion
    }
}
