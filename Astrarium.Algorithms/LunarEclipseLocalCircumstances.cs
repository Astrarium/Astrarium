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

            // Parallactic angle
            // Calculation is based on formula 7, page 26, "Elements of Solar Eclipses" by J.Meeus 
            double q = ToDegrees(Asin(Cos(ToRadians(g.Latitude)) * Sin(ToRadians(H)) / Cos(ToRadians(h.Altitude))));

            JulianDay = e.JulianDay;
            LunarAltitude = h.Altitude;
            QAngle = g.Latitude >= 0 ? q : 180 - q;
            X = e.X;
            Y = e.Y;
            F1 = e.F1;
            F2 = e.F2;
            F3 = e.F3;
        }
    }
}
