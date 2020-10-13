using System;
using System.Windows.Data;

namespace SoundMixer.Converters
{
	class DurationFormatConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (targetType != typeof(string))
			{
				throw new InvalidOperationException("The target must be a string");
			}

			TimeSpan span = TimeSpan.FromTicks((long)value);

			return span.ToString(@"hh\:mm\:ss\.ff");
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}