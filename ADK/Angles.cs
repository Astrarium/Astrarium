using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ADK
{
    /// <summary>
    /// Represents angle expressed in degrees, minutes and seconds
    /// and provides methods to convert from/to decimal units.
    /// </summary>
    public struct DMS
    {
        /// <summary>
        /// Degrees part of angle value.
        /// Should be non-negative.
        /// </summary>
        public uint Degrees { get; set; }

        /// <summary>
        /// Minutes part of angle value.
        /// Should be in range [0 ... 60)
        /// </summary>
        public uint Minutes { get; set; }

        /// <summary>
        /// Seconds part of angle value.
        /// Should be in range [0 ... 60)
        /// </summary>
        public double Seconds { get; set; }

        /// <summary>
        /// Sign of the angle. 
        /// -1 for negative, 1 for positive, 0 for zero value.
        /// </summary>
        private int Sign { get; set; }

        /// <summary>
        /// Creates new angle from its decimal value.
        /// </summary>
        /// <param name="decimalAngle">Decimal value of the angle.</param>
        public DMS(double decimalAngle)
        {
            Degrees = 0;
            Minutes = 0;
            Seconds = 0;
            Sign = Math.Sign(decimalAngle);

            decimalAngle = Math.Abs(decimalAngle);

            Degrees = (uint)decimalAngle;
            Minutes = (uint)((decimalAngle - Degrees) * 60);
            Seconds = (decimalAngle - Degrees - Minutes / 60.0) * 3600;
        }

        /// <summary>
        /// Creates new angle from sexagesimal value 
        /// </summary>
        /// <param name="degrees">Degrees part of angle value. Should be non-negative.</param>
        /// <param name="minutes">Minutes part of angle value. Should be in range [0 ... 60).</param>
        /// <param name="seconds">Seconds part of angle value. Should be in range [0 ... 60).</param>
        public DMS(uint degrees, uint minutes, double seconds)
        {
            if (degrees < 0)
                throw new ArgumentException("Degrees value should be non-negative.", nameof(degrees));

            if (minutes < 0 || minutes > 59)
                throw new ArgumentException("Minutes value should be in range from 0 to 59.", nameof(minutes));

            if (seconds < 0 || seconds > 60)
                throw new ArgumentException("Seconds value should be in range from 0 to 59.", nameof(seconds));

            Degrees = degrees;
            Minutes = minutes;
            Seconds = seconds;
            Sign = Degrees == 0 && Minutes == 0 && Seconds == 0 ? 0 : 1;
        }

        /// <summary>
        /// Changes sign of the degree.
        /// </summary>
        /// <param name="dms">Degree</param>
        /// <returns>Degree value with opposite sign.</returns>
        public static DMS operator-(DMS dms)
        {
            dms.Sign = -dms.Sign;
            return dms;
        }

        private static readonly Regex PARSE_REGEX = new Regex("^\\s*([-+]?)\\s*(\\d+)[\\*°\\s]\\s*(\\d+)[\\s']\\s*(\\d+\\.?\\d*)(''|\"|\\s*){1}\\s*$");

        public static DMS Parse(string angle)
        {
            Match match = PARSE_REGEX.Match(angle);

            if (!match.Success)
                throw new ArgumentException("Unable to parse string as angle value.", nameof(angle));

            int sign = int.Parse(match.Groups[1].Value + "1");

            uint dd = uint.Parse(match.Groups[2].Value);
            uint mm = uint.Parse(match.Groups[3].Value);
            double ss = double.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
                
            return sign > 0 ? new DMS(dd, mm, ss) : -new DMS(dd, mm, ss);            
        }

        /// <summary>
        /// Converts angle value to decimal representation.
        /// </summary>
        /// <returns>
        /// Decimal value of the angle.
        /// </returns>
        public double ToDecimalAngle()
        {
            return Sign * (Math.Abs(Degrees) + Minutes / 60.0 + Seconds / 3600.0);
        }

        /// <summary>
        /// Gets string that represents angle in sexagesimal form.
        /// </summary>
        /// <returns>String that represents angle in sexagesimal form.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:+;-}{1:#}° {2:D2}\u2032 {3:.##}\u2033", Sign, Degrees, Minutes, Seconds);
        }

        /// <summary>
        /// Checks angle value for equality.
        /// </summary>
        /// <param name="obj">Object to compare with.</param>
        /// <returns>True if angle values are equal</returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            else if (obj is DMS)
            {
                var other = (DMS)obj;
                return
                    Degrees == other.Degrees &&
                    Minutes == other.Minutes &&
                    Math.Abs(Seconds - other.Seconds) < 1e-2;
            }
            else return false;
        }

        /// <summary>
        /// Gets hash code for the angle value.
        /// </summary>
        /// <returns>Hash code value.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Degrees.GetHashCode();
                hash = hash * 23 + Minutes.GetHashCode();
                hash = hash * 23 + Seconds.GetHashCode();
                return hash;
            }
        }
    }

    /// <summary>
    /// Represents angle expressed in hours, minutes and seconds
    /// and provides methods to convert from/to decimal units.
    /// </summary>
    public struct HMS
    {
        /// <summary>
        /// Hours part of angle value.
        /// Should be in range [0 ... 24)
        /// </summary>
        public uint Hours { get; set; }

        /// <summary>
        /// Minutes part of angle value.
        /// Should be in range [0 ... 60)
        /// </summary>
        public uint Minutes { get; set; }

        /// <summary>
        /// Seconds part of angle value.
        /// Can be only positive, measured in range [0 ... 60)
        /// </summary>
        public double Seconds { get; set; }

        /// <summary>
        /// Creates new angle from its decimal value.  
        /// </summary>
        /// <param name="decimalAngle">Decimal value of the angle. Is allowed to be in range [0 ... 360)</param>
        public HMS(double decimalAngle)
        {
            if (decimalAngle < 0 || decimalAngle > 360)
                throw new ArgumentException("Angle value should be in range from 0 to 360.", nameof(decimalAngle));

            Hours = 0;
            Minutes = 0;
            Seconds = 0;

            decimalAngle /= 15;

            Hours = (uint)decimalAngle;
            Minutes = (uint)((decimalAngle - Hours) * 60);
            Seconds = (decimalAngle - Hours - Minutes / 60.0) * 3600;
        }

        /// <summary>
        /// Creates new angle from sexagesimal value.
        /// </summary>
        /// <param name="hours">Hours part of angle value. Should be in range [0 ... 260).</param>
        /// <param name="minutes">Minutes part of angle value. Should be in range [0 ... 60).</param>
        /// <param name="seconds">Seconds part of angle value. Should be in range [0 ... 60).</param>
        public HMS(uint hours, uint minutes, double seconds)
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

        /// <summary>
        /// Converts angle value to decimal representation.
        /// </summary>
        /// <returns>
        /// Decimal value of the angle.
        /// </returns>
        public double ToDecimalAngle()
        {
            return (Math.Abs(Hours) + Minutes / 60.0 + Seconds / 3600.0) * 15;
        }

        /// <summary>
        /// Gets string that represents angle in sexagesimal form.
        /// </summary>
        /// <returns>String that represents angle in sexagesimal form.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:D2}h {1:D2}m {2:.###}s", Hours, Minutes, Seconds);
        }

        /// <summary>
        /// Checks angle value for equality.
        /// </summary>
        /// <param name="obj">Object to compare with.</param>
        /// <returns>True if angle values are equal</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            else if (obj is HMS)
            {
                var other = (HMS)obj;
                return
                    Hours == other.Hours &&
                    Minutes == other.Minutes &&
                    Math.Abs(Seconds - other.Seconds) < 1e-2;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets hash code for the angle value.
        /// </summary>
        /// <returns>Hash code value.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Hours.GetHashCode();
                hash = hash * 23 + Minutes.GetHashCode();
                hash = hash * 23 + Seconds.GetHashCode();
                return hash;
            }
        }
    }
}
