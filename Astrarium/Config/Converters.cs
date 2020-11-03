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

    public class SkyColorValueConverter : MultiValueConverterBase
    {
        private SkyColor skyColor;
        private ISettings settings;

        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            skyColor = (SkyColor)values[0];
            settings = (ISettings)values[1];
            return skyColor.GetColor(settings.Get<ColorSchema>("Schema"));
        }

        public override object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            var schema = settings.Get<ColorSchema>("Schema");
            skyColor.SetColor((Color)value, schema);
            return new object[2] { skyColor, schema };
        }
    }
}
