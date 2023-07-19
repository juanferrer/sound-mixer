using System.Windows;
using System.Windows.Controls;

using SoundMixer.Models;

using Stylet;

namespace SoundMixer.ViewModels
{
    class SoundEditViewModel : Screen
    {
        private SoundPropertyModel soundPropertyModel;
        public SoundPropertyModel SoundPropertyModel
        {
            get { return this.soundPropertyModel; }
            private set
            {
                // Validate first

                this.SetAndNotify(ref this.soundPropertyModel, value);
            }
        } 

        public SoundEditViewModel(SoundPropertyModel soundPropertyModel)
        {
            this.soundPropertyModel = soundPropertyModel;
        }

        public void Close()
        {
            this.RequestClose(true);
        }

        public void TextBox_TextChanged(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            var newText = string.Empty;
            var count = 0;
            foreach (var c in textBox.Text.ToCharArray())
            {
                if (char.IsDigit(c) || char.IsControl(c) || (c == '.' && count == 0))
                {
                    newText += c;
                    if (c == '.')
                        count += 1;
                }
            }
            textBox.Text = newText;
        }
    }
}
