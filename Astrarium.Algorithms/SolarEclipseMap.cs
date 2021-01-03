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
        public IList<CrdsGeographical> TotalPath { get; set; } = new CrdsGeographical[0];

        /// <summary>
        /// Defines northern visibility limit of a total (or annular) eclipse.
        /// Can be empty (if the eclipse is partial one, or there is no northern limit exist).
        /// Can be divided into two segments, if the line crosses circumpolar regions. 
        /// </summary>
        public IList<CrdsGeographical>[] UmbraNorthernLimit { get; } = new[] { new List<CrdsGeographical>(), new List<CrdsGeographical>() };

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
        public IList<CrdsGeographical>[] RiseSetCurve { get; } = new[] { new List<CrdsGeographical>(), new List<CrdsGeographical>() };

        /// <summary>
        /// Defines northern visibility limit of an eclipse. 
        /// Can be empty if northern limit does not exist (northern edge of penumbra does not cross the Earth). 
        /// </summary>
        public IList<CrdsGeographical> PenumbraNorthernLimit { get; set; } = new List<CrdsGeographical>();

        /// <summary>
        /// Defines southern visibility limit of an eclipse. 
        /// Can be empty if southern limit does not exist (southern edge of penumbra does not cross the Earth). 
        /// </summary>
        public IList<CrdsGeographical> PenumbraSouthernLimit { get; set; } = new List<CrdsGeographical>();

        /// <summary>
        /// First external contact.
        /// Instant and coordinates of first external tangency of Penumbra with Earth's limb (Partial Eclipse Begins).
        /// </summary>
        public SolarEclipseMapPoint P1 { get; set; }

        /// <summary>
        /// First internal contact.
        /// Instant and coordinates of first internal tangency of Penumbra with Earth's limb.
        /// Can be null.
        /// </summary>
        public SolarEclipseMapPoint P2 { get; set; }

        /// <summary>
        /// Last internal contact.
        /// Instant and coordinates of last internal tangency of Penumbra with Earth's limb.
        /// Can be null.
        /// </summary>
        public SolarEclipseMapPoint P3 { get; set; }

        /// <summary>
        /// Last external contact.
        /// Instant and coordinates of last external tangency of Penumbra with Earth's limb (Partial Eclipse Ends).
        /// </summary>
        public SolarEclipseMapPoint P4 { get; set; }

        /// <summary>
        /// Instant and coordinates of start of total phase (first contact of umbra center with Earth) 
        /// </summary>
        public SolarEclipseMapPoint C1 { get; set; }

        /// <summary>
        /// Instant and coordinates of end of total phase (last contact of umbra center with Earth) 
        /// </summary>
        public SolarEclipseMapPoint C2 { get; set; }

        /// <summary>
        /// Instant and coordinates of eclipse maximum
        /// </summary>
        public SolarEclipseMapPoint Max { get; set; }
    }
}
