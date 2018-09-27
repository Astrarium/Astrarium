using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ADK
{
    public struct DmsAngle
    {
        public int Degrees { get; set; }
        public int Minutes { get; set; }
        public double Seconds { get; set; }

        public DmsAngle(double decimalAngle)
        {
            Degrees = 0;
            Minutes = 0;
            Seconds = 0;

            int sign = Math.Sign(decimalAngle);
            decimalAngle = Math.Abs(decimalAngle);

            Degrees = (int)decimalAngle;
            Minutes = (int)((decimalAngle - Degrees) * 60);
            Seconds = (decimalAngle - Degrees - Minutes / 60.0) * 3600;

            Degrees *= sign;
        }

        public DmsAngle(int degrees, int minutes, double seconds)
        {
            if (minutes < 0 || minutes > 59)
                throw new ArgumentException("Minutes value should be in range from 0 to 59.", nameof(minutes));

            if (seconds < 0 || seconds > 60)
                throw new ArgumentException("Seconds value should be in range from 0 to 59.", nameof(seconds));

            Degrees = degrees;
            Minutes = minutes;
            Seconds = seconds;
        }

        public double ToDecimalAngle()
        {
            return Math.Sign(Degrees) * (Math.Abs(Degrees) + Minutes / 60.0 + Seconds / 3600.0);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:+#;-#}° {1:D2}\u2032 {2:.##}\u2033", Degrees, Minutes, Seconds);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            else if (obj is DmsAngle)
                return ToString() == obj.ToString();
            else
                return false;
        }
    }

    public struct HmsAngle
    {
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public double Seconds { get; set; }

        public HmsAngle(double decimalAngle)
        {
            if (decimalAngle < 0 || decimalAngle > 360)
                throw new ArgumentException("Angle value should be in range from 0 to 360.", nameof(decimalAngle));

            Hours = 0;
            Minutes = 0;
            Seconds = 0;

            decimalAngle /= 15;

            Hours = (int)decimalAngle;
            Minutes = (int)((decimalAngle - Hours) * 60);
            Seconds = (decimalAngle - Hours - Minutes / 60.0) * 3600;
        }

        public HmsAngle(int hours, int minutes, double seconds)
        {
            if (hours < 0 || hours > 23)
                throw new ArgumentException("Hours value should be in range from 0 to 59.", nameof(hours));

            if (minutes < 0 || minutes > 59)
                throw new ArgumentException("Minutes value should be in range from 0 to 59.", nameof(minutes));

            if (seconds < 0 || seconds > 60)
                throw new ArgumentException("Seconds value should be in range from 0 to 59.", nameof(seconds));

            Hours = hours;
            Minutes = minutes;
            Seconds = seconds;
        }

        public double ToDecimalAngle()
        {
            return (Math.Abs(Hours) + Minutes / 60.0 + Seconds / 3600.0) * 15;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:D2}h {1:D2}m {2:.###}s", Hours, Minutes, Seconds);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            else if (obj is HmsAngle)
                return ToString() == obj.ToString();
            else
                return false;
        }
    }
}
