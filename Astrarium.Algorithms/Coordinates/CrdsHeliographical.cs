using System;

namespace Astrarium.Algorithms
{
    public class CrdsHeliographical
    {
        /// <summary>
        /// Heliographical latitude, in degrees
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        ///  Heliographical longitude, in degrees
        /// </summary>
        public double Longitude { get; set; }

        public override string ToString()
        {
            return String.Format("{0}{1} {2}{3}",
                Math.Abs(Latitude), (Latitude < 0 ? "S" : "N"),
                Math.Abs(Longitude), (Longitude < 0 ? "W" : "E"));
        }
    }
}
