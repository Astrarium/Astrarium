using System;
using System.Globalization;

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
        /// Can be positive or negative.
        /// </summary>
        public int Degrees { get; set; }

        /// <summary>
        /// Minutes part of angle value.
        /// Can be only positive, measured in range [0 ... 60)
        /// </summary>
        public int Minutes { get; set; }

        /// <summary>
        /// Seconds part of angle value.
        /// Can be only positive, measured in range [0 ... 60)
        /// </summary>
        public double Seconds { get; set; }

        /// <summary>
        /// Creates new angle from its decimal value.
        /// </summary>
        /// <param name="decimalAngle">Decimal value of the angle.</param>
        public DMS(double decimalAngle)
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

        /// <summary>
        /// Creates new angle from sexagesimal value 
        /// </summary>
        /// <param name="degrees">Degrees part of angle value. Can be positive or negative.</param>
        /// <param name="minutes">Minutes part of angle value. Can be only positive, measured in range [0 ... 60).</param>
        /// <param name="seconds">Seconds part of angle value. Can be only positive, measured in range [0 ... 60).</param>
        public DMS(int degrees, int minutes, double seconds)
        {
            if (minutes < 0 || minutes > 59)
                throw new ArgumentException("Minutes value should be in range from 0 to 59.", nameof(minutes));

            if (seconds < 0 || seconds > 60)
                throw new ArgumentException("Seconds value should be in range from 0 to 59.", nameof(seconds));

            Degrees = degrees;
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
            return (Degrees == 0 ? 1 : Math.Sign(Degrees)) * (Math.Abs(Degrees) + Minutes / 60.0 + Seconds / 3600.0);
        }

        /// <summary>
        /// Gets string that represents angle in sexagesimal form.
        /// </summary>
        /// <returns>String that represents angle in sexagesimal form.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:+#;-#}° {1:D2}\u2032 {2:.##}\u2033", Degrees, Minutes, Seconds);
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
        /// Can be only positive, measured in range [0 ... 24)
        /// </summary>
        public int Hours { get; set; }

        /// <summary>
        /// Minutes part of angle value.
        /// Can be only positive, measured in range [0 ... 60)
        /// </summary>
        public int Minutes { get; set; }

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

            Hours = (int)decimalAngle;
            Minutes = (int)((decimalAngle - Hours) * 60);
            Seconds = (decimalAngle - Hours - Minutes / 60.0) * 3600;
        }

        /// <summary>
        /// Creates new angle from sexagesimal value.
        /// </summary>
        /// <param name="hours">Hours part of angle value. Should be in range [0 ... 260).</param>
        /// <param name="minutes">Minutes part of angle value. Can be only positive, measured in range [0 ... 60).</param>
        /// <param name="seconds">Seconds part of angle value. Can be only positive, measured in range [0 ... 60).</param>
        public HMS(int hours, int minutes, double seconds)
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
