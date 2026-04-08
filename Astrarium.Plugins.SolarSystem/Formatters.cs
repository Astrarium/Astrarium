using Astrarium.Types;
using System;
using System.Globalization;

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
        public static IEphemFormatter M = new UnsignedDoubleFormatter(4, "°");
        public static IEphemFormatter P = new UnsignedDoubleFormatter(4, " d");
        public static IEphemFormatter n = new UnsignedDoubleFormatter(4, " °/d");
        public static IEphemFormatter e = new UnsignedDoubleFormatter(4, "°");
        public static IEphemFormatter a = new UnsignedDoubleFormatter(4, " au");
        public static IEphemFormatter i = new UnsignedDoubleFormatter(4, "°");
        public static IEphemFormatter w = new UnsignedDoubleFormatter(4, "°");
        public static IEphemFormatter Om = new UnsignedDoubleFormatter(4, "°");
        public static IEphemFormatter Pw = new SignedDoubleFormatter(4, " y");
        public static IEphemFormatter POm = new SignedDoubleFormatter(4, " y");
    }
}
