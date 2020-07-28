namespace Astrarium.Algorithms
{
    /// <summary>
    /// Represents set of Besselian elements of solar eclipse,
    /// valid for the time instant
    /// </summary>
    internal class InstantBesselianElements
    {
        /// <summary>
        /// X-coordinate of projection of Moon shadow on fundamental plane)
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Y-coordinate of projection of Moon shadow on fundamental plane)
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Radius of penumbral cone projection on fundamental plane, in Earth radii 
        /// </summary>
        public double L1 { get; set; }

        /// <summary>
        /// Radius of umbral cone projection on fundamental plane, in Earth radii
        /// </summary>
        public double L2 { get; set; }

        /// <summary>
        /// Declination of Moon shadow vector, expressed in degrees
        /// </summary>
        public double D { get; set; }

        /// <summary>
        /// Hour angle of Moon shadow vector, expressed in degrees
        /// </summary>
        public double Mu { get; set; }

        /// <summary>
        /// Inclination of Moon shadow track with respect to Earth equator, in degrees.
        /// 0 value means track path is parallel to equator.
        /// </summary>
        public double Inc { get; set; }
    }
}