using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ADK.Demo
{
    public interface IEphemFormatter
    {
        string Format(object value);
    }

    /// <summary>
    /// Contains default formatters for converting data values to string
    /// </summary>
    public static class Formatters
    {
        /// <summary>
        /// Dictionary of defult formatters to be used when formatter is not provided.
        /// Key is an ephemeris key, value is formatter instance, if exists.
        /// </summary>
        public static Dictionary<string, IEphemFormatter> Default { get; } = new Dictionary<string, IEphemFormatter>();

        /// <summary>
        /// Intializes default formatters dictionary.
        /// </summary>
        static Formatters()
        {
            Default["RTS.Rise"]                 = Time;
            Default["RTS.Transit"]              = Time;
            Default["RTS.Set"]                  = Time;
            Default["Equatorial0.Alpha"]        = RA;
            Default["Equatorial0.Delta"]        = Dec;
            Default["Equatorial.Alpha"]         = RA;
            Default["Equatorial.Delta"]         = Dec;
            Default["Ecliptical.Lambda"]        = Longitude;
            Default["Ecliptical.Beta"]          = Latitude;
            Default["Horizontal.Azimuth"]       = Longitude;
            Default["Horizontal.Altitude"]      = Latitude;
            Default["Magnitude"]                = Magnitude;
            Default["PhaseAngle"]               = PhaseAngle;
            Default["Phase"]                    = Phase;
            Default["HorizontalParallax"]       = HorizontalParallax;
            Default["AngularDiameter"]          = AngularDiameter;
            Default["Libration.Latitude"]       = LibrationLatitude;
            Default["Libration.Longitude"]      = LibrationLongitude;
            Default["MoonPhases.NewMoon"]       = DateTime;
            Default["MoonPhases.FirstQuarter"]  = DateTime;
            Default["MoonPhases.FullMoon"]      = DateTime;
            Default["MoonPhases.LastQuarter"]   = DateTime;
            Default["MoonApsides.Apogee"]       = DateTime;
            Default["MoonApsides.Perigee"]      = DateTime;
        }

        public static IEphemFormatter GetDefault(string key)
        {
            IEphemFormatter formatter = null;
            Default.TryGetValue(key, out formatter);
            if (formatter == null)
            {
                formatter = Simple;
            }
            return formatter;
        }

        /// <summary>
        /// Trivial converter for formatting any value to string.
        /// Calls default ToString() implementation for the type.
        /// </summary>
        private class SimpleFormatter : IEphemFormatter
        {
            public string Format(object value)
            {
                return value?.ToString();
            }
        }

        private class HMSAngleFormatter : IEphemFormatter
        {
            public string Format(object value)
            {
                return new HMS((double)value).ToString();
            }
        }

        private class SignedAngleFormatter : IEphemFormatter
        {
            public string Format(object value)
            {
                return new DMS((double)value).ToString();
            }
        }

        private class UnsignedAngleFormatter : IEphemFormatter
        {
            public string Format(object value)
            {
                return new DMS((double)value).ToUnsignedString();
            }
        }

        private class AngularDiameterFormatter : IEphemFormatter
        {
            public string Format(object value)
            {
                double angle = (double)value;
                var a = new DMS((double)value);
                
                if (angle > 1)
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0:D}° {1:D2}\u2032 {2:00.##}\u2033", a.Degrees, a.Minutes, a.Seconds);
                }
                else if (angle > 1.0 / 60)
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0:D2}\u2032 {1:00.##}\u2033", a.Minutes, a.Seconds);
                }
                else
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0:00.##}\u2033", a.Seconds);
                }
            }
        }

        private class PhaseFormatter : IEphemFormatter
        {
            public string Format(object value)
            {
                double phase = (double)value;
                return Math.Round(Math.Abs(phase), 2).ToString("0.00", CultureInfo.InvariantCulture);
            }
        }

        private class TimeFormatter : IEphemFormatter
        {
            public string Format(object value)
            {
                double v = (double)value;
                if (double.IsInfinity(v) || double.IsNaN(v))
                {
                    return "—";
                }
                else
                {
                    return TimeSpan.FromHours(v * 24).ToString(@"hh\:mm");
                }
            }
        }

        private class UnsignedDoubleFormatter : IEphemFormatter
        {
            private readonly string format = null;
            private readonly string units = null;

            public UnsignedDoubleFormatter(uint decimalPlaces, string units = null)
            {
                string decimals = new string('0', (int)decimalPlaces);
                format = $"0.{decimals}";
                this.units = units;
            }

            public string Format(object value)
            {
                double v = (double)value;
                return v.ToString(format, CultureInfo.InvariantCulture) + (units ?? "");
            }
        }

        private class SignedDoubleFormatter : IEphemFormatter
        {
            private readonly string format = null;
            private readonly string units = null;

            public SignedDoubleFormatter(uint decimalPlaces, string units = null)
            {
                string decimals = new string('0', (int)decimalPlaces);
                format = $"+0.{decimals};-0.{decimals}";
                this.units = units;
            }

            public string Format(object value)
            {
                double v = (double)value;
                return v.ToString(format, CultureInfo.InvariantCulture) + (units ?? "");
            }
        }

        private class LibrationLatitudeFormatter : IEphemFormatter
        {
            public string Format(object value)
            {
                double libration = (double)value;
                return $"{Math.Abs(libration).ToString("0.0", CultureInfo.InvariantCulture)}\u00B0 {(libration > 0 ? "N" : "S")}";
            }
        }

        private class LibrationLongitudeFormatter : IEphemFormatter
        {
            public string Format(object value)
            {
                double libration = (double)value;
                return $"{Math.Abs(libration).ToString("0.0", CultureInfo.InvariantCulture)}\u00B0 {(libration > 0 ? "E" : "W")}";
            }
        }

        private class DateTimeFormatter : IEphemFormatter
        {
            private readonly string[] months = CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedMonthNames.Take(12).ToArray();

            public string Format(object value)
            {
                Date d = (Date)value;
                return $"{(int)d.Day:00} {months[d.Month-1]} {d.Year} {d.Hour:00}:{d.Minute:00}";
            }
        }

        private static readonly IEphemFormatter Simple = new SimpleFormatter(); 

        public static readonly IEphemFormatter RA = new HMSAngleFormatter();
        public static readonly IEphemFormatter Dec = new SignedAngleFormatter();
        public static readonly IEphemFormatter Latitude = new SignedAngleFormatter();
        public static readonly IEphemFormatter Longitude = new UnsignedAngleFormatter();
        public static readonly IEphemFormatter Time = new TimeFormatter();
        public static readonly IEphemFormatter IntAzimuth = new UnsignedDoubleFormatter(0);
        public static readonly IEphemFormatter Altitude1d = new SignedDoubleFormatter(1);

        public static readonly IEphemFormatter Phase = new PhaseFormatter();
        public static readonly IEphemFormatter Magnitude = new SignedDoubleFormatter(2, " m");
        public static readonly IEphemFormatter PhaseAngle = new UnsignedDoubleFormatter(2, "\u00B0");
        public static readonly IEphemFormatter HorizontalParallax = new UnsignedAngleFormatter();
        public static readonly IEphemFormatter AngularDiameter = new AngularDiameterFormatter();

        public static readonly IEphemFormatter LibrationLatitude = new LibrationLatitudeFormatter();
        public static readonly IEphemFormatter LibrationLongitude = new LibrationLongitudeFormatter();

        public static readonly IEphemFormatter DateTime = new DateTimeFormatter();
    }
}