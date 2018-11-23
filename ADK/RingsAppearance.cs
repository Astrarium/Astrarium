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
        /// Gets visible size of Saturn ring edge, in arcseconds.
        /// </summary>
        /// <param name="ring">Ring number. Value should be in range from 0 to 2 (0 = outer ring, 1 = inner ring, 2 = dusky ring).</param>
        /// <param name="edge">Ring edge, outer or inner</param>
        /// <param name="axis">Ring axis, major or minor</param>
        /// <returns>Visible size of Saturn ring edge, in arcseconds.</returns>
        public double GetRingSize(int ring, RingEdge edge, RingAxis axis)
        {
            double ax = axis == RingAxis.Major ? a : b;
            return ax * Rings[ring][(int)edge];
        }

        /// <summary>
        /// Ring factors.
        /// First array index is a ring index (0 = outer ring, 1 = inner ring, 2 = dusky ring).
        /// Seconds array index is a factor value for the corresponding ring edge (0 = outer edge, 1 = inner edge).
        /// </summary>
        public static readonly float[][] Rings = new float[3][]
        {
            /// <summary>
            /// Outer ring factors.
            /// Should be multiplied to <see cref="a"/> or <see cref="b"/> axis size 
            /// to determine visual size of the outer ring.
            /// </summary>
            new float[] { 1, 0.8801f },

            /// <summary>+
            /// Inner ring factors.
            /// Should be multiplied to <see cref="a"/> or <see cref="b"/> axis size 
            /// to determine visual size of the inner ring.
            /// </summary>
            new float[] { 0.8599f, 0.6650f },

            /// <summary>
            /// Dusky ring factors.
            /// Should be multiplied to <see cref="a"/> or <see cref="b"/> axis size 
            /// to determine visual size of the dusky ring.
            /// </summary>
            new float[] { 0.6650f, 0.5486f }
        };
    }

    /// <summary>
    /// Defines Saturn ring axis, major or minor
    /// </summary>
    public enum RingAxis
    {
        /// <summary>
        /// Major axis
        /// </summary>
        Major = 0,

        /// <summary>
        /// Minor axis
        /// </summary>
        Minor = 1
    }

    /// <summary>
    /// Defines Saturn ring edge, outer or inner
    /// </summary>
    public enum RingEdge
    {
        /// <summary>
        /// Outer ring edge
        /// </summary>
        Outer = 0,

        /// <summary>
        /// Inner ring edge
        /// </summary>
        Inner = 1
    }
}
