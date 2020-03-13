using System;
using System.Collections.Generic;
using System.Text;

namespace Astrarium.Algorithms
{
    /// <summary>
    /// Represents a pair of the galactical coordinates
    /// </summary>
    public class CrdsGalactical
    {
        /// <summary>
        /// Galactic longitude, in degrees.
        /// </summary>
        public double l { get; set; }

        /// <summary>
        /// Galactic latitude, in degrees.
        /// </summary>
        public double b { get; set; }

        public CrdsGalactical() { }

        public CrdsGalactical(double lon, double lat)
        {
            l = lon;
            b = lat;
        }

        public CrdsGalactical(DMS lon, DMS lat)
        {
            l = lon.ToDecimalAngle();
            b = lat.ToDecimalAngle();
        }

        /// <summary>
        /// Sets pair of galactic coordiantes
        /// </summary>
        /// <param name="lon"></param>
        /// <param name="lat"></param>
        public void Set(double lon, double lat)
        {
            l = lon;
            b = lat;
        }
    }
}
