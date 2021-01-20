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
        /// Flag indicating the eclipse is invisible from current point
        /// </summary>
        public bool IsInvisible { get; set; }


        public SolarEclipseLocalCircumstancesContactPoint PartialBegin { get; set; }
        public SolarEclipseLocalCircumstancesContactPoint TotalBegin { get; set; }
        public SolarEclipseLocalCircumstancesContactPoint Maximum { get; set; }
        public SolarEclipseLocalCircumstancesContactPoint TotalEnd { get; set; }
        public SolarEclipseLocalCircumstancesContactPoint PartialEnd { get; set; }


        /// <summary>
        /// Instant of maximum eclipse for the current point, in Julian days.
        /// </summary>
        //public double JulianDayMax { get; set; }

        /// <summary>
        /// Altitude of the Sun at eclipse maximum, in degrees.
        /// </summary>
        //public double SunAltMax { get; set; }

        /// <summary>
        /// Position angle of the point of center of lunar disk, 
        /// measured from the zenith point of the solar limb towards the east,
        /// in degrees, at eclipse maximum.
        /// </summary>
        //public double ZAngleMax { get; set; }
        //public double PAngleMax { get; set; }

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
        //public double JulianDayPartialBegin { get; set; }

        /// <summary>
        /// Altitude of the Sun at the beginning of partial phase for the current place, in degrees.
        /// </summary>
        //public double SunAltPartialBegin { get; set; }

        /// <summary>
        /// Position angle of the point of center of lunar disk, 
        /// measured from the zenith point of the solar limb towards the east,
        /// in degrees, at the beginning of partial phase.
        /// </summary>
        //public double ZAnglePartialBegin { get; set; }
        //public double PAnglePartialBegin { get; set; }

        /// <summary>
        /// Instant of end of partial phase for the current place, in Julian days.
        /// </summary>
        //public double JulianDayPartialEnd { get; set; }

        /// <summary>
        /// Altitude of the Sun at the end of partial phase for the current place, in degrees.
        /// </summary>
        //public double SunAltPartialEnd { get; set; }

        /// <summary>
        /// Position angle of the point of center of lunar disk, 
        /// measured from the zenith point of the solar limb towards the east,
        /// in degrees, at the end of partial phase.
        /// </summary>
        //public double ZAnglePartialEnd { get; set; }
        //public double PAnglePartialEnd { get; set; }

        /// <summary>
        /// Instant of beginning of total phase for the current place, in Julian days.
        /// </summary>
        //public double JulianDayTotalBegin { get; set; }

        /// <summary>
        /// Position angle of the point of center of lunar disk, 
        /// measured from the zenith point of the solar limb towards the east,
        /// in degrees, at the beginning of total phase.
        /// </summary>
        //public double ZAngleTotalBegin { get; set; }
        //public double PAngleTotalBegin { get; set; }

        /// <summary>
        /// Altitude of the Sun at beginning of the total phase for the current place, in degrees.
        /// </summary>
        //public double SunAltTotalBegin { get; set; }

        /// <summary>
        /// Position angle of the point of center of lunar disk, 
        /// measured from the zenith point of the solar limb towards the east,
        /// in degrees, at the end of total phase.
        /// </summary>
        //public double ZAngleTotalEnd { get; set; }
        //public double PAngleTotalEnd { get; set; }

        /// <summary>
        /// Instant of beginning of total phase for the current place, in Julian days.
        /// </summary>
        //public double JulianDayTotalEnd { get; set; }

        /// <summary>
        /// Altitude of the Sun at end of the total phase for the current place, in degrees.
        /// </summary>
        //public double SunAltTotalEnd { get; set; }

        /// <summary>
        /// Duration of total phase, in fractions of day.
        /// </summary>
        public double TotalDuration => TotalBegin != null && TotalEnd != null ? TotalEnd.JulianDay - TotalBegin.JulianDay : 0;

        /// <summary>
        /// Duration of partial phase, in fractions of day.
        /// </summary>
        public double PartialDuration => PartialBegin != null && PartialEnd != null ? PartialEnd.JulianDay - PartialBegin.JulianDay : 0;
    }

    public class SolarEclipseLocalCircumstancesContactPoint
    {
        public double JulianDay { get; private set; }
        public double SolarAltitude { get; private set; }
        public double PAngle { get; private set; }
        public double ZAngle { get; private set; }
        
        /// <summary>
        /// Parallactic angle, in degrees
        /// </summary>
        public double QAngle { get; private set; }

        public SolarEclipseLocalCircumstancesContactPoint(double jd, double solarAlt, double pAngle, double zAngle, double qAngle)
        {
            JulianDay = jd;
            SolarAltitude = solarAlt;
            PAngle = pAngle;
            ZAngle = zAngle;
            QAngle = qAngle;
        }
    }
}
