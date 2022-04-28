using Astrarium.Algorithms;
using Astrarium.Types;
using Astrarium.Types.Themes;
using System;
using System.Globalization;

namespace Astrarium.Plugins.Planner.Views
{
    public class Converter : ValueConverterBase
    {
        private IEphemFormatter formatter;
        private string formatterName;

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (formatter == null || formatterName != (string)parameter)
            {
                formatterName = (string)parameter;
                formatter = Formatters.GetDefault(formatterName);
            }
            return formatter.Format(value);
        }
    }

    public class TimeLinkVisibilityConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return double.IsNaN((value as Date).Day) ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        }
    }

    public class TimeLinkInverseVisibilityConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return double.IsNaN((value as Date).Day) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }
    }
}
