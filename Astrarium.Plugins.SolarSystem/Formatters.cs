using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.SolarSystem
{
    internal class LunarDistanceFormatter : IEphemFormatter
    {
        public string Format(object value)
        {
            return value + " km";
        }
    }

    internal class LibrationLatitudeFormatter : IEphemFormatter
    {
        public string Format(object value)
        {
            double libration = Convert.ToDouble(value);
            return $"{Math.Abs(libration).ToString("0.0", CultureInfo.InvariantCulture)}\u00B0 {(libration > 0 ? "N" : "S")}";
        }
    }

    internal class LibrationLongitudeFormatter : IEphemFormatter
    {
        public string Format(object value)
        {
            double libration = Convert.ToDouble(value);
            return $"{Math.Abs(libration).ToString("0.0", CultureInfo.InvariantCulture)}\u00B0 {(libration > 0 ? "E" : "W")}";
        }
    }
}
