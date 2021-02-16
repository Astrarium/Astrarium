using static System.Math;
using static Astrarium.Algorithms.Angle;

namespace Astrarium.Algorithms
{
    /// <summary>
    /// Contains details of local circumstances of the lunar eclipse for a given place.
    /// </summary>
    public class LunarEclipseLocalCircumstances
    {
        /// <summary>
        /// Flag indicating the eclipse is invisible from current place.
        /// </summary>
        public bool IsInvisible
        {
            get
            {
                return
                    (PenumbralBegin.LunarAltitude <= 0 &&
                    (PartialBegin == null || PartialBegin.LunarAltitude <= 0) &&
                    (TotalBegin == null || TotalBegin.LunarAltitude <= 0) &&
                     Maximum.LunarAltitude <= 0 &&
                    (PenumbralEnd == null || PenumbralEnd.LunarAltitude <= 0) &&
                    (PartialEnd == null || PartialEnd.LunarAltitude <= 0) &&
                    (TotalEnd == null || TotalEnd.LunarAltitude <= 0));
            }
        }

        /// <summary>
        /// Geographical location.
        /// </summary>
        public CrdsGeographical Location { get; set; }

        /// <summary>
        /// Details for instant of beginning of penumbral phase of lunar eclipse.
        /// </summary>
        public LunarEclipseLocalCircumstancesContactPoint PenumbralBegin { get; set; }

        /// <summary>
        /// Details for instant of beginning of partial phase of lunar eclipse.
        /// </summary>
        public LunarEclipseLocalCircumstancesContactPoint PartialBegin { get; set; }

        /// <summary>
        /// Details for instant of beginning of total phase of lunar eclipse.
        /// </summary>
        public LunarEclipseLocalCircumstancesContactPoint TotalBegin { get; set; }

        /// <summary>
        /// Details for instant of maximum of lunar eclipse.
        /// </summary>
        public LunarEclipseLocalCircumstancesContactPoint Maximum { get; set; }

        /// <summary>
        /// Details for instant of end of total phase of lunar eclipse.
        /// </summary>
        public LunarEclipseLocalCircumstancesContactPoint TotalEnd { get; set; }

        /// <summary>
        /// Details for instant of end of partial phase of lunar eclipse.
        /// </summary>
        public LunarEclipseLocalCircumstancesContactPoint PartialEnd { get; set; }

        /// <summary>
        /// Details for instant of end of penumbral phase of lunar eclipse.
        /// </summary>
        public LunarEclipseLocalCircumstancesContactPoint PenumbralEnd { get; set; }
    }

    /// <summary>
    /// Describes details of instant of lunar eclipse for a given point.
    /// </summary>
    public class LunarEclipseLocalCircumstancesContactPoint
    {
        /// <summary>
        /// Julian day of instant.
        /// </summary>
        public double JulianDay { get; private set; }

        /// <summary>
        /// Local altitude of the Moon at the instant.
        /// </summary>
        public double LunarAltitude { get; private set; }

        /// <summary>
        /// Parallactic angle, measured in degrees, is a difference between
        /// zenithal position angle Z and equatorial position angle P of the Moon center
        /// </summary>
        public double QAngle { get; private set; }

        /// <summary>
        /// Equatorial position angle of the Moon center, measured in degrees
        /// </summary>
        public double PAngle { get; private set; }

        /// <summary>
        /// Zenithal position angle of the Moon center, measured in degrees
        /// </summary>
        public double ZAngle { get; private set; }

        /// <summary>
        /// X-coordinate of center of the Moon in fundamental plane.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Y-coordinate of center of the Moon in fundamental plane.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Earth penumbra radius, in degrees.
        /// </summary>
        public double F1 { get; set; }

        /// <summary>
        /// Earth umbra radius, in degrees.
        /// </summary>
        public double F2 { get; set; }

        /// <summary>
        /// Lunar radius (semidiameter), in degrees.
        /// </summary>
        public double F3 { get; set; }

        /// <summary>
        /// Creates new instance
        /// </summary>
        public LunarEclipseLocalCircumstancesContactPoint(InstantLunarEclipseElements e, CrdsGeographical g)
        {
            CrdsEquatorial eq = new CrdsEquatorial(e.Alpha, e.Delta);

            // Nutation elements
            var nutation = Nutation.NutationElements(e.JulianDay);

            // True obliquity
            var epsilon = Date.TrueObliquity(e.JulianDay, nutation.deltaEpsilon);

            // Greenwich apparent sidereal time 
            double siderealTime = Date.ApparentSiderealTime(e.JulianDay, nutation.deltaPsi, epsilon);

            // Geocenric distance to the Moon, in km
            double dist = 358473400.0 / (e.F3 * 3600);

            // Horizontal parallax of the Moon
            double parallax = LunarEphem.Parallax(dist);

            // Topocentrical equatorial coordinates
            var eqTopo = eq.ToTopocentric(g, siderealTime, parallax);

            // Horizontal coordinates
            var h = eqTopo.ToHorizontal(g, siderealTime);

            // Hour angle
            double H = Coordinates.HourAngle(siderealTime, g.Longitude, eqTopo.Alpha);

            // Position angles

            // Parallactic angle - AA(II), p.98, formula 14.1
            double qAngle = ToDegrees(Atan2(Sin(ToRadians(H)), Tan(ToRadians(g.Latitude)) * Cos(ToRadians(eqTopo.Delta)) - Sin(ToRadians(eqTopo.Delta)) * Cos(ToRadians(H))));

            // Position angle, measured from North to East (CCW)
            double pAngle = To360(ToDegrees(Atan2(e.X, e.Y)));

            // Position angle, measured from Zenith CCW
            double zAngle = To360(pAngle - qAngle);

            JulianDay = e.JulianDay;
            LunarAltitude = h.Altitude;
            QAngle = qAngle;
            PAngle = pAngle;
            ZAngle = zAngle;
            X = e.X;
            Y = e.Y;
            F1 = e.F1;
            F2 = e.F2;
            F3 = e.F3;
        }
    }
}
