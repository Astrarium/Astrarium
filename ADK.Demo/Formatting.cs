using System;
using System.Globalization;

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

        private class PhaseFormatter : IEphemFormatter
        {
            public string Format(object value)
            {
                double phase = (double)value;
                return Math.Round(Math.Abs(phase), 2).ToString("0.00", CultureInfo.InvariantCulture);
            }
        }

        private class RTSFormatter : IEphemFormatter
        {
            public string Format(object value)
            {
                double v = (double)value;
                if (double.IsInfinity(v) || double.IsNaN(v))
                {
                    return "-----";
                }
                else
                {
                    return TimeSpan.FromHours(v * 24).ToString(@"hh\:mm");
                }
            }
        }

        private class UnsignedDoubleFormatter : IEphemFormatter
        {
            private int decimalPlaces = 0;

            public UnsignedDoubleFormatter(uint decimalPlaces)
            {
                this.decimalPlaces = (int)decimalPlaces;
            }

            public string Format(object value)
            {
                double v = (double)value;
                if (double.IsNaN(v))
                {
                    return "-----";
                }
                else
                {
                    return Math.Round(v, decimalPlaces).ToString();
                }
            }
        }

        private class SignedDoubleFormatter : IEphemFormatter
        {
            private readonly string format = null;

            public SignedDoubleFormatter(uint decimalPlaces)
            {
                string decimals = new string('0', (int)decimalPlaces);
                format = $"+0.{decimals};-0.{decimals}";
            }

            public string Format(object value)
            {
                double v = (double)value;
                if (double.IsNaN(v))
                {
                    return "-----";
                }
                else
                {
                    return v.ToString(format, CultureInfo.InvariantCulture);
                }
            }
        }

        public static readonly IEphemFormatter RA = new HMSAngleFormatter();
        public static readonly IEphemFormatter Dec = new SignedAngleFormatter();
        public static readonly IEphemFormatter Latitude = new SignedAngleFormatter();
        public static readonly IEphemFormatter Longitude = new UnsignedAngleFormatter();
        public static readonly IEphemFormatter Time = new RTSFormatter();
        public static readonly IEphemFormatter IntAzimuth = new UnsignedDoubleFormatter(0);
        public static readonly IEphemFormatter Altitude1d = new SignedDoubleFormatter(1);

        public static readonly IEphemFormatter Phase = new PhaseFormatter();
        public static readonly IEphemFormatter Magnitude = new SignedDoubleFormatter(2);
    }
}