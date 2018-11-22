namespace ADK
{
    /// <summary>
    /// Describes appearance of Saturn rings
    /// </summary>
    public struct RingsAppearance
    {
        /// <summary>
        /// Major axis of the outer edge of the outer ring, in arcseconds 
        /// </summary>
        public double a { get; set; }

        /// <summary>
        /// Minor axis of the outer edge of the outer ring, in arcseconds 
        /// </summary>
        public double b { get; set; }

        /// <summary>
        /// Saturnicentric latitude of the Sun referred to the plane of rings
        /// </summary>
        public double B { get; set; }

        /// <summary>
        /// Difference between Saturnicentri longitudes of the Sun and the Earth, in degrees
        /// </summary>
        public double DeltaU { get; set; }

        /// <summary>
        /// Position angle of the north pole of rotation of the Saturn.
        /// Measured counter-clockwise from direction to celestial North pole.
        /// </summary>
        public double P { get; set; }

        /// <summary>
        /// Outer ring factors.
        /// Should be multiplied to <see cref="a"/> or <see cref="a"/> axis size 
        /// to determine visual size of the outer ring.
        /// </summary>
        public static readonly float[] OuterRing = new float[] { 1, 0.8801f };

        /// <summary>
        /// Inner ring factors.
        /// Should be multiplied to <see cref="a"/> or <see cref="a"/> axis size 
        /// to determine visual size of the inner ring.
        /// </summary>
        public static readonly float[] InnerRing = new float[] { 0.8599f, 0.6650f };

        /// <summary>
        /// Dusky ring factors.
        /// Should be multiplied to <see cref="a"/> or <see cref="a"/> axis size 
        /// to determine visual size of the dusky ring.
        /// </summary>
        public static readonly float[] DuskyRing = new float[] { 0.6650f, 0.5486f };
    }
}
