using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Algorithms
{
    public class SolarEclipseLocalCircumstances
    {
        public double JulianDayMax { get; set; }

        public double SunAltMax { get; set; }
        public double MaxMagnitude { get; set; }
        public double MoonToSunDiameterRatio { get; set; }

        public double JulianDayPartialBegin { get; set; }
        public double SunAltPartialBegin { get; set; }

        public double JulianDayPartialEnd { get; set; }
        public double SunAltPartialEnd { get; set; }

        public double JulianDayTotalBegin { get; set; }
        public double SunAltTotalBegin { get; set; }

        public double JulianDayTotalEnd { get; set; }
        public double SunAltTotalEnd { get; set; }

        public double TotalDuration => JulianDayTotalEnd - JulianDayTotalBegin;
        public double PartialDuration => JulianDayPartialEnd - JulianDayPartialBegin;

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
                    .AppendLine($"Total Dur = {TimeToString(TotalDuration)}")
                    .AppendLine($"Partial Dur = {TimeToString(PartialDuration)}")
                    .ToString();
        }
    }
}
