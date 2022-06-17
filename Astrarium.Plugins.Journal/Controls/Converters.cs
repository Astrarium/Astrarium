using Astrarium.Types.Themes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Controls
{
    public class DateTimeComparer : IEqualityComparer<DateTime>
    {
        public bool Equals(DateTime x, DateTime y)
        {
            return x.Date.ToString("yyyy-MM-dd").Equals(y.Date.ToString("yyyy-MM-dd"));
        }

        public int GetHashCode(DateTime obj)
        {
            return obj.Date.ToString("yyyy-MM-dd").GetHashCode();
        }
    }

    public class SessionDaysHighlightConverter : MultiValueConverterBase
    {
        private static IEqualityComparer<DateTime> Comparer = new DateTimeComparer();

        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null)
                return false;
            var date = (DateTime)values[0];
            var dates = values[1] as DateTime[];
            if (dates != null)
            {
                return dates.Contains(date, Comparer);
            }
            return false;
        }
    }

    public class SelectedCalendarDateConverter : MultiValueConverterBase
    {
        private static IEqualityComparer<DateTime> Comparer = new DateTimeComparer();

        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(values[0] is DateTime) || !(values[1] is DateTime))
                return false;
            var date = (DateTime)values[0];
            var selectedDate = (DateTime)values[1];
            return Comparer.Equals(date, selectedDate);
        }
    }
}
