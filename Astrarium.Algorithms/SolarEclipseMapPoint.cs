namespace Astrarium.Algorithms
{
    /// <summary>
    /// Defines point on the solar eclipse map.
    /// </summary>
    public class SolarEclipseMapPoint : CrdsGeographical
    {
        /// <summary>
        /// Julian day
        /// </summary>
        public double JulianDay { get; set; }

        /// <summary>
        /// Creates new point with Julian Day value and coordinates
        /// </summary>
        /// <param name="jd">Julian Day value</param>
        /// <param name="c">Coordinates of the point</param>
        public SolarEclipseMapPoint(double jd, CrdsGeographical c) : base(c)
        {
            JulianDay = jd;
        }
    }
}
