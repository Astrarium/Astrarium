using System.Collections.Generic;
using System.Linq;

namespace Astrarium.Algorithms
{
    public class SolarEclipseCurves
    {
        public List<CrdsGeographical> UmbraPath { get; } = new List<CrdsGeographical>();
        public List<CrdsGeographical> UmbraNorthernLimit { get; } = new List<CrdsGeographical>();
        public List<CrdsGeographical> UmbraSouthernLimit { get; } = new List<CrdsGeographical>();

        public List<CrdsGeographical>[] RiseSetCurve { get; } = new[] { new List<CrdsGeographical>(), new List<CrdsGeographical>() };
        public List<CrdsGeographical> PenumbraNorthernLimit { get; } = new List<CrdsGeographical>();
        public List<CrdsGeographical> PenumbraSouthernLimit { get; } = new List<CrdsGeographical>();
    
        /// <summary>
        /// First external contact, 
        /// Instant when outer edge of penumbra enters Earth surface first time.
        /// Can not be null.
        /// </summary>
        public EclipsePoint P1 { get; set; }

        /// <summary>
        /// First internal contact,
        /// Instant when penumbra outline enters Earth outline.
        /// Can be null.
        /// </summary>
        public EclipsePoint P2 { get; set; }

        /// <summary>
        /// Last internal contact,
        /// Instant when penumbra outline exits Earth outline.
        /// Can be null.
        /// </summary>
        public EclipsePoint P3 { get; set; }

        /// <summary>
        /// Last external contact, 
        /// Instant when outer edge of penumbra exits Earth surface last time.
        /// Can not be null.
        /// </summary>
        public EclipsePoint P4 { get; set; }
    }

    public class EclipsePoint
    {
        public CrdsGeographical Coordinates { get; set; }
        public double JulianDay { get; set; }
    } 
}
