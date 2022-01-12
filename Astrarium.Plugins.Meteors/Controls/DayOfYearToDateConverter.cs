using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Astrarium.Plugins.Meteors
{
    public class DayOfYearToDateConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(string) && values.Count() == 2 && values[0] is short dayOfYear && values[1] is double jd0)
                return Formatters.Date.Format(new Date(jd0 + dayOfYear));
            else
                return "?";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
