using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Astrarium.Types.Themes;

namespace Astrarium.Plugins.SolarSystem
{
    internal class MagTypeFormatter
    {
        private static Dictionary<string, string> magTypes = new Dictionary<string, string>{ 
            ["alpha"] = "α",
            ["beta"] = "β",
            ["gamma"] = "γ",
            ["delta"] = "δ"
        };

        public static string MagType(string magType) => string.Join("-", magType.Split('-').Select(x => magTypes[x]));
    }

    internal class MagTypeConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return MagTypeFormatter.MagType(value as string);
        }
    }

    internal class MagTypeColorConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string magType = value as string;
            switch (magType)
            {
                case "gamma":
                case "beta-delta":
                case "beta-gamma":
                case "delta":
                    return System.Windows.Media.Brushes.Tomato;
                case "beta-gamma-delta":
                case "gamma-delta":
                    return System.Windows.Media.Brushes.Brown;
                default:
                    return System.Windows.Media.Brushes.DarkGreen;
            }
        }
    }

}
