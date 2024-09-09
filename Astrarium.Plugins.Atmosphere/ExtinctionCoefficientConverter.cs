using Astrarium.Types;
using Astrarium.Types.Themes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Atmosphere
{
    internal class ExtinctionCoefficientConverter : ValueConverterBase
    {
        private static double[] ranges = new double[] { 0.1, 0.15, 0.20, 0.25, 0.30, 0.40, 0.50 };

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal coeff = (decimal)value;
            int index = ranges.TakeWhile(x => x <= (double)coeff).Count() - 1;
            return Text.Get($"ExtinctionCoefficientValue.{index}");
        }
    }
}
