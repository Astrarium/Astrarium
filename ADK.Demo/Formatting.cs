using System;
using System.Globalization;

namespace ADK.Demo
{
    public interface IEphemFormatter
    {
        string Format(object value);
    }

    public static class Formatters
    {
        private class SimpleFormatter : IEphemFormatter
        {
            public string Format(object value)
            {
                return value?.ToString();
            }
        }

        private class RAFormatter : IEphemFormatter
        {
            public string Format(object value)
            {
                return new HMS((double)value).ToString();
            }
        }

        private class DecFormatter : IEphemFormatter
        {
            public string Format(object value)
            {
                return new DMS((double)value).ToString();
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
            private int decimalPlaces = 0;

            public SignedDoubleFormatter(uint decimalPlaces)
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
                    return Math.Round(v, decimalPlaces).ToString("+0.0", CultureInfo.InvariantCulture);
                }
            }
        }

        public static readonly IEphemFormatter RA = new RAFormatter();
        public static readonly IEphemFormatter Dec = new DecFormatter();
        public static readonly IEphemFormatter RTS = new RTSFormatter();
        public static readonly IEphemFormatter IntAzimuth = new UnsignedDoubleFormatter(0);
        public static readonly IEphemFormatter Altitude1d = new SignedDoubleFormatter(1);
    }
}