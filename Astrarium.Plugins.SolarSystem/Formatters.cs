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
            return Convert.ToInt32(value) + " km";
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

    internal static class OrbitalElementsFormatters
    {
        public static IEphemFormatter M = new Formatters.UnsignedDoubleFormatter(4, "°");
        public static IEphemFormatter P = new Formatters.UnsignedDoubleFormatter(4, " d");
        public static IEphemFormatter n = new Formatters.UnsignedDoubleFormatter(4, " °/d");
        public static IEphemFormatter e = new Formatters.UnsignedDoubleFormatter(4, "°");
        public static IEphemFormatter a = new Formatters.UnsignedDoubleFormatter(4, " au");
        public static IEphemFormatter i = new Formatters.UnsignedDoubleFormatter(4, "°");
        public static IEphemFormatter w = new Formatters.UnsignedDoubleFormatter(4, "°");
        public static IEphemFormatter Om = new Formatters.UnsignedDoubleFormatter(4, "°");
        public static IEphemFormatter Pw = new Formatters.SignedDoubleFormatter(4, " y");
        public static IEphemFormatter POm = new Formatters.SignedDoubleFormatter(4, " y");
    }
}
