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

        /// <summary>
        /// Geographical location
        /// </summary>
        public CrdsGeographical Location { get; set; }

        public SolarEclipseLocalCircumstancesContactPoint PartialBegin { get; set; }
        public SolarEclipseLocalCircumstancesContactPoint TotalBegin { get; set; }
        public SolarEclipseLocalCircumstancesContactPoint Maximum { get; set; }
        public SolarEclipseLocalCircumstancesContactPoint TotalEnd { get; set; }
        public SolarEclipseLocalCircumstancesContactPoint PartialEnd { get; set; }

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
