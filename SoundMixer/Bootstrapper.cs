using System;
using System.Collections.Generic;
using System.Text;

using SoundMixer.ViewModels;

using Stylet;
using StyletIoC;

namespace SoundMixer
{
    public class Bootstrapper : Bootstrapper<RootViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            // Configure the IoC container in here
        }

        protected override void Configure()
        {
            // Perform any other configuration before the application starts
        }
    }
}
