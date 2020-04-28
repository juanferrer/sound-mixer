﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

using SoundMixer.Models;
using SoundMixer.Extensions;

using Newtonsoft.Json;
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
            TextBox textBox = sender as TextBox;
            string newText = string.Empty;
            int count = 0;
            foreach (char c in textBox.Text.ToCharArray())
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
