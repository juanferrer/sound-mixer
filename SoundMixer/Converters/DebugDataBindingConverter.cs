using System;
using System.Windows;
using System.Windows.Data;
using System.Diagnostics;

namespace SoundMixer.Converters
{
    class DebugDataBindingConverter : IValueConverter
    {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			Debugger.Break();
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			Debugger.Break();
			return value;
		}
	}
}
