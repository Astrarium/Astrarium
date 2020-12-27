using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Astrarium.Algorithms
{
    /// <summary>
    /// Describes points and curves of solar eclipse map.
    /// </summary>
    /// <remarks>
    /// See explanations of eclipse map curves and points here: http://www.gautschy.ch/~rita/archast/solec/solec.html
    /// </remarks>
    public class SolarEclipseMap
    {
        /// <summary>
        /// Defines points on a cenral line of an eclipse.
        /// Can be empty (if the eclipse is partial one).
        /// Central line of eclipse can be divided into two segments, if the line crosses circumpolar regions. 
        /// </summary>
        public List<CrdsGeographical> TotalPath { get; set; } = new List<CrdsGeographical>();

        /// <summary>
        /// Defines northern visibility limit of a total (or annular) eclipse.
        /// Can be empty (if the eclipse is partial one, or there is no northern limit exist).
        /// Can be divided into two segments, if the line crosses circumpolar regions. 
        /// </summary>
        public List<CrdsGeographical>[] UmbraNorthernLimit { get; } = new[] { new List<CrdsGeographical>(), new List<CrdsGeographical>() };

        /// <summary>
        /// Defines southern visibility limit of a total (or annular) eclipse.
        /// Can be empty (if the eclipse is partial one, or there is no southern limit).
        /// Can be divided into two segments, if the line crosses circumpolar regions. 
        /// </summary>
        public List<CrdsGeographical>[] UmbraSouthernLimit { get; } = new[] { new List<CrdsGeographical>(), new List<CrdsGeographical>() };

        /// <summary>
        /// Defines areas on the Earth where the eclipse is visible on sunrise or sunset.
        /// Points can be joined in one eightlike curve, or can be splitted into 2 closed curves that look like raindrops.
        /// </summary>
        public List<CrdsGeographical>[] RiseSetCurve { get; } = new[] { new List<CrdsGeographical>(), new List<CrdsGeographical>() };

        /// <summary>
        /// Defines northern visibility limit of an eclipse. 
        /// Can be empty if northern limit does not exist (northern edge of penumbra does not cross the Earth). 
        /// </summary>
        public List<CrdsGeographical> PenumbraNorthernLimit { get; set; } = new List<CrdsGeographical>();

        /// <summary>
        /// Defines southern visibility limit of an eclipse. 
        /// Can be empty if southern limit does not exist (southern edge of penumbra does not cross the Earth). 
        /// </summary>
        public List<CrdsGeographical> PenumbraSouthernLimit { get; set; } = new List<CrdsGeographical>();

        /// <summary>
        /// First external contact.
        /// Instant and coordinates of first external tangency of Penumbra with Earth's limb (Partial Eclipse Begins).
        /// </summary>
        public SolarEclipsePoint P1 { get; set; }

        /// <summary>
        /// First internal contact.
        /// Instant and coordinates of first internal tangency of Penumbra with Earth's limb.
        /// Can be null.
        /// </summary>
        public SolarEclipsePoint P2 { get; set; }

        /// <summary>
        /// Last internal contact.
        /// Instant and coordinates of last internal tangency of Penumbra with Earth's limb.
        /// Can be null.
        /// </summary>
        public SolarEclipsePoint P3 { get; set; }

        /// <summary>
        /// Last external contact.
        /// Instant and coordinates of last external tangency of Penumbra with Earth's limb (Partial Eclipse Ends).
        /// </summary>
        public SolarEclipsePoint P4 { get; set; }

        /// <summary>
        /// Instant and coordinates of start of total phase (first contact of umbra center with Earth) 
        /// </summary>
        public SolarEclipsePoint C1 { get; set; }

        /// <summary>
        /// Instant and coordinates of end of total phase (last contact of umbra center with Earth) 
        /// </summary>
        public SolarEclipsePoint C2 { get; set; }

        /// <summary>
        /// Instant and coordinates of eclipse maximum
        /// </summary>
        public SolarEclipsePoint Max { get; set; }
    }

    /// <summary>
    /// Defines point on the solar eclipse map.
    /// </summary>
    public class SolarEclipsePoint
    {
        /// <summary>
        /// Julian day
        /// </summary>
        public double JulianDay { get; set; }

        /// <summary>
        /// Coordinates of the point
        /// </summary>
        public CrdsGeographical Coordinates { get; set; }

        /// <summary>
        /// Creates new point with Julian Day value and coordinates
        /// </summary>
        /// <param name="jd">Julian Day value</param>
        /// <param name="c">Coordinates of the point</param>
        public SolarEclipsePoint(double jd, CrdsGeographical c)
        {
            JulianDay = jd;
            Coordinates = c;
        }
    }

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
