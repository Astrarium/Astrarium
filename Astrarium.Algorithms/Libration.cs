namespace Astrarium.Algorithms
{
    /// <summary>
    /// Represents libration values for the Moon
    /// </summary>
    public struct Libration
    {
        /// <summary>
        /// Libration in longitude, in degrees.
        /// Positive to east, negative to west.
        /// </summary>
        public double l { get; set; }

        /// <summary>
        /// Libration in latitude, in degrees.
        /// Positive to south, negative to north.
        /// </summary>
        public double b { get; set; }
    }
}
