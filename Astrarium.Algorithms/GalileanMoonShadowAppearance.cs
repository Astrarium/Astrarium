namespace Astrarium.Algorithms
{
    /// <summary>
    /// Represents appearance details of Galilean moon shadow 
    /// projected on Jupiter or another moon surface, 
    /// as seen from the Earth. 
    /// </summary>
    public class GalileanMoonShadowAppearance
    {
        /// <summary>
        /// Visible umbra semidiameter, in seconds of arc
        /// </summary>
        public double Umbra { get; set; }

        /// <summary>
        /// Visible penumbra semidiameter, in seconds of arc
        /// </summary>
        public double Penumbra { get; set; }
    }
}
