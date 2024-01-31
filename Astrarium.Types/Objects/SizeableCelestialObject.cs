namespace Astrarium.Types
{
    public abstract class SizeableCelestialObject : CelestialObject
    {
        /// <summary>
        /// Visible semidiameter, in seconds of arc
        /// </summary>
        public virtual double Semidiameter { get; set; }
    }
}
