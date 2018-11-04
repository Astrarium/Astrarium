using System;
using System.Collections.Generic;
using System.Text;

namespace ADK
{
    /// <summary>
    /// Represents a pair of equatorial coordinates.
    /// </summary>
    public class CrdsEquatorial
    {
        /// <summary>
        /// Right Ascension, in decimal degrees, from 0 to 360.
        /// </summary>
        public double Alpha { get; set; }

        /// <summary>
        /// Declination, in decimal degrees, from -90 to +90. Positive towards north of the celestial equator, negative towards south.
        /// </summary>
        public double Delta { get; set; }

        /// <summary>
        /// Creates a pair of equatorial coordinates with empty values.
        /// </summary>
        public CrdsEquatorial() { }

        /// <summary>
        /// Creates a pair of equatorial coordinates with provided values of Right Ascension and Declination.
        /// </summary>
        /// <param name="alpha">Right Ascension value, in decimal degrees.</param>
        /// <param name="delta">Declination value, in decimal degrees.</param>
        public CrdsEquatorial(double alpha, double delta)
        {
            Alpha = alpha;
            Delta = delta;
        }

        /// <summary>
        /// Creates a pair of equatorial coordinates with provided values of Right Ascension and Declination.
        /// </summary>
        /// <param name="alpha">Right Ascension value, expressed in hours, minutes and seconds.</param>
        /// <param name="delta">Declination value, expressed in degrees, minutes and seconds.</param>
        public CrdsEquatorial(HMS alpha, DMS delta)
        {
            Alpha = alpha.ToDecimalAngle();
            Delta = delta.ToDecimalAngle();
        }

        public override string ToString()
        {
            return $"RA: {new HMS(Alpha)}; Dec:{new DMS(Delta)}";
        }
    }
}
