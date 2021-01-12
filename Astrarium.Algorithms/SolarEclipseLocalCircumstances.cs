using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Algorithms
{
    /// <summary>
    /// Contains details of local circumstances of the solar eclipse for given place.
    /// </summary>
    public class SolarEclipseLocalCircumstances
    {
        /// <summary>
        /// Instant of maximum eclipse for the current point, in Julian days.
        /// </summary>
        public double JulianDayMax { get; set; }

        /// <summary>
        /// Altitude of the Sun at eclipse maximum, in degrees.
        /// </summary>
        public double SunAltMax { get; set; }

        /// <summary>
        /// Maximal eclipse magnitude.
        /// </summary>
        public double MaxMagnitude { get; set; }

        /// <summary>
        /// Moon/Sun visible diameters ratio.
        /// </summary>
        public double MoonToSunDiameterRatio { get; set; }

        /// <summary>
        /// Total path width, in kilometers, for total or annular phases only.
        /// </summary>
        public double PathWidth { get; set; }

        /// <summary>
        /// Instant of beginning of partial phase for the current place, in Julian days.
        /// </summary>
        public double JulianDayPartialBegin { get; set; }

        /// <summary>
        /// Altitude of the Sun at the beginning of partial phase for the current place, in degrees.
        /// </summary>
        public double SunAltPartialBegin { get; set; }

        /// <summary>
        /// Instant of end of partial phase for the current place, in Julian days.
        /// </summary>
        public double JulianDayPartialEnd { get; set; }

        /// <summary>
        /// Altitude of the Sun at the end of partial phase for the current place, in degrees.
        /// </summary>
        public double SunAltPartialEnd { get; set; }

        /// <summary>
        /// Instant of beginning of total phase for the current place, in Julian days.
        /// </summary>
        public double JulianDayTotalBegin { get; set; }

        /// <summary>
        /// Altitude of the Sun at beginning of the total phase for the current place, in degrees.
        /// </summary>
        public double SunAltTotalBegin { get; set; }

        /// <summary>
        /// Instant of beginning of total phase for the current place, in Julian days.
        /// </summary>
        public double JulianDayTotalEnd { get; set; }

        /// <summary>
        /// Altitude of the Sun at end of the total phase for the current place, in degrees.
        /// </summary>
        public double SunAltTotalEnd { get; set; }

        /// <summary>
        /// Duration of total phase, in fractions of day.
        /// </summary>
        public double TotalDuration => JulianDayTotalEnd - JulianDayTotalBegin;

        /// <summary>
        /// Duration of partial phase, in fractions of day.
        /// </summary>
        public double PartialDuration => JulianDayPartialEnd - JulianDayPartialBegin;

        /// <inheritdoc/>
        public override string ToString()
        {
            return
                new StringBuilder()
                    .AppendLine($"Partial Begin = {JdToString(JulianDayPartialBegin)} / ({SunAltPartialBegin} deg)")
                    .AppendLine($"Total Begin = {JdToString(JulianDayTotalBegin)} / ({SunAltTotalBegin} deg)")
                    .AppendLine($"Max = {JdToString(JulianDayMax)} / ({SunAltMax} deg)")
                    .AppendLine($"Total End = {JdToString(JulianDayTotalEnd)} / ({SunAltTotalEnd} deg)")
                    .AppendLine($"Partial End = {JdToString(JulianDayPartialEnd)} / ({SunAltPartialEnd} deg)")
                    .AppendLine($"Max Mag = {MaxMagnitude}")
                    .AppendLine($"Moon/Sun size ratio = {MoonToSunDiameterRatio}")
                    .AppendLine($"Path Width = {PathWidth} km")
                    .AppendLine($"Total Dur = {TimeToString(TotalDuration)}")
                    .AppendLine($"Partial Dur = {TimeToString(PartialDuration)}")
                    .ToString();
        }

        private string JdToString(double jd)
        {
            if (double.IsNaN(jd) || jd == 0)
                return "-";
            else
            {
                var date = new Date(jd, 0);
                var culture = CultureInfo.InvariantCulture;
                string month = culture.DateTimeFormat.GetMonthName(date.Month);
                int day = (int)date.Day;
                int year = date.Year;
                return string.Format($"{day} {month} {year} {TimeToString(date.Day - day)} UT");
            }
        }

        private string TimeToString(double time)
        {
            if (double.IsNaN(time) || time == 0)
                return "-";
            else
            {
                var ts = TimeSpan.FromDays(time);
                return string.Format($"{ts:hh\\:mm\\:ss}");
            }
        }
    }
}
