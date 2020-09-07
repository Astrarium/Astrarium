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
        /// Points of total eclipse path
        /// </summary>
        public List<CrdsGeographical>[] TotalPath { get; } = new[] { new List<CrdsGeographical>(), new List<CrdsGeographical>() };



        public List<CrdsGeographical>[] UmbraNorthernLimit { get; } = new[] { new List<CrdsGeographical>(), new List<CrdsGeographical>() };
        public List<CrdsGeographical>[] UmbraSouthernLimit { get; } = new[] { new List<CrdsGeographical>(), new List<CrdsGeographical>() };
    


        public List<CrdsGeographical>[] RiseSetCurve { get; } = new[] { new List<CrdsGeographical>(), new List<CrdsGeographical>() };
        
        
        public List<CrdsGeographical> PenumbraNorthernLimit { get; } = new List<CrdsGeographical>();
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

    public class SolarEclipsePoint
    {
        public double JulianDay { get; set; }
        public CrdsGeographical Coordinates { get; set; }

        public SolarEclipsePoint(double jd, CrdsGeographical c)
        {
            JulianDay = jd;
            Coordinates = c;
        }
    } 
}
