using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Astrarium.Types
{
    /// <summary>
    /// Defines methods which must be implemented by ephemeris formatter class
    /// </summary>
    public interface IEphemFormatter
    {
        /// <summary>
        /// Formats ephemeris value to string
        /// </summary>
        /// <param name="value">Ephemeris value</param>
        /// <returns>String representation of the ephemeris value</returns>
        string Format(object value);
    }

    /// <summary>
    /// Contains default formatters for ephemeris values to string
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
            Default["Constellation"]            = new ConstellationFormatter();
            Default["RTS.Rise"]                 = Time;
            Default["RTS.Transit"]              = Time;
            Default["RTS.Set"]                  = Time;
            Default["RTS.RiseAzimuth"]          = new AzimuthShortFormatter();
            Default["RTS.TransitAltitude"]      = new SignedDoubleFormatter(1, "\u00B0");
            Default["RTS.SetAzimuth"]           = new AzimuthShortFormatter();
            Default["RTS.Duration"]             = new DurationFormatter();
            Default["Equatorial0.Alpha"]        = RA;
            Default["Equatorial0.Delta"]        = Dec;
            Default["Equatorial.Alpha"]         = RA;
            Default["Equatorial.Delta"]         = Dec;
            Default["Distance"]                 = DistanceInAu;
            Default["DistanceFromSun"]          = DistanceInAu;
            Default["DistanceFromEarth"]        = DistanceInAu;
            Default["Ecliptical.Lambda"]        = Longitude;
            Default["Ecliptical.Beta"]          = Latitude;
            Default["Horizontal.Azimuth"]       = Azimuth;
            Default["Horizontal.Altitude"]      = Altitude;
            Default["Magnitude"]                = Magnitude;
            Default["PhaseAngle"]               = PhaseAngle;
            Default["Phase"]                    = Phase;
            Default["HorizontalParallax"]       = Angle;
            Default["AngularDiameter"]          = Angle;
            Default["Visibility.Duration"]      = VisibilityDuration;
            Default["Visibility.Period"]        = new VisibilityPeriodFormatter();
            Default["Visibility.Begin"]         = Time;
            Default["Visibility.End"]           = Time;
            Default["Rectangular.X"]            = Rectangular;
            Default["Rectangular.Y"]            = Rectangular;
            Default["Rectangular.Z"]            = Rectangular;
        }

        public static IEphemFormatter GetDefault(string key)
        {
            if (Default.ContainsKey(key))
                return Default[key];
            else
                return Simple;
        }

        private class ConstellationFormatter : IEphemFormatter
        {
            public string Format(object value)
            {
                if (string.IsNullOrEmpty((string)value))
                    return "—";
                else
                    return Text.Get($"ConName.{((string)value).ToUpper()}");
            }
        }

        /// <summary>
        /// Trivial converter for formatting any value to string.
        /// Calls default ToString() implementation for the type.
        /// </summary>
        public class SimpleFormatter : IEphemFormatter
        {
            public string Format(object value)
            {
                return value?.ToString();
            }
        }

        private class HMSAngleFormatter : IEphemFormatter
        {
            private static Func<HMS, string> format = (HMS hms) => string.Format(CultureInfo.InvariantCulture, $"{{0:D2}}{Text.Get("Formatters.HMSAngleFormatter.Hours")} {{1:D2}}{Text.Get("Formatters.HMSAngleFormatter.Minutes")} {{2:.##}}{Text.Get("Formatters.HMSAngleFormatter.Seconds")}", hms.Hours, hms.Minutes, hms.Seconds);

            public string Format(object value)
            {
                if (value == null || double.IsNaN((double)value))
                    return "—";
                else
                    return new HMS((double)value).ToString(format);
            }
        }

        public class SignedAngleFormatter : IEphemFormatter
        {
            public string Format(object value)
            {
                if (value == null || double.IsNaN((double)value))
                    return "—";
                else
                    return new DMS((double)value).ToString();
            }
        }

        public class GeoCoordinatesFormatter : IEphemFormatter
        {
            public string Format(object value)
            {
                if (value is CrdsGeographical g && g != null)
                {
                    string ns = g.Latitude >= 0 ? "N" : "S";
                    string we = g.Longitude >= 0 ? "W" : "E";
                    return $"{new DMS(Math.Abs(g.Latitude)).ToString(DMSFormatter)}\u2009{ns} {new DMS(Math.Abs(g.Longitude)).ToString(DMSFormatter)}\u2009{we}";
                }
                else
                    return null;
            }

            private string DMSFormatter(DMS angle)
            {
                return $"{angle.Degrees:00}°\u2009{angle.Minutes:00}′\u2009{(int)angle.Seconds:00}″";
            }
        }

        public class UnsignedAngleFormatter : IEphemFormatter
        {
            public string Format(object value)
            {
                if (value == null || double.IsNaN((double)value))
                    return "—";
                else
                    return new DMS((double)value).ToUnsignedString();
            }
        }

        public class AzimuthFormatter : IEphemFormatter
        {
            public string Format(object value)
            {
                if (value == null || double.IsNaN((double)value))
                    return "—";
                else
                    return new DMS(Algorithms.Angle.To360((double)value) + (CrdsHorizontal.MeasureAzimuthFromNorth ? 180 : 0)).ToUnsignedString();
            }
        }

        public class AzimuthShortFormatter : IEphemFormatter
        {
            public string Format(object value)
            {
                double v = Convert.ToDouble(value);
                if (double.IsInfinity(v) || double.IsNaN(v))
                {
                    return "—";
                }
                else
                {
                    return Algorithms.Angle.To360(v + (CrdsHorizontal.MeasureAzimuthFromNorth ? 180 : 0)).ToString("0.\u00B0", CultureInfo.InvariantCulture);
                }
            }
        }

        public class AngleFormatter : IEphemFormatter
        {
            public virtual string Format(object value)
            {
                double angle = Convert.ToDouble(value);
                var a = new DMS(angle);
                
                if (angle >= 1)
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0:D}° {1:D2}\u2032 {2:00.##}\u2033", a.Degrees, a.Minutes, a.Seconds);
                }
                else if (angle >= 1.0 / 60)
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0:D2}\u2032 {1:00.##}\u2033", a.Minutes, a.Seconds);
                }
                else
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0:0.##}\u2033", a.Seconds);
                }
            }
        }

        private class PhaseFormatter : IEphemFormatter
        {
            public string Format(object value)
            {
                double phase = 0;
                if (value is double)
                {
                    phase = (double)value;
                }
                else if (value is float)
                {
                    phase = (float)value;
                }
                else
                {
                    return "?";
                }

                return Math.Round(Math.Abs(phase), 2).ToString("0.00", CultureInfo.InvariantCulture);
            }
        }

        public class TimeFormatter : IEphemFormatter
        {
            private bool withSeconds = false;

            public TimeFormatter(bool withSeconds = false)
            {
                this.withSeconds = withSeconds;
            }

            public string Format(object value)
            {
                double time;
                if (value is double v)
                {
                    time = v;
                }
                else if (value is Date date)
                {
                    time = date.Time;
                }
                else
                {
                    return "?";
                }

                if (double.IsInfinity(time) || double.IsNaN(time))
                {
                    return "—";
                }
                else
                {
                    return System.TimeSpan.FromHours(time * 24).ToString(withSeconds ? @"hh\:mm\:ss" : @"hh\:mm");
                }
            }
        }

        private class DurationFormatter : IEphemFormatter
        {
            public string Format(object value)
            {
                double v = (double)value;
                if (v == 0)
                {
                    return Text.Get("Formatters.DurationFormatter.Nonrising");
                }
                else if (v == 1)
                {
                    return Text.Get("Formatters.DurationFormatter.Nonsetting");
                }
                else
                {
                    var ts = System.TimeSpan.FromHours(v * 24);
                    return $"{ts.Hours:D2}{Text.Get("Formatters.DurationFormatter.Hours")} {ts.Minutes:D2}{Text.Get("Formatters.DurationFormatter.Minutes")}";
                }
            }
        }

        public class UnsignedDoubleFormatter : IEphemFormatter
        {
            private readonly string format = null;
            private readonly string units = null;

            public UnsignedDoubleFormatter(uint decimalPlaces, string units)
            {
                string decimals = new string('0', (int)decimalPlaces);
                format = $"0.{decimals}";
                this.units = units;
            }

            public string Format(object value)
            {
                double v = Convert.ToDouble(value);
                if (double.IsInfinity(v) || double.IsNaN(v))
                {
                    return "—";
                }
                else
                {
                    return v.ToString(format, CultureInfo.InvariantCulture) + (units != null ? (units.StartsWith("$") ? Text.Get(units.Substring(1)) : units) : "");
                }
            }
        }

        public class SignedDoubleFormatter : IEphemFormatter
        {
            private readonly string format = null;
            private readonly string units = null;

            public SignedDoubleFormatter(uint decimalPlaces, string units = null)
            {
                string decimals = new string('0', (int)decimalPlaces);
                format = $"{{0:+0.{decimals};-0.{decimals}}}";
                this.units = units;
            }

            public string Format(object value)
            {
                if ((value == null) ||
                    (value is DBNull) ||
                    (value is double && (double.IsInfinity((double)value) || double.IsNaN((double)value))) ||
                    (value is float && (float.IsInfinity((float)value) || float.IsNaN((float)value))))
                {
                    return "—";
                }
                else
                {
                    return string.Format(CultureInfo.InvariantCulture, format, value) + (units ?? "");
                }
            }
        }

        private abstract class AbstractDateFormatter
        {
            protected static string[] AbbreviatedMonthNames => CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames.Take(12).ToArray();
            protected static string[] MonthNames => CultureInfo.CurrentCulture.DateTimeFormat.MonthNames.Take(12).ToArray();
        }

        private class DateTimeFormatter : AbstractDateFormatter, IEphemFormatter
        {
            public string Format(object value)
            {
                if (value is Date d)
                {
                    return $"{(int)d.Day:00} {AbbreviatedMonthNames[d.Month - 1]} {d.Year} {d.Hour:00}:{d.Minute:00}";
                }
                else if (value is DateTime dt)
                {
                    return $"{dt.Day:00} {AbbreviatedMonthNames[dt.Month - 1]} {dt.Year} {dt.Hour:00}:{dt.Minute:00}";
                }
                else
                {
                    return "?";
                }
            }
        }

        private class DateFormatter : AbstractDateFormatter, IEphemFormatter
        {
            public string Format(object value)
            {
                Date d = (Date)value;
                return $"{(int)d.Day:00} {AbbreviatedMonthNames[d.Month - 1]} {d.Year}";
            }
        }

        private class MonthYearFormatter : AbstractDateFormatter, IEphemFormatter
        {
            public string Format(object value)
            {
                Date d = (Date)value;
                return $"{MonthNames[d.Month - 1]} {d.Year}";
            }
        }

        private class VisibilityDurationFormatter : IEphemFormatter
        {
            public string Format(object value)
            {
                double duration = (double)value;
                return duration > 0 ? $"{duration.ToString("0.0", CultureInfo.InvariantCulture)} {Text.Get("Formatters.VisibilityDurationFormatter.Hours")}" : "—";
            }
        }

        private class VisibilityPeriodFormatter : IEphemFormatter
        {
            public string Format(object value)
            {
                VisibilityPeriod p = (VisibilityPeriod)value;

                List<string> visibility = new List<string>();

                if ((p & VisibilityPeriod.Evening) != 0)
                {
                    visibility.Add(Text.Get("Formatters.VisibilityPeriodFormatter.Evening"));
                }
                if ((p & VisibilityPeriod.Night) != 0)
                {
                    visibility.Add(Text.Get("Formatters.VisibilityPeriodFormatter.Night"));
                }
                if ((p & VisibilityPeriod.Morning) != 0)
                {
                    visibility.Add(Text.Get("Formatters.VisibilityPeriodFormatter.Morning"));
                }

                if (visibility.Count == 3)
                    return Text.Get("Formatters.VisibilityPeriodFormatter.WholeNight");
                else if (visibility.Any())
                    return string.Join(", ", visibility);
                else
                    return Text.Get("Formatters.VisibilityPeriodFormatter.Invisible");
            }
        }

        private class TimeSpanFormatter : IEphemFormatter
        {
            public string Format(object value)
            {
                var timeSpan = (TimeSpan)value;

                int d = timeSpan.Days;
                int h = timeSpan.Hours;
                int m = timeSpan.Minutes;
                int s = timeSpan.Seconds;

                var text = new StringBuilder();

                if (d > 0)
                {
                    text.Append(d)
                        .Append(Text.Get("Formatters.TimeSpanFormatter.Days")).Append(" ");
                }
                if (h > 0 || (text.Length > 0 && (m > 0 || s > 0)))
                {
                    text.Append(text.Length > 0 ? $"{h:D2}" : $"{h}")
                        .Append(Text.Get("Formatters.TimeSpanFormatter.Hours")).Append(" ");

                }
                if (m > 0 || (text.Length > 0 && (s > 0)))
                {
                    text.Append(text.Length > 0 ? $"{m:D2}" : $"{m}")
                        .Append(Text.Get("Formatters.TimeSpanFormatter.Minutes")).Append(" ");
                }
                if (s > 0)
                {
                    text.Append(text.Length > 0 ? $"{s:D2}" : $"{s}")
                        .Append(Text.Get("Formatters.TimeSpanFormatter.Seconds"));
                }

                return text.ToString().Trim();
            }
        }

        public static readonly IEphemFormatter Simple = new SimpleFormatter(); 
        public static readonly IEphemFormatter RA = new HMSAngleFormatter();
        public static readonly IEphemFormatter Dec = new SignedAngleFormatter();
        public static readonly IEphemFormatter Altitude = new SignedAngleFormatter();
        public static readonly IEphemFormatter Azimuth = new AzimuthFormatter();
        public static readonly IEphemFormatter Latitude = new SignedAngleFormatter();
        public static readonly IEphemFormatter Longitude = new SignedAngleFormatter();
        public static readonly IEphemFormatter Time = new TimeFormatter();
        public static readonly IEphemFormatter DistanceInAu = new UnsignedDoubleFormatter(3, "$Formatters.DistanceInAu.AU");
        public static readonly IEphemFormatter Phase = new PhaseFormatter();
        public static readonly IEphemFormatter VisibilityDuration = new VisibilityDurationFormatter();
        public static readonly IEphemFormatter Magnitude = new SignedDoubleFormatter(2, "ᵐ");
        public static readonly IEphemFormatter PhaseAngle = new UnsignedDoubleFormatter(2, "\u00B0");
        public static readonly IEphemFormatter Angle = new AngleFormatter();
        public static readonly IEphemFormatter DateTime = new DateTimeFormatter();
        public static readonly IEphemFormatter Date = new DateFormatter();
        public static readonly IEphemFormatter MonthYear = new MonthYearFormatter();
        public static readonly IEphemFormatter TimeSpan = new TimeSpanFormatter();
        public static readonly IEphemFormatter Rectangular = new SignedDoubleFormatter(3);
    }
}