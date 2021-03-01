using Astrarium.Types.Themes;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Astrarium.Plugins.ObservationsLog.Controls
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class EmptyToVisibilityConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrWhiteSpace(System.Convert.ToString(value)) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
