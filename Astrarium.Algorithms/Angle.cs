﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Astrarium.Algorithms
{
    #region Angle

    /// <summary>
    /// Contains utility methods to work with angle values.
    /// </summary>
    public static class Angle
    {
        /// <summary>
        /// 1 radian in degrees 
        /// </summary>
        private const double RAD = 180.0 / Math.PI;

        /// <summary>
        /// Converts angle value expressed in degrees to radians.
        /// </summary>
        /// <param name="angle">Angle value in degrees</param>
        /// <returns>Angle value expressed in radians</returns>
        public static double ToRadians(double angle)
        {
            return angle / RAD;
        }

        /// <summary>
        /// Converts angle value expressed in radians to degrees.
        /// </summary>
        /// <param name="angle">Angle value in radians</param>
        /// <returns>Angle value expressed in degrees</returns>
        public static double ToDegrees(double angle)
        {
            return angle * RAD;
        }

        /// <summary>
        /// Normalizes angle value expressed in degrees to value in range from 0 to 360.
        /// </summary>
        /// <param name="angle">Angle value expressed in degrees.</param>
        /// <returns>Value expressed in degrees in range from 0 to 360</returns>
        public static double To360(double angle)
        {
            return (angle %= 360) >= 0 ? angle : (angle + 360);
        }

        /// <summary>
        /// Normalizes angle value expressed in degrees to value in range from -180 to +180.
        /// </summary>
        /// <param name="angle">Angle value expressed in degrees.</param>
        /// <returns>Value expressed in degrees in range from -180 to +180</returns>
        public static double To180(double angle) 
        {
            return To360(angle + 180) - 180;
        }

        /// <summary>
        /// Calculates angular separation between two points with horizontal coordinates
        /// </summary>
        /// <param name="p1">Horizontal coordinates of the first point</param>
        /// <param name="p2">Horizontal coordinates of the second point</param>
        /// <returns>Angular separation in degrees</returns>
        public static double Separation(CrdsHorizontal p1, CrdsHorizontal p2)
        {
            double a1 = ToRadians(p1.Altitude);
            double a2 = ToRadians(p2.Altitude);
            double A1 = p1.Azimuth;
            double A2 = p2.Azimuth;

            double a = Math.Acos(
                Math.Sin(a1) * Math.Sin(a2) +
                Math.Cos(a1) * Math.Cos(a2) * Math.Cos(ToRadians(A1 - A2)));

            return double.IsNaN(a) ? 0 : ToDegrees(a);
        }

        /// <summary>
        /// Calculates angular separation between two points with equatorial coordinates
        /// </summary>
        /// <param name="p1">Equatorial coordinates of the first point</param>
        /// <param name="p2">Equatorial coordinates of the second point</param>
        /// <returns>Angular separation in degrees</returns>
        public static double Separation(CrdsEquatorial p1, CrdsEquatorial p2)
        {
            double a1 = ToRadians(p1.Delta);
            double a2 = ToRadians(p2.Delta);
            double A1 = p1.Alpha;
            double A2 = p2.Alpha;

            double a = Math.Acos(
                Math.Sin(a1) * Math.Sin(a2) +
                Math.Cos(a1) * Math.Cos(a2) * Math.Cos(ToRadians(A1 - A2)));

            return double.IsNaN(a) ? 0 : ToDegrees(a);
        }

        /// <summary>
        /// Calculates angular separation between two points with ecliptical coordinates
        /// </summary>
        /// <param name="p1">Ecliptical coordinates of the first point</param>
        /// <param name="p2">Ecliptical coordinates of the second point</param>
        /// <returns>Angular separation in degrees</returns>
        public static double Separation(CrdsEcliptical p1, CrdsEcliptical p2)
        {
            double a1 = ToRadians(p1.Beta);
            double a2 = ToRadians(p2.Beta);
            double A1 = p1.Lambda;
            double A2 = p2.Lambda;

            double a = Math.Acos(
                Math.Sin(a1) * Math.Sin(a2) +
                Math.Cos(a1) * Math.Cos(a2) * Math.Cos(ToRadians(A1 - A2)));

            return double.IsNaN(a) ? 0 : ToDegrees(a);
        }

        /// <summary>
        /// Calculates angular separation between two points with geographical coordinates
        /// </summary>
        /// <param name="p1">Geographical coordinates of the first point</param>
        /// <param name="p2">Geographical coordinates of the second point</param>
        /// <returns>Angular separation in degrees</returns>
        public static double Separation(CrdsGeographical p1, CrdsGeographical p2)
        {
            double a1 = ToRadians(p1.Latitude);
            double a2 = ToRadians(p2.Latitude);
            double A1 = To360(p1.Longitude);
            double A2 = To360(p2.Longitude);

            double a = Math.Acos(
                Math.Sin(a1) * Math.Sin(a2) +
                Math.Cos(a1) * Math.Cos(a2) * Math.Cos(ToRadians(A1 - A2)));

            return double.IsNaN(a) ? 0 : ToDegrees(a);
        }

        /// <summary>
        /// Calculates an intermediate point at any fraction along the great circle path 
        /// between two points with horizontal coordinates
        /// </summary>
        /// <param name="p1">Horizontal coordinates of the first point</param>
        /// <param name="p2">Horizontal coordinates of the second point</param>
        /// <param name="fraction">Fraction along great circle route (f=0 is point 1, f=1 is point 2).</param>
        /// <returns>
        /// The intermediate point at specified fraction
        /// </returns>
        /// <remarks>
        /// Formula is taken from <see href="http://www.movable-type.co.uk/scripts/latlong.html"/>
        /// that is originally based on <see cref="http://www.edwilliams.org/avform.htm#Intermediate"/>.
        /// </remarks>
        public static CrdsHorizontal Intermediate(CrdsHorizontal p1, CrdsHorizontal p2, double fraction)
        {
            if (fraction < 0 || fraction > 1)
                throw new ArgumentException("Fraction value should be in range [0, 1]", nameof(fraction));

            double d = ToRadians(Separation(p1, p2));

            double a, b;

            if (d <= 1e-6)
            {
                a = 1 - fraction;
                b = fraction;
            }
            else
            {
                a = Math.Sin((1 - fraction) * d) / Math.Sin(d);
                b = Math.Sin(fraction * d) / Math.Sin(d);
            }

            double alt1 = ToRadians(p1.Altitude);
            double alt2 = ToRadians(p2.Altitude);
            double az1 = ToRadians(p1.Azimuth);
            double az2 = ToRadians(p2.Azimuth);

            double x = a * Math.Cos(alt1) * Math.Cos(az1) + b * Math.Cos(alt2) * Math.Cos(az2);
            double y = a * Math.Cos(alt1) * Math.Sin(az1) + b * Math.Cos(alt2) * Math.Sin(az2);
            double z = a * Math.Sin(alt1) + b * Math.Sin(alt2);
            double alt = Math.Atan2(z, Math.Sqrt(x * x + y * y));
            double az = Math.Atan2(y, x);

            return new CrdsHorizontal(ToDegrees(az), ToDegrees(alt));
        }

        /// <summary>
        /// Calculates an intermediate point at any fraction along the great circle path 
        /// between two points with geographical coordinates
        /// </summary>
        /// <param name="p1">Geographical coordinates of the first point</param>
        /// <param name="p2">Geographical coordinates of the second point</param>
        /// <param name="fraction">Fraction along great circle route (f=0 is point 1, f=1 is point 2).</param>
        /// <returns>
        /// The intermediate point at specified fraction
        /// </returns>
        /// <remarks>
        /// Formula is taken from <see href="http://www.movable-type.co.uk/scripts/latlong.html"/>
        /// that is originally based on <see cref="http://www.edwilliams.org/avform.htm#Intermediate"/>.
        /// </remarks>
        public static CrdsGeographical Intermediate(CrdsGeographical p1, CrdsGeographical p2, double fraction)
        {
            if (fraction < 0 || fraction > 1)
                throw new ArgumentException("Fraction value should be in range [0, 1]", nameof(fraction));

            double d = ToRadians(Separation(p1, p2));

            double a, b;

            if (d <= 1e-6)
            {
                a = 1 - fraction;
                b = fraction;
            }
            else
            {
                a = Math.Sin((1 - fraction) * d) / Math.Sin(d);
                b = Math.Sin(fraction * d) / Math.Sin(d);
            }

            double alt1 = ToRadians(p1.Latitude);
            double alt2 = ToRadians(p2.Latitude);
            double az1 = ToRadians(p1.Longitude);
            double az2 = ToRadians(p2.Longitude);

            double x = a * Math.Cos(alt1) * Math.Cos(az1) + b * Math.Cos(alt2) * Math.Cos(az2);
            double y = a * Math.Cos(alt1) * Math.Sin(az1) + b * Math.Cos(alt2) * Math.Sin(az2);
            double z = a * Math.Sin(alt1) + b * Math.Sin(alt2);
            double lat = Math.Atan2(z, Math.Sqrt(x * x + y * y));
            double lon = Math.Atan2(y, x);

            return new CrdsGeographical(ToDegrees(lon), ToDegrees(lat));
        }

        // TODO: tests
        public static double[] Align(double[] array)
        {
            for (int i = 0; i < array.Length - 1; i++)
            {
                double d = array[i + 1] - array[i];
                if (Math.Abs(d) > 180)
                {
                    array[i + 1] += 360 * -Math.Sign(d);
                }
            }
            return array;
        }
    }

    #endregion Angle

    #region DMS

    /// <summary>
    /// Represents angle expressed in degrees, minutes and seconds
    /// and provides methods to convert from/to decimal units.
    /// </summary>
    public struct DMS
    {
        /// <summary>
        /// Regex to parse angle value from string.
        /// </summary>
        private static readonly Regex REGEX = new Regex(@"^\s*([-+]?)\s*(\d+)[\*°d\s]\s*(\d+)[\s'm]\s*(\d+\.?\d*)(''|\""|s|\\s*){1}\s*$");

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

            const decimal _60 = 60;

            Degrees = (uint)decimalAngle;
            Minutes = (uint)(((decimal)decimalAngle - Degrees) * _60);
            Seconds = (double)(((decimal)decimalAngle - Degrees - Minutes / _60) * 3600);
        }

        /// <summary>
        /// Creates new angle from sexagesimal value 
        /// </summary>
        /// <param name="degrees">Degrees part of angle value. Should be non-negative.</param>
        /// <param name="minutes">Minutes part of angle value. Should be in range [0 ... 60).</param>
        /// <param name="seconds">Seconds part of angle value. Should be in range [0 ... 60).</param>
        public DMS(uint degrees, uint minutes, double seconds)
        {
            Degrees = degrees;
            Minutes = minutes;
            Seconds = seconds;
            Sign = Degrees == 0 && Minutes == 0 && Seconds == 0 ? 0 : 1;

            Validate();
        }

        /// <summary>
        /// Creates new angle from string that represents sexagesimal value. 
        /// </summary>
        /// <param name="angle">String in form DD*MM'SS''</param>
        public DMS(string angle)
        {
            Match match = REGEX.Match(angle);

            if (!match.Success)
                throw new ArgumentException("Unable to parse string as angle value.", nameof(angle));

            Sign = int.Parse(match.Groups[1].Value + "1");

            Degrees = uint.Parse(match.Groups[2].Value);
            Minutes = uint.Parse(match.Groups[3].Value);
            Seconds = double.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);

            Validate();
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
            return string.Format(CultureInfo.InvariantCulture, "{0:+;-}{1:D}° {2:D2}\u2032 {3:00.##}\u2033", Sign, Degrees, Minutes, Seconds);
        }

        /// <summary>
        /// Gets string that represents angle in unsigned sexagesimal form.
        /// </summary>
        /// <returns>String that represents angle in unsigned sexagesimal form.</returns>
        public string ToUnsignedString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:D}° {1:D2}\u2032 {2:00.##}\u2033", Degrees, Minutes, Seconds);
        }

        /// <summary>
        /// Converts sexagesimal angle to arbitrary string representation.
        /// </summary>
        /// <param name="formatter">Formatter function</param>
        /// <returns></returns>
        public string ToString(Func<DMS, string> formatter)
        {
            return formatter(this);
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
                    Sign == other.Sign &&
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
                hash = hash * 23 + Sign.GetHashCode();
                hash = hash * 23 + Degrees.GetHashCode();
                hash = hash * 23 + Minutes.GetHashCode();
                hash = hash * 23 + Seconds.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Checks correctness of the value
        /// </summary>
        private void Validate()
        {
            if (Degrees < 0)
                throw new ArgumentException("Degrees value should be non-negative.", nameof(Degrees));

            if (Minutes < 0 || Minutes > 59)
                throw new ArgumentException("Minutes value should be in range from 0 to 59.", nameof(Minutes));

            if (Seconds < 0 || Seconds > 60)
                throw new ArgumentException("Seconds value should be in range from 0 to 59.", nameof(Seconds));
        }
    }

    #endregion DMS

    #region HMS

    /// <summary>
    /// Represents angle expressed in hours, minutes and seconds
    /// and provides methods to convert from/to decimal units.
    /// </summary>
    public struct HMS
    {
        /// <summary>
        /// Regex to parse angle value from string.
        /// </summary>
        private static readonly Regex REGEX = new Regex("^\\s*(\\d+)\\s*[h\\s]\\s*(\\d+)\\s*[m\\s]\\s*(\\d+\\.?\\d*)\\s*[s\\s*]?\\s*$");

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

            const decimal _60 = 60;

            Hours = (uint)decimalAngle;
            Minutes = (uint)(((decimal)decimalAngle - Hours) * _60);
            Seconds = (double)(((decimal)decimalAngle - Hours - Minutes / _60) * 3600);
        }

        /// <summary>
        /// Creates new angle from sexagesimal value.
        /// </summary>
        /// <param name="hours">Hours part of angle value. Should be in range [0 ... 360).</param>
        /// <param name="minutes">Minutes part of angle value. Should be in range [0 ... 60).</param>
        /// <param name="seconds">Seconds part of angle value. Should be in range [0 ... 60).</param>
        public HMS(uint hours, uint minutes, double seconds)
        {
            Hours = hours;
            Minutes = minutes;
            Seconds = seconds;

            Validate();
        }

        /// <summary>
        /// Creates new angle from string that represents sexagesimal value. 
        /// </summary>
        /// <param name="angle">String in form HHh MMm SSs</param>
        public HMS(string angle)
        {
            Match match = REGEX.Match(angle);

            if (!match.Success)
                throw new ArgumentException("Unable to parse string as angle value.", nameof(angle));

            Hours = uint.Parse(match.Groups[1].Value);
            Minutes = uint.Parse(match.Groups[2].Value);
            Seconds = double.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);

            Validate();
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
            return string.Format(CultureInfo.InvariantCulture, "{0:D2}h {1:D2}m {2:00.###}s", Hours, Minutes, Seconds);
        }

        /// <summary>
        /// Converts sexagesimal angle to arbitrary string representation.
        /// </summary>
        /// <param name="formatter">Formatter function</param>
        /// <returns></returns>
        public string ToString(Func<HMS, string> formatter)
        {
            return formatter(this);
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

        /// <summary>
        /// Checks correctness of the value
        /// </summary>
        private void Validate()
        {
            if (Hours < 0 || Hours > 23)
                throw new ArgumentException("Hours value should be in range from 0 to 59.", nameof(Hours));

            if (Minutes < 0 || Minutes > 59)
                throw new ArgumentException("Minutes value should be in range from 0 to 59.", nameof(Minutes));

            if (Seconds < 0 || Seconds > 60)
                throw new ArgumentException("Seconds value should be in range from 0 to 59.", nameof(Seconds));
        }
    }

    #endregion HMS

    #region AngleRange

    /// <summary>
    /// Defines a circular sector with two given angle values: position angle (start) and width (range), 
    /// both expressed in degrees from 0 to 360.
    /// </summary>
    public class AngleRange
    {
        /// <summary>
        /// Creates new instance of <see cref="AngleRange"/>.
        /// </summary>
        /// <param name="start">Starting position angle (start), in degrees.</param>
        /// <param name="range">Sector width (range), in degrees.</param>
        public AngleRange(double start, double range)
        {
            Start = Angle.To360(start);
            Range = Angle.To360(range);
        }

        /// <summary>
        /// Gets starting position angle (start), in degrees.
        /// </summary>
        public double Start { get; private set; }

        /// <summary>
        /// Gets sector width (range), in degrees.
        /// </summary>
        public double Range { get; private set; }

        /// <summary>
        /// Finds intersections (overlaps) of the range with second one.
        /// </summary>
        /// <param name="r">Second angle range to find overlaps with.</param>
        /// <returns>Collection (max 2 items) of angle ranges which represent overlaps of two initial ranges.</returns>
        /// <remarks>
        /// The idea of this method is based on solution provided here:
        /// <see href="https://stackoverflow.com/questions/48984436/finding-the-intersections-between-two-angle-ranges-segments"/>.
        /// </remarks>
        public ICollection<AngleRange> Overlaps(AngleRange r)
        {
            List<AngleRange> ranges = new List<AngleRange>();

            double aStart = Start;
            double aSweep = Range;
            double bStart = r.Start;
            double bSweep = r.Range;

            double greaterAngle;
            double greaterSweep;
            double originAngle;
            double originSweep;
            if (aStart < bStart)
            {
                originAngle = aStart;
                originSweep = aSweep;
                greaterSweep = bSweep;
                greaterAngle = bStart;
            }
            else
            {
                originAngle = bStart;
                originSweep = bSweep;
                greaterSweep = aSweep;
                greaterAngle = aStart;
            }
            double greaterAngleRel = greaterAngle - originAngle;
            if (greaterAngleRel < originSweep)
            {
                ranges.Add(new AngleRange(greaterAngle, Math.Min(greaterSweep, originSweep - greaterAngleRel)));
            }
            double rouno = greaterAngleRel + greaterSweep;
            if (rouno > 360)
            {
                ranges.Add(new AngleRange(originAngle, Math.Min(rouno - 360, originSweep)));
            }

            return ranges;
        }
    }

    #endregion AngleRange
}
