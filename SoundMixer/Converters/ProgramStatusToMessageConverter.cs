using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

using SoundMixer.Miscellanea;

namespace SoundMixer.Converters
{
    class ProgramStatusToMessageConverter : IValueConverter
    {
        readonly private Dictionary<Enums.ProgramStatus, string> dictionary = new()
        {
            { Enums.ProgramStatus.Loading, "Loading" },
            { Enums.ProgramStatus.Ready, "Ready" }
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string))
            {
                throw new InvalidOperationException("The target must be a string");
            }

            return dictionary[(Enums.ProgramStatus)value];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Enums.ProgramStatus))
            {
                throw new InvalidOperationException("The target must be an Enums.ProgramStatus");
            }

            return dictionary.FirstOrDefault(x => x.Value == (string)value).Key;
        }
    }
}
