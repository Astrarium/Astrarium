using System;

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
                return TimeSpan.FromHours(v * 24).ToString(@"hh\:mm");
            }
        }

        public static readonly IEphemFormatter RA = new RAFormatter();
        public static readonly IEphemFormatter Dec = new DecFormatter();
        public static readonly IEphemFormatter RTS = new RTSFormatter();
    }
}