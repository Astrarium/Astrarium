using System;

namespace Astrarium.Algorithms
{
    /// <summary>
    /// Represents set of Besselian elements of solar eclipse,
    /// valid for the time instant
    /// </summary>
    public class InstantBesselianElements
    {
        /// <summary>
        /// Instant of the elements
        /// </summary>
        public double JulianDay { get; set; }

        /// <summary>
        /// DeltaT value (difference between Dynamical and Universal Times).
        /// </summary>
        public double DeltaT { get; set; }

        /// <summary>
        /// X-coordinate of projection of Moon shadow on fundamental plane.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Y-coordinate of projection of Moon shadow on fundamental plane.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Radius of penumbral cone projection on fundamental plane, in Earth radii.
        /// </summary>
        public double L1 { get; set; }

        /// <summary>
        /// Radius of umbral cone projection on fundamental plane, in Earth radii.
        /// </summary>
        public double L2 { get; set; }

        /// <summary>
        /// Declination of Moon shadow vector, expressed in degrees.
        /// </summary>
        public double D { get; set; }

        /// <summary>
        /// Hour angle of Moon shadow vector, expressed in degrees.
        /// </summary>
        public double Mu { get; set; }

        /// <summary>
        /// Tangent of the penumbral cone angle.
        /// </summary>
        public double TanF1 { get; set; }

        /// <summary>
        /// Tangent of the umbral cone angle.
        /// </summary>
        public double TanF2 { get; set; }

        /// <summary>
        /// Derivative of X.
        /// </summary>
        public double dX { get; set; }

        /// <summary>
        /// Derivative of Y.
        /// </summary>
        public double dY { get; set; }
    }
}