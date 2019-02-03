namespace ADK
{
    /// <summary>
    /// Represents details of Earth shadow projected on the celestial sphere
    /// </summary>
    public class ShadowAppearance
    {
        /// <summary>
        /// Penumbra radius in Earth radii
        /// </summary>
        public double PenumbraRadius { get; private set; }
            
        /// <summary>
        /// Umbra radius in Earth radii
        /// </summary>
        public double UmbraRadius { get; private set; }

        public double Ratio
        {
            get
            {
                return PenumbraRadius / UmbraRadius;
            }
        }

        /// <summary>
        /// Creates new instance of ShadowAppearance.
        /// </summary>
        /// <param name="u">Radius of the penumbral cone in the fundamental plane, 
        /// in units of the Earth's equatorial radius
        /// </param>
        /// <remarks>Formulae in constructor are from AA(II), p. 382.</remarks>
        public ShadowAppearance(double u)
        {
            PenumbraRadius = 1.2848 + u;
            UmbraRadius = 0.7403 - u;
        }
    }
}
