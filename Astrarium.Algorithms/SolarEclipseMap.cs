using System.Collections.Generic;
using System.Linq;

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
        public List<CrdsGeographical>[] TotalPath { get; } = new[] { new List<CrdsGeographical>(), new List<CrdsGeographical>() };

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
        public List<CrdsGeographical> PenumbraNorthernLimit { get; } = new List<CrdsGeographical>();

        /// <summary>
        /// Defines southern visibility limit of an eclipse. 
        /// Can be empty if southern limit does not exist (southern edge of penumbra does not cross the Earth). 
        /// </summary>
        public List<CrdsGeographical> PenumbraSouthernLimit { get; } = new List<CrdsGeographical>();

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
        /// Northernmost point and instant where the solar eclipse starts to be observable
        /// </summary>
        public SolarEclipsePoint PN1 { get; set; }

        /// <summary>
        /// Southernmost point and instant where the solar eclipse starts to be observable
        /// </summary>
        public SolarEclipsePoint PS1 { get; set; }

        /// <summary>
        /// Northernmost point and instant where the solar eclipse ends to be observable
        /// </summary>
        public SolarEclipsePoint PN2 { get; set; }

        /// <summary>
        /// Southernmost point and instant where the solar eclipse ends to be observable
        /// </summary>
        public SolarEclipsePoint PS2 { get; set; }
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
}
