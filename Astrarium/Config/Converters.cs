using Astrarium.Types;
using Astrarium.Types.Themes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Astrarium.Config
{
    public class CultureToStringConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string language = value as string;
            return CultureInfo.GetCultureInfo(language);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CultureInfo cultureInfo = value as CultureInfo;
            return cultureInfo.Name;
        }
    }

    public class CultureDescriptionConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CultureInfo cultureInfo = value as CultureInfo;
            return cultureInfo.NativeName;
        }
    }
}
