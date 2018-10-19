using System;
using System.Collections.Generic;
using System.Text;

namespace ADK
{
    /// <summary>
    /// Represents a pair of the local horizontal coordinates.
    /// </summary>
    public class CrdsHorizontal
    {
        /// <summary>
        /// Azimuth, in degrees. Measured westwards from the south.
        /// </summary>
        public double Azimuth { get; set; }

        /// <summary>
        /// Altitude, in degrees. Positive above the horizon, negative below.
        /// </summary>
        public double Altitude { get; set; }

        /// <summary>
        /// Creates a pair of horizontal coordinates with empty values.
        /// </summary>
        public CrdsHorizontal() { }

        /// <summary>
        /// Creates a pair of equatorial coordinates with provided values of Right Ascension and Declination.
        /// </summary>
        /// <param name="azimuth">Azimuth, in degrees. Measured westwards from the south.</param>
        /// <param name="altitude">Altitude, in degrees. Positive above the horizon, negative below.</param>
        public CrdsHorizontal(double azimuth, double altitude)
        {
            Azimuth = azimuth;
            Altitude = altitude;
        }

        /// <summary>
        /// Sets horizontal coordinates values.
        /// </summary>
        /// <param name="azimuth">Azimuth, in degrees. Measured westwards from the south.</param>
        /// <param name="altitude">Altitude, in degrees. Positive above the horizon, negative below.</param>
        public void Set(double azimuth, double altitude)
        {
            Azimuth = azimuth;
            Altitude = altitude;
        }
    }
}
