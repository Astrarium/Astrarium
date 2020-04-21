using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.SolarSystem
{
    /// <summary>
    /// Converts size of Saturn rings expressed in arcseconds to string
    /// </summary>
    internal class SaturnRingsFormatter : IEphemFormatter
    {
        public string Format(object value)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.##}\u2033", (double)value);
        }
    }

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
